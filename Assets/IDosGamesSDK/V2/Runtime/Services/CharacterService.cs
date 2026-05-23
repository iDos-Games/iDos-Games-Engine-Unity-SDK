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
        public static event Action<EquipItemsResponse> OnItemsEquipped;
        public static event Action<string, List<string>> OnItemsUnequipped;
        public static event Action OnAllCharactersUnequipped;
        public static event Action<UnlockCharacterResponse> OnCharacterUnlocked;

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
        /// Equips one or more items. On success, mirrors the authoritative server response
        /// (<see cref="EquipItemsResponse.Equipment"/>, <see cref="EquipItemsResponse.ReplacedInstanceIDs"/>,
        /// <see cref="EquipItemsResponse.Power"/>) into
        /// <c>UserData.State.Character.Characters[id]</c> and
        /// <c>UserData.State.InventoryV2.UnstackableItems[instId].EquippedSlot</c>.
        /// </summary>
        public static async Task<OperationResult<EquipItemsResponse>> EquipItems(string characterID, List<EquipSlotPair> itemsToEquip)
        {
            if (itemsToEquip == null || itemsToEquip.Count == 0)
            {
                Debug.LogWarning("[CharacterService] EquipItems: items list is empty.");
                return OperationResult<EquipItemsResponse>.Fail("Items list is empty");
            }

            var sanitized = itemsToEquip
                .Where(x => x != null
                            && !string.IsNullOrWhiteSpace(x.SlotID)
                            && (!string.IsNullOrWhiteSpace(x.ItemInstanceID) || !string.IsNullOrWhiteSpace(x.ItemID)))
                .ToList();

            if (sanitized.Count == 0)
            {
                Debug.LogWarning("[CharacterService] EquipItems: items list is empty after sanitisation.");
                return OperationResult<EquipItemsResponse>.Fail("Items list is empty after validation");
            }

            var resolvedCharacterID = ResolveCharacterID(characterID);
            var request = CreateBaseRequest();
            request.CharacterID = resolvedCharacterID;
            request.ItemsToEquip = sanitized;

            var result = await CharacterAPI.EquipItems(request);
            if (result.Success && result.Data != null)
            {
                ApplyEquipChangesLocally(result.Data);
                OnItemsEquipped?.Invoke(result.Data);
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

        /// <summary>
        /// Unlocks a character for the player. Patches <c>UserData.State.Character.Characters[id]</c>
        /// with a new entry (level 0, empty stats/equipment) and applies the resource op locally.
        /// </summary>
        public static async Task<OperationResult<UnlockCharacterResponse>> UnlockCharacter(string characterID)
        {
            if (string.IsNullOrWhiteSpace(characterID))
            {
                Debug.LogWarning("[CharacterService] UnlockCharacter: CharacterID is required.");
                return OperationResult<UnlockCharacterResponse>.Fail("CharacterID is required");
            }

            var resolvedCharacterID = characterID.Trim();
            var request = CreateBaseRequest();
            request.CharacterID = resolvedCharacterID;
            request.RelatedEntityID = $"unlock_char_{resolvedCharacterID}_{Guid.NewGuid():N}";

            var result = await CharacterAPI.UnlockCharacter(request);
            if (result.Success && result.Data != null)
            {
                var data = result.Data;

                IDosGamesData.User.PatchCharacter(data.CharacterID, ch =>
                {
                    ch.StatLevels ??= new Dictionary<string, int>();
                    ch.Equipment ??= new Dictionary<string, EquippedItem>();
                    ch.UpdatedAt = data.ServerTimeUtc;
                });

                if (data.Resources != null)
                    IDosGamesData.User.ApplyResourceOperation(data.Resources, IDosGamesData.Config?.TitlePublicConfiguration?.Item);

                OnCharacterUnlocked?.Invoke(data);
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

        private static void ApplyEquipChangesLocally(EquipItemsResponse response)
        {
            if (response == null || string.IsNullOrWhiteSpace(response.CharacterID)) return;

            var user = IDosGamesData.User;
            var state = user.State;
            if (state == null) return;

            string characterID = response.CharacterID;
            var replaced = response.ReplacedInstanceIDs;

            CharacterModel character = null;
            state.Character?.Characters?.TryGetValue(characterID, out character);

            user.MutateUnstackableItems(store =>
            {
                var touched = new HashSet<(string ItemID, string CatalogID)>();

                if (replaced != null)
                {
                    foreach (var instID in replaced)
                    {
                        if (string.IsNullOrWhiteSpace(instID)) continue;
                        if (store.TryGetValue(instID, out var inst) && inst != null)
                        {
                            inst.EquippedSlot = null;
                            if (!string.IsNullOrEmpty(inst.ItemID))
                                touched.Add((inst.ItemID, inst.CatalogID ?? string.Empty));
                        }
                    }
                }

                if (response.Equipment != null)
                {
                    foreach (var kv in response.Equipment)
                    {
                        string slotID = kv.Key;
                        var equipped = kv.Value;
                        if (string.IsNullOrWhiteSpace(slotID) || equipped == null) continue;

                        if (character?.Equipment != null
                            && character.Equipment.TryGetValue(slotID, out var oldEquipped)
                            && oldEquipped != null
                            && !string.IsNullOrWhiteSpace(oldEquipped.ItemInstanceID)
                            && !string.Equals(oldEquipped.ItemInstanceID, equipped.ItemInstanceID, StringComparison.Ordinal)
                            && (replaced == null || !replaced.Contains(oldEquipped.ItemInstanceID))
                            && store.TryGetValue(oldEquipped.ItemInstanceID, out var oldInst)
                            && oldInst != null)
                        {
                            oldInst.EquippedSlot = null;
                            if (!string.IsNullOrEmpty(oldInst.ItemID))
                                touched.Add((oldInst.ItemID, oldInst.CatalogID ?? string.Empty));
                        }

                        string equippedInstID = equipped.ItemInstanceID;
                        if (string.IsNullOrWhiteSpace(equippedInstID)) continue;

                        if (!string.IsNullOrEmpty(equipped.ItemID))
                            touched.Add((equipped.ItemID, equipped.CatalogID ?? string.Empty));

                        var newSlot = new EquipmentSlot
                        {
                            CharacterID = characterID,
                            SlotID = slotID,
                        };

                        if (store.TryGetValue(equippedInstID, out var existing) && existing != null)
                        {
                            existing.EquippedSlot = newSlot;
                        }
                        else
                        {
                            string bundleID = FindPristineBundleID(store, equipped.ItemID, equipped.CatalogID);
                            if (bundleID != null && store.TryGetValue(bundleID, out var bundle) && bundle != null)
                            {
                                int bundleQty = Mathf.Max(1, bundle.Quantity);
                                if (bundleQty <= 1) store.Remove(bundleID);
                                else bundle.Quantity = bundleQty - 1;
                            }

                            store[equippedInstID] = new UnstackableItemInstanceState
                            {
                                ItemInstanceID = equippedInstID,
                                ItemID = equipped.ItemID,
                                CatalogID = equipped.CatalogID,
                                Quantity = 1,
                                Level = 1,
                                RemainingUses = 1,
                                AcquiredAt = response.ServerTimeUtc,
                                EquippedSlot = newSlot,
                            };
                        }
                    }
                }

                DefragmentPristineDuplicates(store, touched);
            });

            if (response.Equipment != null)
            {
                foreach (var kv in response.Equipment)
                {
                    if (string.IsNullOrWhiteSpace(kv.Key) || kv.Value == null) continue;
                    user.PatchCharacterEquipment(characterID, kv.Key, kv.Value);
                }
            }

            user.PatchCharacter(characterID, ch =>
            {
                ch.Power = response.Power;
                ch.UpdatedAt = response.ServerTimeUtc;
            });
        }

        private static void ApplyUnequipChangesLocally(string characterID, List<string> slotIDs)
        {
            var user = IDosGamesData.User;
            var state = user.State;
            if (state?.Character?.Characters == null
                || !state.Character.Characters.TryGetValue(characterID, out var character)
                || character?.Equipment == null
                || character.Equipment.Count == 0)
            {
                return;
            }

            var pairs = new List<(string slotID, string instID)>();
            foreach (var slotID in slotIDs)
            {
                if (!character.Equipment.TryGetValue(slotID, out var equipped)
                    || equipped == null
                    || string.IsNullOrWhiteSpace(equipped.ItemInstanceID))
                {
                    continue;
                }
                pairs.Add((slotID, equipped.ItemInstanceID));
            }

            foreach (var (slotID, _) in pairs)
                user.PatchCharacterEquipment(characterID, slotID, null);

            user.MutateUnstackableItems(store =>
            {
                var touched = new HashSet<(string ItemID, string CatalogID)>();
                foreach (var (_, instID) in pairs)
                {
                    if (!store.TryGetValue(instID, out var inst) || inst == null) continue;
                    if (!string.IsNullOrEmpty(inst.ItemID))
                        touched.Add((inst.ItemID, inst.CatalogID ?? string.Empty));
                    inst.EquippedSlot = null;
                    TryMergeIntoPristineBundle(store, instID, inst);
                }
                DefragmentPristineDuplicates(store, touched);
            });
        }

        private static void ApplyUnequipAllLocally()
        {
            var user = IDosGamesData.User;
            var characters = user.State?.Character?.Characters;
            if (characters == null || characters.Count == 0) return;

            var allInstanceIDs = new List<string>();
            foreach (var charKv in characters)
            {
                var charID = charKv.Key;
                var charModel = charKv.Value;
                if (charModel?.Equipment == null || charModel.Equipment.Count == 0) continue;

                var instanceIDs = charModel.Equipment.Values
                    .Where(e => e != null && !string.IsNullOrWhiteSpace(e.ItemInstanceID))
                    .Select(e => e.ItemInstanceID)
                    .ToList();
                var slotIDs = charModel.Equipment.Keys.ToList();

                foreach (var slotID in slotIDs)
                    user.PatchCharacterEquipment(charID, slotID, null);

                allInstanceIDs.AddRange(instanceIDs);
            }

            if (allInstanceIDs.Count == 0) return;

            user.MutateUnstackableItems(store =>
            {
                var touched = new HashSet<(string ItemID, string CatalogID)>();
                foreach (var instID in allInstanceIDs)
                {
                    if (!store.TryGetValue(instID, out var inst) || inst == null) continue;
                    if (!string.IsNullOrEmpty(inst.ItemID))
                        touched.Add((inst.ItemID, inst.CatalogID ?? string.Empty));
                    inst.EquippedSlot = null;
                    TryMergeIntoPristineBundle(store, instID, inst);
                }
                // Mirror server post-defrag for the cross-character case.
                DefragmentPristineDuplicates(store, touched);
            });
        }

        private static bool IsPristineForMerge(UnstackableItemInstanceState inst)
        {
            if (inst == null) return false;
            return Math.Max(1, inst.Level) == 1
                && Math.Max(1L, inst.RemainingUses) == 1L
                && inst.ExpiresAt == null
                && inst.EquippedSlot == null
                && string.IsNullOrEmpty(inst.CustomData);
        }

        private static string FindPristineBundleID(
            IDictionary<string, UnstackableItemInstanceState> store,
            string itemID, string catalogID, string excludeID = null)
        {
            if (store == null) return null;

            UnstackableItemInstanceState best = null;
            foreach (var kv in store)
            {
                if (excludeID != null && string.Equals(kv.Key, excludeID, StringComparison.Ordinal)) continue;
                var inst = kv.Value;
                if (inst == null) continue;
                if (!string.Equals(inst.ItemID, itemID, StringComparison.Ordinal)) continue;
                bool catalogMatches = string.Equals(
                    inst.CatalogID ?? string.Empty,
                    catalogID ?? string.Empty,
                    StringComparison.Ordinal);
                if (!catalogMatches) continue;
                if (!IsPristineForMerge(inst)) continue;

                if (best == null
                    || inst.AcquiredAt < best.AcquiredAt
                    || (inst.AcquiredAt == best.AcquiredAt
                        && string.CompareOrdinal(inst.ItemInstanceID, best.ItemInstanceID) < 0))
                {
                    best = inst;
                }
            }
            return best?.ItemInstanceID;
        }

        private static void TryMergeIntoPristineBundle(
            IDictionary<string, UnstackableItemInstanceState> store,
            string instID, UnstackableItemInstanceState inst)
        {
            if (!IsPristineForMerge(inst)) return;

            string bundleID = FindPristineBundleID(store, inst.ItemID, inst.CatalogID, excludeID: instID);
            if (bundleID == null || !store.TryGetValue(bundleID, out var bundle) || bundle == null) return;

            int addQty = Math.Max(1, inst.Quantity);
            bundle.Quantity = Math.Max(1, bundle.Quantity) + addQty;
            store.Remove(instID);
        }

        private static void DefragmentPristineDuplicates(
            IDictionary<string, UnstackableItemInstanceState> store,
            HashSet<(string ItemID, string CatalogID)> touchedItems)
        {
            if (store == null || touchedItems == null || touchedItems.Count == 0) return;

            foreach (var key in touchedItems)
            {
                if (string.IsNullOrWhiteSpace(key.ItemID)) continue;

                List<UnstackableItemInstanceState> pristine = null;
                foreach (var inst in store.Values)
                {
                    if (inst == null) continue;
                    if (!string.Equals(inst.ItemID, key.ItemID, StringComparison.Ordinal)) continue;
                    if (!string.Equals(inst.CatalogID ?? string.Empty,
                                       key.CatalogID ?? string.Empty,
                                       StringComparison.Ordinal)) continue;
                    if (!IsPristineForMerge(inst)) continue;
                    pristine ??= new List<UnstackableItemInstanceState>();
                    pristine.Add(inst);
                }

                if (pristine == null || pristine.Count <= 1) continue;

                pristine.Sort((a, b) =>
                {
                    int cmp = a.AcquiredAt.CompareTo(b.AcquiredAt);
                    if (cmp != 0) return cmp;
                    return string.CompareOrdinal(a.ItemInstanceID, b.ItemInstanceID);
                });

                var canonical = pristine[0];
                int canonicalQty = Math.Max(1, canonical.Quantity);
                for (int i = 1; i < pristine.Count; i++)
                {
                    var frag = pristine[i];
                    canonicalQty += Math.Max(1, frag.Quantity);
                    store.Remove(frag.ItemInstanceID);
                }
                canonical.Quantity = canonicalQty;
            }
        }
    }
}
