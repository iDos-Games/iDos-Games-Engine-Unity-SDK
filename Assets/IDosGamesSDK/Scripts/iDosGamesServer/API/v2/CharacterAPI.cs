using System.Threading.Tasks;
using IDosGames.ClientModels;
using IDosGames.TitlePublicConfiguration;

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

        public static async Task<OperationResult<CharacterDefinitions>> GetCharacterDefinitions(CharacterRequest request)
        {
            return await SendRequest<CharacterDefinitions>(CharacterAction.GetCharacterDefinitions, request);
        }

        public static async Task<OperationResult<GetCharactersResponse>> GetUserCharacters(CharacterRequest request)
        {
            return await SendRequest<GetCharactersResponse>(CharacterAction.GetUserCharacters, request);
        }

        public static async Task<OperationResult<UpgradeStatLevelResponse>> UpgradeStatLevel(CharacterRequest request)
        {
            return await SendRequest<UpgradeStatLevelResponse>(CharacterAction.UpgradeStatLevel, request);
        }

        public static async Task<OperationResult<UpgradeCharacterLevelResponse>> UpgradeCharacterLevel(CharacterRequest request)
        {
            return await SendRequest<UpgradeCharacterLevelResponse>(CharacterAction.UpgradeCharacterLevel, request);
        }

        public static async Task<OperationResult<SuccessResponse>> EquipItems(CharacterRequest request)
        {
            return await SendRequest<SuccessResponse>(CharacterAction.EquipItems, request);
        }

        public static async Task<OperationResult<SuccessResponse>> UnequipItems(CharacterRequest request)
        {
            return await SendRequest<SuccessResponse>(CharacterAction.UnequipItems, request);
        }
    }
}
