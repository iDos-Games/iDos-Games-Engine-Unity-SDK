using System;
using System.Threading.Tasks;
using IDosGames.ClientModels;

namespace IDosGames
{
    public static class CharacterService
    {
        // —обыти€ дл€ обновлени€ UI при успешных действи€х
        public static event Action<GetCharactersResponse> OnCharactersUpdated;
        public static event Action<UpgradeStatLevelResponse> OnStatUpgradeSuccess;

        private static IGSAuthenticationContext Ctx => AuthService.GetAuthContext();
        private static string UserID => Ctx.UserID;
        private static string ClientSessionTicket => Ctx.ClientSessionTicket;

        /// <summary>
        /// —оздает базовый запрос с об€зательными пол€ми авторизации
        /// </summary>
        private static CharacterRequest CreateBaseRequest()
        {
            return new CharacterRequest
            {
                UserID = UserID,
                ClientSessionTicket = ClientSessionTicket,
                // ƒополнительные метаданные, если нужны серверу
                UsageTime = IDosGamesSDKSettings.Instance.PlayTime,
                BuildKey = IDosGamesSDKSettings.Instance.BuildKey
            };
        }

        /// <summary>
        /// ѕолучает список определений статов (конфиг)
        /// </summary>
        public static async Task<OperationResult<GetStatDefinitionsResponse>> GetStatDefinitions()
        {
            var request = CreateBaseRequest();
            return await CharacterAPI.GetStatDefinitions(request);
        }

        /// <summary>
        /// ѕолучает текущих персонажей игрока
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
        /// ѕрокачивает уровень стата
        /// </summary>
        /// <param name="statId">ID стата (например, "Strength")</param>
        /// <param name="characterId">ID персонажа (по умолчанию null/Main)</param>
        public static async Task<OperationResult<UpgradeStatLevelResponse>> UpgradeStatLevel(string statId, string characterId = null)
        {
            var request = CreateBaseRequest();
            request.StatID = statId;
            request.CharacterID = characterId;

            var result = await CharacterAPI.UpgradeStatLevel(request);

            if (result.Success)
            {
                // ≈сли прокачка прошла успешно, вызываем событие (например, дл€ проигрывани€ звука или FX)
                OnStatUpgradeSuccess?.Invoke(result.Data);

                // ќпционально: можно автоматически обновить локальный инвентарь или список персонажей
                // UserDataService.RequestUserInventory(); 
            }

            return result;
        }
    }
}
