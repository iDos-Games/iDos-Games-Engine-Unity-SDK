using System.Threading.Tasks;

namespace IDosGames
{
    public static class CharacterAPI
    {
        private static string GetEndpoint(CharacterAction action, string userID)
        {
            string titleID = IDosGamesSDKSettings.Instance.TitleID;
            return $"api/v2/{titleID}/Client/Character/{action}/{userID}";
        }

        private static async Task<OperationResult<TResponse>> SendRequest<TResponse>(CharacterAction action, CharacterRequest request)
        {
            return await HttpService.Post<TResponse>(GetEndpoint(action, request.UserID), request, request.ClientSessionTicket);
        }

        public static Task<OperationResult<CharacterDefinitions>> GetCharacterDefinitions(CharacterRequest request)
            => SendRequest<CharacterDefinitions>(CharacterAction.GetCharacterDefinitions, request);

        public static Task<OperationResult<UserCharactersState>> GetUserCharacters(CharacterRequest request)
            => SendRequest<UserCharactersState>(CharacterAction.GetUserCharacters, request);

        public static Task<OperationResult<UpgradeStatLevelResponse>> UpgradeStatLevel(CharacterRequest request)
            => SendRequest<UpgradeStatLevelResponse>(CharacterAction.UpgradeStatLevel, request);

        public static Task<OperationResult<UpgradeCharacterLevelResponse>> UpgradeCharacterLevel(CharacterRequest request)
            => SendRequest<UpgradeCharacterLevelResponse>(CharacterAction.UpgradeCharacterLevel, request);

        public static Task<OperationResult<EquipItemsResponse>> EquipItems(CharacterRequest request)
            => SendRequest<EquipItemsResponse>(CharacterAction.EquipItems, request);

        public static Task<OperationResult<SuccessResponse>> UnequipItems(CharacterRequest request)
            => SendRequest<SuccessResponse>(CharacterAction.UnequipItems, request);

        public static Task<OperationResult<SuccessResponse>> UnequipAllCharacters(CharacterRequest request)
            => SendRequest<SuccessResponse>(CharacterAction.UnequipAllCharacters, request);

        public static Task<OperationResult<UnlockCharacterResponse>> UnlockCharacter(CharacterRequest request)
            => SendRequest<UnlockCharacterResponse>(CharacterAction.UnlockCharacter, request);
    }
}
