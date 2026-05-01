using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IDosGames.ClientModels;
using IDosGames.TitlePublicConfiguration;

namespace IDosGames
{
    public static class CharacterService
    {
        public static event Action<GetCharactersResponse> OnCharactersUpdated;
        public static event Action<UpgradeStatLevelResponse> OnStatUpgradeSuccess;
        public static event Action<UpgradeCharacterLevelResponse> OnCharacterLevelUpgradeSuccess;
        public static event Action<SuccessResponse> OnEquipItems;
        public static event Action<SuccessResponse> OnUnequipItems;
        public static event Action<SuccessResponse> OnUnequipAllCharacters;

        private static IGSAuthenticationContext Ctx => AuthenticationService.GetAuthContext();
        private static string UserID => Ctx.UserID;
        private static string ClientSessionTicket => Ctx.ClientSessionTicket;

        /// <summary>
        /// Creates a basic request with required authorization fields
        /// </summary>
        private static CharacterRequest CreateBaseRequest()
        {
            return new CharacterRequest
            {
                UserID = UserID,
                ClientSessionTicket = ClientSessionTicket,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
                WebAppLink = WebSDK.webAppLink,
            };
        }

        /// <summary>
        /// Gets a list of Character Definitions (config)
        /// </summary>
        public static async Task<OperationResult<CharacterDefinitions>> GetCharacterDefinitions()
        {
            var request = CreateBaseRequest();
            var result = await CharacterAPI.GetCharacterDefinitions(request);

            if (result.Success)
            {
                IDosGamesData.Config.PatchCharacterDefinitions(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Gets the player's current characters
        /// </summary>
        public static async Task<OperationResult<GetCharactersResponse>> GetUserCharacters()
        {
            var request = CreateBaseRequest();
            var result = await CharacterAPI.GetUserCharacters(request);

            if (result.Success)
            {
                IDosGamesData.User.ApplyCharacters(result.Data.Characters);
                OnCharactersUpdated?.Invoke(result.Data);
            }

            return result;
        }

        /// <summary>
        /// Increases stat level
        /// </summary>
        /// <param name="statId">Stat ID (e.g. "Health")</param>
        /// <param name="characterId">Character ID (default null/Main)</param>
        public static async Task<OperationResult<UpgradeStatLevelResponse>> UpgradeStatLevel(string statId, string characterId = null)
        {
            var request = CreateBaseRequest();
            request.StatID = statId;
            request.CharacterID = characterId;

            var result = await CharacterAPI.UpgradeStatLevel(request);

            if (result.Success)
            {
                var targetId = characterId ?? DefaultData.Main;
                IDosGamesData.User.PatchCharacter(targetId, c =>
                {
                    c.StatLevels ??= new();
                    c.StatLevels[result.Data.StatID] = result.Data.StatLevel;
                });
                IDosGamesData.User.PatchConsumedResource(result.Data.ConsumedResource, result.Data.ConsumedResourceBalance);
                OnStatUpgradeSuccess?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<UpgradeCharacterLevelResponse>> UpgradeCharacterLevel(string characterId = null)
        {
            var request = CreateBaseRequest();
            request.CharacterID = characterId;

            var result = await CharacterAPI.UpgradeCharacterLevel(request);

            if (result.Success)
            {
                var targetId = result.Data.CharacterID ?? characterId ?? DefaultData.Main;
                IDosGamesData.User.PatchCharacter(targetId, c =>
                {
                    c.Level = result.Data.NewLevel;
                });
                IDosGamesData.User.ConsumeResources(result.Data.ConsumedResources);
                OnCharacterLevelUpgradeSuccess?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<SuccessResponse>> EquipItems(List<EquipSlotPair> itemsToEquip, string characterId = null)
        {
            var request = CreateBaseRequest();
            request.CharacterID = characterId;
            request.ItemsToEquip = itemsToEquip;

            var result = await CharacterAPI.EquipItems(request);

            if (result.Success)
            {
                var targetId = characterId ?? DefaultData.Main;
                foreach (var pair in itemsToEquip)
                {
                    IDosGamesData.User.PatchCharacterEquipment(targetId, pair.SlotID, new EquippedItem
                    {
                        ItemID = pair.ItemID,
                        ItemInstanceID = pair.ItemInstanceID
                    });
                }
                OnEquipItems?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<SuccessResponse>> UnequipItems(List<string> unequipSlotIDs, string characterId = null)
        {
            var request = CreateBaseRequest();
            request.CharacterID = characterId;
            request.UnequipSlotIDs = unequipSlotIDs;

            var result = await CharacterAPI.UnequipItems(request);

            if (result.Success)
            {
                var targetId = characterId ?? DefaultData.Main;
                foreach (var slotId in unequipSlotIDs)
                {
                    IDosGamesData.User.PatchCharacterEquipment(targetId, slotId, null);
                }
                OnUnequipItems?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<SuccessResponse>> UnequipAllCharacters()
        {
            var request = CreateBaseRequest();

            var result = await CharacterAPI.UnequipAllCharacters(request);

            if (result.Success)
            {
                IDosGamesData.User.ClearAllCharactersEquipment();
                OnUnequipAllCharacters?.Invoke(result.Data);
            }

            return result;
        }
    }
}
