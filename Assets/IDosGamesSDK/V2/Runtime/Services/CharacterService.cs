using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace IDosGames
{
    /// <summary>
    /// High-level wrapper over <see cref="CharacterAPI"/>. Every successful response patches
    /// <see cref="IDosGamesData"/> (User.State / Config) and fires a typed event so UI can react.
    /// </summary>
    public static class CharacterService
    {
        // ---------- Typed events ----------
        public static event Action<CharacterDefinitions> OnCharacterDefinitionsLoaded;
        public static event Action<Dictionary<string, CharacterModel>> OnUserCharactersLoaded;
        public static event Action<UpgradeStatLevelResponse> OnStatLevelUpgraded;
        public static event Action<UpgradeCharacterLevelResponse> OnCharacterLevelUpgraded;
        public static event Action<string, List<EquipSlotPair>> OnItemsEquipped;
        public static event Action<string, List<string>> OnItemsUnequipped;
        public static event Action OnAllCharactersUnequipped;

        // ---------- Helpers ----------
        private static AuthContext Ctx => AuthenticationService.GetAuthContext();

        private static CharacterRequest CreateBaseRequest() => new()
        {
            UserID = Ctx.UserID,
            ClientSessionTicket = Ctx.ClientSessionTicket,
            BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
            WebAppLink = WebSDK.webAppLink,
        };

        private static string ResolveCharacterID(string characterID)
        {
            var trimmed = (characterID ?? "").Trim();
            return string.IsNullOrEmpty(trimmed) ? DefaultData.Main : trimmed;
        }

        // ===================== Actions =====================

        /// <summary>Loads <c>TitlePublicConfiguration.Character</c> and stores it via <c>TitleConfig.PatchCharacter</c>.</summary>
        public static async Task<OperationResult<CharacterDefinitions>> GetCharacterDefinitions()
        {
            var request = CreateBaseRequest();
            var result = await CharacterAPI.GetCharacterDefinitions(request);
            if (result.Success && result.Data != null)
            {
                IDosGamesData.Config.PatchCharacter(result.Data);
                OnCharacterDefinitionsLoaded?.Invoke(result.Data);
            }
            return result;
        }

        /// <summary>Loads the player's character map and replaces <c>UserData.State.Character</c> via <c>ApplyCharacter</c>.</summary>
        public static async Task<OperationResult<UserCharactersState>> GetUserCharacters()
        {
            var request = CreateBaseRequest();
            var result = await CharacterAPI.GetUserCharacters(request);
            if (result.Success && result.Data != null)
            {
                var characters = result.Data.Characters ?? new Dictionary<string, CharacterModel>();
                IDosGamesData.User.ApplyCharacter(characters);
                OnUserCharactersLoaded?.Invoke(characters);
            }
            return result;
        }

        /// <summary>Upgrades a single stat by one level. Patches the target character and applies the resource op locally.</summary>
        public static async Task<OperationResult<UpgradeStatLevelResponse>> UpgradeStatLevel(string characterID, string statID)
        {
            if (string.IsNullOrWhiteSpace(statID))
            {
                Debug.LogWarning("[CharacterService] UpgradeStatLevel: StatID is required.");
                return OperationResult<UpgradeStatLevelResponse>.Fail("StatID is required");
            }
            if (statID.Contains('.') || statID.Contains('$'))
            {
                Debug.LogWarning($"[CharacterService] UpgradeStatLevel: StatID '{statID}' contains invalid characters.");
                return OperationResult<UpgradeStatLevelResponse>.Fail("StatID contains invalid characters ('.' or '$').");
            }

            var resolvedCharacterID = ResolveCharacterID(characterID);
            var request = CreateBaseRequest();
            request.CharacterID = resolvedCharacterID;
            request.StatID = statID;
            request.RelatedEntityID = $"upgrade_stat_{resolvedCharacterID}_{statID}_{Guid.NewGuid():N}";

            var result = await CharacterAPI.UpgradeStatLevel(request);
            if (result.Success && result.Data != null)
            {
                var data = result.Data;

                IDosGamesData.User.PatchCharacter(data.CharacterID, ch =>
                {
                    ch.StatLevels ??= new Dictionary<string, int>();
                    ch.StatLevels[data.StatID] = data.StatLevel;
                    ch.UpdatedAt = data.ServerTimeUtc;
                });

                if (data.Resources != null)
                    IDosGamesData.User.ApplyResourceOperation(data.Resources, IDosGamesData.Config?.TitlePublicConfiguration?.Item);

                OnStatLevelUpgraded?.Invoke(data);
            }
            return result;
        }

        /// <summary>Raises the character's level by one. Patches the character and applies the resource op locally.</summary>
        public static async Task<OperationResult<UpgradeCharacterLevelResponse>> UpgradeCharacterLevel(string characterID)
        {
            var resolvedCharacterID = ResolveCharacterID(characterID);
            var request = CreateBaseRequest();
            request.CharacterID = resolvedCharacterID;
            request.RelatedEntityID = $"upgrade_char_{resolvedCharacterID}_{Guid.NewGuid():N}";

            var result = await CharacterAPI.UpgradeCharacterLevel(request);
            if (result.Success && result.Data != null)
            {
                var data = result.Data;

                IDosGamesData.User.PatchCharacter(data.CharacterID, ch =>
                {
                    ch.Level = data.NewLevel;
                    ch.UpdatedAt = data.ServerTimeUtc;
                    if (data.NewLevel == 1)
                    {
                        ch.StatLevels ??= new Dictionary<string, int>();
                        ch.Equipment ??= new Dictionary<string, EquippedItem>();
                    }
                });

                if (data.Resources != null)
                    IDosGamesData.User.ApplyResourceOperation(data.Resources, IDosGamesData.Config?.TitlePublicConfiguration?.Item);

                OnCharacterLevelUpgraded?.Invoke(data);
            }
            return result;
        }

        /// <summary>
        /// Equips one or more items. Locally mirrors the server change in
        /// <c>UserData.State.Character.Characters[id].Equipment</c> and
        /// <c>UserData.State.InventoryV2.UnstackableItems[instId].EquippedSlot</c>.
        /// If a pair has no <see cref="EquipSlotPair.ItemInstanceID"/>, the local mirror for that pair is skipped;
        /// callers should re-fetch via <see cref="GetUserCharacters"/> to discover the chosen instance.
        /// </summary>
        public static async Task<OperationResult<SuccessResponse>> EquipItems(string characterID, List<EquipSlotPair> itemsToEquip)
        {
            if (itemsToEquip == null || itemsToEquip.Count == 0)
            {
                Debug.LogWarning("[CharacterService] EquipItems: items list is empty.");
                return OperationResult<SuccessResponse>.Fail("Items list is empty");
            }

            var sanitized = itemsToEquip
                .Where(x => x != null
                            && !string.IsNullOrWhiteSpace(x.SlotID)
                            && (!string.IsNullOrWhiteSpace(x.ItemInstanceID) || !string.IsNullOrWhiteSpace(x.ItemID)))
                .ToList();

            if (sanitized.Count == 0)
            {
                Debug.LogWarning("[CharacterService] EquipItems: items list is empty after sanitisation.");
                return OperationResult<SuccessResponse>.Fail("Items list is empty after validation");
            }

            var resolvedCharacterID = ResolveCharacterID(characterID);
            var request = CreateBaseRequest();
            request.CharacterID = resolvedCharacterID;
            request.ItemsToEquip = sanitized;

            var result = await CharacterAPI.EquipItems(request);
            if (result.Success)
            {
                ApplyEquipChangesLocally(resolvedCharacterID, sanitized);
                OnItemsEquipped?.Invoke(resolvedCharacterID, sanitized);
            }
            return result;
        }

        /// <summary>Unequips the given slots and locally mirrors the result.</summary>
        public static async Task<OperationResult<SuccessResponse>> UnequipItems(string characterID, List<string> slotIDs)
        {
            if (slotIDs == null || slotIDs.Count == 0)
                return OperationResult<SuccessResponse>.Ok(new SuccessResponse());

            var sanitized = slotIDs
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (sanitized.Count == 0)
                return OperationResult<SuccessResponse>.Ok(new SuccessResponse());

            var resolvedCharacterID = ResolveCharacterID(characterID);
            var request = CreateBaseRequest();
            request.CharacterID = resolvedCharacterID;
            request.UnequipSlotIDs = sanitized;

            var result = await CharacterAPI.UnequipItems(request);
            if (result.Success)
            {
                ApplyUnequipChangesLocally(resolvedCharacterID, sanitized);
                OnItemsUnequipped?.Invoke(resolvedCharacterID, sanitized);
            }
            return result;
        }

        /// <summary>Wipes equipment from every character of the player and locally resets every affected instance.</summary>
        public static async Task<OperationResult<SuccessResponse>> UnequipAllCharacters()
        {
            var request = CreateBaseRequest();
            var result = await CharacterAPI.UnequipAllCharacters(request);
            if (result.Success)
            {
                ApplyUnequipAllLocally();
                OnAllCharactersUnequipped?.Invoke();
            }
            return result;
        }

        // ===================== Local mirrors =====================

        private static void ApplyEquipChangesLocally(string characterID, List<EquipSlotPair> pairs)
        {
            var user = IDosGamesData.User;
            var state = user.State;
            if (state == null) return;

            var now = DateTime.UtcNow;
            var characters = state.Character?.Characters;
            CharacterModel character = null;
            characters?.TryGetValue(characterID, out character);

            foreach (var pair in pairs)
            {
                // We can only mirror when we know which instance was equipped.
                if (string.IsNullOrWhiteSpace(pair.ItemInstanceID))
                    continue;

                // Discover the catalog id from current inventory.
                string catalogID = null;
                string itemID = pair.ItemID;
                if (state.InventoryV2?.UnstackableItems != null
                    && state.InventoryV2.UnstackableItems.TryGetValue(pair.ItemInstanceID, out var inst)
                    && inst != null)
                {
                    itemID = inst.ItemID ?? itemID;
                    catalogID = inst.CatalogID;
                }

                // 1. Reset previously equipped instance in the same slot (if any).
                if (character?.Equipment != null
                    && character.Equipment.TryGetValue(pair.SlotID, out var oldEquipped)
                    && oldEquipped != null
                    && !string.IsNullOrWhiteSpace(oldEquipped.ItemInstanceID))
                {
                    user.PatchUnstackableItemEquippedSlot(oldEquipped.ItemInstanceID, null);
                }

                // 2. Write the new equipped item on the character.
                var equipped = new EquippedItem
                {
                    ItemID = itemID,
                    ItemInstanceID = pair.ItemInstanceID,
                    CatalogID = catalogID,
                    EquippedAt = now,
                };
                user.PatchCharacterEquipment(characterID, pair.SlotID, equipped);

                // 3. Stamp EquippedSlot on the instance (source of truth).
                user.PatchUnstackableItemEquippedSlot(pair.ItemInstanceID, new EquipmentSlot
                {
                    CharacterID = characterID,
                    SlotID = pair.SlotID,
                });
            }
        }

        private static void ApplyUnequipChangesLocally(string characterID, List<string> slotIDs)
        {
            var state = IDosGamesData.User.State;
            if (state?.Character?.Characters == null
                || !state.Character.Characters.TryGetValue(characterID, out var character)
                || character?.Equipment == null
                || character.Equipment.Count == 0)
            {
                return;
            }

            foreach (var slotID in slotIDs)
            {
                if (!character.Equipment.TryGetValue(slotID, out var equipped)
                    || equipped == null
                    || string.IsNullOrWhiteSpace(equipped.ItemInstanceID))
                {
                    continue;
                }

                IDosGamesData.User.PatchCharacterEquipment(characterID, slotID, null);
                IDosGamesData.User.PatchUnstackableItemEquippedSlot(equipped.ItemInstanceID, null);
            }
        }

        private static void ApplyUnequipAllLocally()
        {
            var user = IDosGamesData.User;
            var characters = user.State?.Character?.Characters;
            if (characters == null || characters.Count == 0) return;

            foreach (var charKv in characters)
            {
                var charID = charKv.Key;
                var charModel = charKv.Value;
                if (charModel?.Equipment == null || charModel.Equipment.Count == 0) continue;

                // Snapshot ids before mutating.
                var instanceIDs = charModel.Equipment.Values
                    .Where(e => e != null && !string.IsNullOrWhiteSpace(e.ItemInstanceID))
                    .Select(e => e.ItemInstanceID)
                    .ToList();
                var slotIDs = charModel.Equipment.Keys.ToList();

                foreach (var slotID in slotIDs)
                    user.PatchCharacterEquipment(charID, slotID, null);

                foreach (var instID in instanceIDs)
                    user.PatchUnstackableItemEquippedSlot(instID, null);
            }
        }
    }
}
