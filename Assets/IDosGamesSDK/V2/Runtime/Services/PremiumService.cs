using IDosGames.ClientModels;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace IDosGames
{
    public static class PremiumService
    {
        private static IGSAuthenticationContext Ctx => AuthenticationService.GetAuthContext();

        // ─── Events ────────────────────────────────────────────────────────

        public static event Action<PremiumDefinitions> OnDefinitionsLoaded;
        public static event Action<UserPremiumState> OnUserStateLoaded;
        public static event Action<PremiumPurchaseResponse> OnTrialActivated;
        public static event Action<PremiumPurchaseResponse> OnPurchaseCompleted;

        // ─── Base request ──────────────────────────────────────────────────

        private static PremiumRequest CreateBaseRequest() => new()
        {
            UserID = Ctx.UserID,
            ClientSessionTicket = Ctx.ClientSessionTicket,
            BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
            WebAppLink = WebSDK.webAppLink,
            RelatedEntityID = Guid.NewGuid().ToString(),
        };

        // ─── GetDefinitions ────────────────────────────────────────────────

        /// <summary>
        /// Загружает PremiumDefinitions из конфига тайтла и сохраняет в IDosGamesData.Config.
        /// </summary>
        public static async Task<OperationResult<PremiumDefinitionsResponse>> GetDefinitions()
        {
            var request = CreateBaseRequest();
            var result = await PremiumAPI.GetDefinitions(request);

            if (result.Success && result.Data?.Premium != null)
            {
                // TODO: добавить internal void PatchPremium(PremiumDefinitions) в TitleConfig,
                //       если метод отсутствует. Реализация:
                //         TitlePublicConfiguration.Premium = data;
                //         OnTitlePublicConfigurationUpdated?.Invoke();
                //         OnAnyUpdated?.Invoke();
                IDosGamesData.Config.PatchPremium(result.Data.Premium);
                OnDefinitionsLoaded?.Invoke(result.Data.Premium);
            }

            return result;
        }

        // ─── GetUserState ──────────────────────────────────────────────────

        /// <summary>
        /// Загружает UserPremiumState с сервера (с нормализацией просроченных подписок)
        /// и сохраняет в IDosGamesData.User.
        /// </summary>
        public static async Task<OperationResult<PremiumStateResponse>> GetUserState()
        {
            var request = CreateBaseRequest();
            var result = await PremiumAPI.GetUserState(request);

            if (result.Success && result.Data?.Premium != null)
            {
                // TODO: добавить internal void ApplyPremium(UserPremiumState) в UserData,
                //       если метод отсутствует. Реализация:
                //         Premium = data;
                //         OnPremiumUpdated?.Invoke();
                //         OnAnyUpdated?.Invoke();
                IDosGamesData.User.ApplyPremium(result.Data.Premium);
                OnUserStateLoaded?.Invoke(result.Data.Premium);
            }

            return result;
        }

        // ─── ActivateTrial ─────────────────────────────────────────────────

        /// <summary>
        /// Активирует триал-подписку. Доступно один раз на PremiumID на аккаунт.
        /// </summary>
        /// <param name="premiumID">ID подписки из конфига (напр. "silver_vip").</param>
        /// <param name="transactionID">Стабильный ID операции (повторный вызов с тем же ID — идемпотентен).</param>
        public static async Task<OperationResult<PremiumPurchaseResponse>> ActivateTrial(
            string premiumID,
            string transactionID)
        {
            if (string.IsNullOrWhiteSpace(premiumID))
            {
                Debug.LogWarning("[PremiumService.ActivateTrial] PremiumID is required.");
                return OperationResult<PremiumPurchaseResponse>.Fail("PremiumID is required.");
            }
            if (string.IsNullOrWhiteSpace(transactionID))
            {
                Debug.LogWarning("[PremiumService.ActivateTrial] TransactionID is required.");
                return OperationResult<PremiumPurchaseResponse>.Fail("TransactionID is required.");
            }

            var request = CreateBaseRequest();
            request.PremiumID = premiumID;
            request.TransactionID = transactionID;
            // Стабильный ключ: тот же premiumID + transactionID дают тот же ключ при ретрае.
            request.RelatedEntityID = $"trial_{premiumID}_{transactionID}";

            var result = await PremiumAPI.ActivateTrial(request);

            if (result.Success)
            {
                if (result.Data?.Premium != null)
                    IDosGamesData.User.ApplyPremium(result.Data.Premium);

                // Триал не двигает ресурсы (сервер возвращает пустой ResourceOperation),
                // но вызываем на всякий случай — ApplyResourceOperationResult safe на пустом op.
                if (result.Data?.Resources != null)
                    IDosGamesData.User.ApplyResourceOperationResult(result.Data.Resources);

                OnTrialActivated?.Invoke(result.Data);
            }

            return result;
        }

        // ─── PurchaseItemOrCurrency ────────────────────────────────────────

        /// <summary>
        /// Покупает подписку за ресурсы (виртуальная валюта / предметы / ивент-токены).
        /// </summary>
        /// <param name="premiumID">ID подписки из конфига.</param>
        /// <param name="transactionID">Стабильный ID операции для идемпотентности.</param>
        /// <param name="selectedOptionID">Индекс PriceOption из конфига (default 0).</param>
        /// <param name="count">Количество периодов подписки (bulk-покупка, default 1).</param>
        public static async Task<OperationResult<PremiumPurchaseResponse>> PurchaseItemOrCurrency(
            string premiumID,
            string transactionID,
            int selectedOptionID = 0,
            int count = 1)
        {
            if (string.IsNullOrWhiteSpace(premiumID))
            {
                Debug.LogWarning("[PremiumService.PurchaseItemOrCurrency] PremiumID is required.");
                return OperationResult<PremiumPurchaseResponse>.Fail("PremiumID is required.");
            }
            if (string.IsNullOrWhiteSpace(transactionID))
            {
                Debug.LogWarning("[PremiumService.PurchaseItemOrCurrency] TransactionID is required.");
                return OperationResult<PremiumPurchaseResponse>.Fail("TransactionID is required.");
            }
            if (count <= 0)
            {
                Debug.LogWarning("[PremiumService.PurchaseItemOrCurrency] Count must be > 0.");
                return OperationResult<PremiumPurchaseResponse>.Fail("Count must be greater than 0.");
            }

            // Проверяем баланс локально перед отправкой (fast-fail в UI).
            var option = GetPriceOption(premiumID, selectedOptionID);
            if (option != null)
                ValidatePriceOptionLocally(option);

            var request = CreateBaseRequest();
            request.PremiumID = premiumID;
            request.TransactionID = transactionID;
            request.SelectedOptionID = selectedOptionID;
            request.Count = count;
            // Стабильный ключ по конкретной сделке — идемпотентен при ретраях.
            request.RelatedEntityID = $"premium_purchase_{premiumID}_{transactionID}";

            var result = await PremiumAPI.PurchaseItemOrCurrency(request);

            if (result.Success)
            {
                if (result.Data?.Premium != null)
                    IDosGamesData.User.ApplyPremium(result.Data.Premium);

                // Применяем списание стоимости к InventoryV2.
                if (result.Data?.Resources != null)
                    IDosGamesData.User.ApplyResourceOperationResult(result.Data.Resources);

                OnPurchaseCompleted?.Invoke(result.Data);
            }

            return result;
        }

        // ─── PurchaseRealMoney ─────────────────────────────────────────────

        /// <summary>
        /// Подтверждает IAP-покупку подписки (Apple / Google).
        /// </summary>
        /// <param name="premiumID">ID подписки из конфига.</param>
        /// <param name="store">Платформа магазина.</param>
        /// <param name="productID">ID продукта в магазине.</param>
        /// <param name="transactionID">Транзакционный ID платформы (для идемпотентности).</param>
        /// <param name="receiptData">Чек Apple (base64). Обязателен для StoreType.Apple.</param>
        /// <param name="purchaseToken">Токен Google. Обязателен для StoreType.Google.</param>
        /// <param name="packageName">Имя пакета приложения (Google).</param>
        /// <param name="appStoreEnvironment">Окружение Apple ("Sandbox" / "Production").</param>
        public static async Task<OperationResult<PremiumPurchaseResponse>> PurchaseRealMoney(
            string premiumID,
            StoreType store,
            string productID,
            string transactionID,
            string receiptData = null,
            string purchaseToken = null,
            string packageName = null,
            string appStoreEnvironment = null)
        {
            if (string.IsNullOrWhiteSpace(premiumID))
            {
                Debug.LogWarning("[PremiumService.PurchaseRealMoney] PremiumID is required.");
                return OperationResult<PremiumPurchaseResponse>.Fail("PremiumID is required.");
            }
            if (string.IsNullOrWhiteSpace(productID))
            {
                Debug.LogWarning("[PremiumService.PurchaseRealMoney] ProductID is required.");
                return OperationResult<PremiumPurchaseResponse>.Fail("ProductID is required.");
            }
            if (string.IsNullOrWhiteSpace(transactionID))
            {
                Debug.LogWarning("[PremiumService.PurchaseRealMoney] TransactionID is required.");
                return OperationResult<PremiumPurchaseResponse>.Fail("TransactionID is required.");
            }

            bool hasAppleReceipt = store == StoreType.Apple && !string.IsNullOrWhiteSpace(receiptData);
            bool hasGoogleToken = store == StoreType.Google && !string.IsNullOrWhiteSpace(purchaseToken);
            if (!hasAppleReceipt && !hasGoogleToken)
            {
                Debug.LogWarning("[PremiumService.PurchaseRealMoney] ReceiptData (Apple) or PurchaseToken (Google) is required.");
                return OperationResult<PremiumPurchaseResponse>.Fail("Platform receipt or purchase token is required.");
            }

            var request = CreateBaseRequest();
            request.PremiumID = premiumID;
            request.Store = store;
            request.ProductID = productID;
            request.TransactionID = transactionID;
            request.ReceiptData = receiptData;
            request.PurchaseToken = purchaseToken;
            request.PackageName = packageName;
            request.AppStoreEnvironment = appStoreEnvironment;
            // Стабильный ключ: IAP транзакция конкретного premiumID уникальна по transactionID платформы.
            request.RelatedEntityID = $"premium_iap_{premiumID}_{transactionID}";

            var result = await PremiumAPI.PurchaseRealMoney(request);

            if (result.Success)
            {
                if (result.Data?.Premium != null)
                    IDosGamesData.User.ApplyPremium(result.Data.Premium);

                if (result.Data?.Resources != null)
                    IDosGamesData.User.ApplyResourceOperationResult(result.Data.Resources);

                OnPurchaseCompleted?.Invoke(result.Data);
            }

            return result;
        }

        // ─── Local helpers ─────────────────────────────────────────────────

        /// <summary>
        /// Достаёт PriceOption из локального конфига для fast-fail валидации в UI.
        /// Возвращает null, если конфиг ещё не загружен.
        /// </summary>
        private static PremiumPriceOption GetPriceOption(string premiumID, int optionID)
        {
            var defs = IDosGamesData.Config.TitlePublicConfiguration?.Premium?.Definitions;
            if (defs == null) return null;

            foreach (var def in defs)
            {
                if (def.PremiumID != premiumID) continue;
                if (def.PriceOptions == null) return null;

                foreach (var opt in def.PriceOptions)
                    if (opt.OptionID == optionID) return opt;
            }
            return null;
        }

        /// <summary>
        /// Локальная проверка баланса перед отправкой запроса (soft-check для UI).
        /// Выводит предупреждение, но не блокирует запрос — сервер авторитетен.
        /// </summary>
        private static void ValidatePriceOptionLocally(PremiumPriceOption option)
        {
            var inv = IDosGamesData.User.InventoryV2;
            if (inv == null || option.RequiredResources?.Standard?.Items == null) return;

            foreach (var cost in option.RequiredResources.Standard.Items)
            {
                if (cost.Type == ItemType.VirtualCurrency && cost.CurrencyID != null)
                {
                    long balance = 0;
                    if (inv.VirtualCurrencies != null
                        && inv.VirtualCurrencies.TryGetValue(cost.CurrencyID, out var ccState)
                        && ccState != null)
                    {
                        balance = ccState.Amount;
                    }

                    if (balance < (cost.Amount ?? 0))
                    {
                        Debug.LogWarning(
                            $"[PremiumService] Insufficient {cost.CurrencyID}: " +
                            $"has {balance}, needs {cost.Amount}.");
                    }
                }
                else if (cost.Type == ItemType.Item && cost.ItemID != null)
                {
                    long total = 0;
                    if (inv.Items != null
                        && inv.Items.TryGetValue(cost.ItemID, out var totals)
                        && totals != null)
                    {
                        total = totals.TotalAmount;
                    }

                    if (total < (cost.Amount ?? 0))
                    {
                        Debug.LogWarning(
                            $"[PremiumService] Insufficient item {cost.ItemID}: " +
                            $"has {total}, needs {cost.Amount}.");
                    }
                }
            }
        }
    }
}