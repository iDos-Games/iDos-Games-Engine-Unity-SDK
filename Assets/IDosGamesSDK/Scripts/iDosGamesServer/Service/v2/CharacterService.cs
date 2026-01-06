using System;
using System.Threading.Tasks;
using IDosGames.ClientModels;

namespace IDosGames
{
    public static class CharacterService
    {
        public static event Action<GetCharactersResponse> OnCharactersUpdated;
        public static event Action<UpgradeStatLevelResponse> OnStatUpgradeSuccess;
        public static event Action<UpgradeCharacterLevelResponse> OnCharacterLevelUpgradeSuccess;

        private static IGSAuthenticationContext Ctx => AuthService.GetAuthContext();
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
        /// Gets a list of stat definitions (config)
        /// </summary>
        public static async Task<OperationResult<GetStatDefinitionsResponse>> GetStatDefinitions()
        {
            var request = CreateBaseRequest();
            return await CharacterAPI.GetStatDefinitions(request);
        }

        /// <summary>
        /// Gets the player's current characters
        /// </summary>
        public static async Task<OperationResult<GetCharactersResponse>> GetCharacters()
        {
            var request = CreateBaseRequest();
            var result = await CharacterAPI.GetCharacters(request);

            if (result.Success)
            {
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
                OnStatUpgradeSuccess?.Invoke(result.Data);
            }

            return result;
        }

        public static async Task<OperationResult<UpgradeCharacterLevelResponse>> UpgradeCharacterLevel()
        {
            var request = CreateBaseRequest();
            var result = await CharacterAPI.UpgradeCharacterLevel(request);

            if (result.Success)
            {
                OnCharacterLevelUpgradeSuccess?.Invoke(result.Data);
            }

            return result;
        }
    }
}
