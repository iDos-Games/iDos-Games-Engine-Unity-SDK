using System.Threading.Tasks;
using IDosGames.ClientModels;

namespace IDosGames
{
    public static class CharacterAPI
    {
        /// <summary>
        /// Формирует URL для запроса.
        /// </summary>
        private static string GetEndpoint(CharacterAction action, string userID)
        {
            string templateID = IDosGamesSDKSettings.Instance.TitleTemplateID;
            string titleID = IDosGamesSDKSettings.Instance.TitleID;
            // Важно: убедитесь, что структура пути совпадает с серверным Route
            return $"api/v2/{templateID}/{titleID}/Client/Character/{action}/{userID}";
        }

        /// <summary>
        /// Универсальный метод отправки запроса, скрывающий детали HttpService.
        /// </summary>
        private static async Task<OperationResult<TResponse>> SendRequest<TResponse>(CharacterAction action, CharacterRequest request)
        {
            return await HttpService.Post<TResponse>(
                GetEndpoint(action, request.UserID),
                request,
                request.ClientSessionTicket
            );
        }

        // =================================================================================
        // PUBLIC METHODS
        // =================================================================================

        public static async Task<OperationResult<GetStatDefinitionsResponse>> GetStatDefinitions(CharacterRequest request)
        {
            return await SendRequest<GetStatDefinitionsResponse>(CharacterAction.GetStatDefinitions, request);
        }

        public static async Task<OperationResult<GetCharactersResponse>> GetCharacters(CharacterRequest request)
        {
            return await SendRequest<GetCharactersResponse>(CharacterAction.GetCharacters, request);
        }

        public static async Task<OperationResult<UpgradeStatLevelResponse>> UpgradeStatLevel(CharacterRequest request)
        {
            return await SendRequest<UpgradeStatLevelResponse>(CharacterAction.UpgradeStatLevel, request);
        }

        public static async Task<OperationResult<UpgradeCharacterLevelResponse>> UpgradeCharacterLevel(CharacterRequest request)
        {
            return await SendRequest<UpgradeCharacterLevelResponse>(CharacterAction.UpgradeCharacterLevel, request);
        }
    }
}
