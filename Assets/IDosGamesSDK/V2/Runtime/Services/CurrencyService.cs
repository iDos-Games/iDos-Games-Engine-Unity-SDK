using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using UnityEngine;

namespace IDosGames
{
    public static class CurrencyService
    {
        // ---------- Typed events ----------

        /// <summary>Fires after a successful VC↔VC conversion with the server-confirmed result.</summary>
        public static event Action<ConvertResponse> OnConverted;

        /// <summary>Fires after a successful crypto-involved conversion with the server-confirmed result.</summary>
        public static event Action<CryptoConvertResponse> OnCryptoConverted;

        // ---------- Helpers ----------
        private static AuthContext Ctx => AuthenticationService.GetAuthContext();

        private static CurrencyRequest CreateBaseRequest() => new()
        {
            UserID = Ctx.UserID,
            ClientSessionTicket = Ctx.ClientSessionTicket,
            BuildKey = IDosGamesSDKSettings.Instance.BuildKey,
            WebAppLink = WebSDK.webAppLink,
            RelatedEntityID = Guid.NewGuid().ToString(),
        };

        // ===================== Actions =====================

        /// <summary>
        /// VC↔VC currency conversion. On success: applies consume/grant locally via
        /// <c>ApplyResourceOperation</c>, then fires <see cref="OnConverted"/>.
        /// Server-side rules (status, daily limit, rate, fee) are authoritative — only cheap
        /// preflight checks happen here.
        /// </summary>
        public static async Task<OperationResult<ConvertResponse>> Convert(
            CurrencyType sourceType, string sourceID,
            CurrencyType targetType, string targetID,
            long sourceAmount,
            string transactionID = null)
        {
            if (string.IsNullOrWhiteSpace(sourceID))
            {
                Debug.LogWarning("[CurrencyService] Convert: SourceID is required.");
                return OperationResult<ConvertResponse>.Fail("SourceID is required.");
            }
            if (string.IsNullOrWhiteSpace(targetID))
            {
                Debug.LogWarning("[CurrencyService] Convert: TargetID is required.");
                return OperationResult<ConvertResponse>.Fail("TargetID is required.");
            }
            if (sourceAmount <= 0)
            {
                Debug.LogWarning("[CurrencyService] Convert: Amount must be positive.");
                return OperationResult<ConvertResponse>.Fail("Amount must be positive.");
            }
            if (sourceType == targetType && string.Equals(sourceID, targetID, StringComparison.Ordinal))
            {
                Debug.LogWarning("[CurrencyService] Convert: Source and target must differ.");
                return OperationResult<ConvertResponse>.Fail("Source and target must differ.");
            }
            if (sourceType == CurrencyType.Crypto || targetType == CurrencyType.Crypto)
            {
                Debug.LogWarning("[CurrencyService] Convert: Crypto sides require CryptoConvert.");
                return OperationResult<ConvertResponse>.Fail("Convert supports VC↔VC only. Use CryptoConvert for any operation involving crypto currencies.");
            }

            var request = CreateBaseRequest();
            request.SourceType = sourceType;
            request.SourceID = sourceID;
            request.TargetType = targetType;
            request.TargetID = targetID;
            request.SourceAmount = sourceAmount.ToString(CultureInfo.InvariantCulture);
            request.TransactionID = string.IsNullOrEmpty(transactionID)
                ? $"convert_{sourceType}_{sourceID}_to_{targetType}_{targetID}_{Guid.NewGuid():N}"
                : transactionID;

            var result = await CurrencyAPI.Convert(request);
            if (result.Success && result.Data != null)
            {
                var data = result.Data;

                // Mirror server: consume source VC + grant target VC in one resource op.
                var op = new ResourceOperation
                {
                    Consume = new ResourceConsume
                    {
                        Standard = new ResourceBundle
                        {
                            Entries = new List<ResourceEntry>
                            {
                                new()
                                {
                                    Type = ResourceEntryType.VirtualCurrency,
                                    CurrencyID = data.SourceID,
                                    Amount = data.SourceSpent,
                                },
                            },
                        },
                    },
                    Grant = new ResourceGrant
                    {
                        Standard = new ResourceBundle
                        {
                            Entries = new List<ResourceEntry>
                            {
                                new()
                                {
                                    Type = ResourceEntryType.VirtualCurrency,
                                    CurrencyID = data.TargetID,
                                    Amount = data.TargetCredited,
                                },
                            },
                        },
                    },
                };
                IDosGamesData.User.ApplyResourceOperation(op, IDosGamesData.Config?.TitlePublicConfiguration?.Item);

                OnConverted?.Invoke(data);
            }
            return result;
        }

        /// <summary>
        /// Conversion involving at least one crypto currency (Crypto→VC, VC→Crypto, Crypto→Crypto).
        /// On success: applies VC sides via <c>ApplyResourceOperation</c> and crypto sides via
        /// <c>PatchCryptoCurrencyDelta</c> (decimal precision), then fires <see cref="OnCryptoConverted"/>.
        /// </summary>
        public static async Task<OperationResult<CryptoConvertResponse>> CryptoConvert(
            CurrencyType sourceType, string sourceID,
            CurrencyType targetType, string targetID,
            decimal sourceAmount,
            string transactionID = null)
        {
            if (string.IsNullOrWhiteSpace(sourceID))
            {
                Debug.LogWarning("[CurrencyService] CryptoConvert: SourceID is required.");
                return OperationResult<CryptoConvertResponse>.Fail("SourceID is required.");
            }
            if (string.IsNullOrWhiteSpace(targetID))
            {
                Debug.LogWarning("[CurrencyService] CryptoConvert: TargetID is required.");
                return OperationResult<CryptoConvertResponse>.Fail("TargetID is required.");
            }
            if (sourceAmount <= 0m)
            {
                Debug.LogWarning("[CurrencyService] CryptoConvert: SourceAmount must be a positive decimal.");
                return OperationResult<CryptoConvertResponse>.Fail("SourceAmount must be a positive decimal.");
            }
            if (sourceType == targetType && string.Equals(sourceID, targetID, StringComparison.Ordinal))
            {
                Debug.LogWarning("[CurrencyService] CryptoConvert: Source and target must differ.");
                return OperationResult<CryptoConvertResponse>.Fail("Source and target must differ.");
            }
            if (sourceType == CurrencyType.Virtual && targetType == CurrencyType.Virtual)
            {
                Debug.LogWarning("[CurrencyService] CryptoConvert: VC↔VC requires Convert action.");
                return OperationResult<CryptoConvertResponse>.Fail("CryptoConvert requires at least one side to be Crypto. Use Convert for VC↔VC.");
            }
            if (sourceType == CurrencyType.Virtual && sourceAmount != Math.Truncate(sourceAmount))
            {
                Debug.LogWarning("[CurrencyService] CryptoConvert: VC source amount must be integer.");
                return OperationResult<CryptoConvertResponse>.Fail("Virtual currency source amount must be integer (no fractional VC).");
            }

            var request = CreateBaseRequest();
            request.SourceType = sourceType;
            request.SourceID = sourceID;
            request.TargetType = targetType;
            request.TargetID = targetID;
            request.SourceAmount = sourceAmount.ToString(CultureInfo.InvariantCulture);
            request.TransactionID = string.IsNullOrEmpty(transactionID)
                ? $"crypto_convert_{sourceType}_{sourceID}_to_{targetType}_{targetID}_{Guid.NewGuid():N}"
                : transactionID;

            var result = await CurrencyAPI.CryptoConvert(request);
            if (result.Success && result.Data != null)
            {
                var data = result.Data;

                // VC sides flow through the standard ResourceOperation pipeline (long-typed).
                var consumeEntries = new List<ResourceEntry>();
                var grantEntries = new List<ResourceEntry>();
                if (data.SourceType == CurrencyType.Virtual)
                {
                    consumeEntries.Add(new ResourceEntry
                    {
                        Type = ResourceEntryType.VirtualCurrency,
                        CurrencyID = data.SourceID,
                        Amount = (long)data.SourceSpent,
                    });
                }
                if (data.TargetType == CurrencyType.Virtual)
                {
                    grantEntries.Add(new ResourceEntry
                    {
                        Type = ResourceEntryType.VirtualCurrency,
                        CurrencyID = data.TargetID,
                        Amount = (long)data.TargetCredited,
                    });
                }
                if (consumeEntries.Count > 0 || grantEntries.Count > 0)
                {
                    var op = new ResourceOperation
                    {
                        Consume = consumeEntries.Count > 0
                            ? new ResourceConsume { Standard = new ResourceBundle { Entries = consumeEntries } }
                            : null,
                        Grant = grantEntries.Count > 0
                            ? new ResourceGrant { Standard = new ResourceBundle { Entries = grantEntries } }
                            : null,
                    };
                    IDosGamesData.User.ApplyResourceOperation(op, IDosGamesData.Config?.TitlePublicConfiguration?.Item);
                }

                // Crypto sides need decimal precision — handled by a dedicated patch.
                if (data.SourceType == CurrencyType.Crypto)
                    IDosGamesData.User.PatchCryptoCurrencyDelta(data.SourceID, -data.SourceSpent, data.ServerTimeUtc);
                if (data.TargetType == CurrencyType.Crypto)
                    IDosGamesData.User.PatchCryptoCurrencyDelta(data.TargetID, data.TargetCredited, data.ServerTimeUtc);

                OnCryptoConverted?.Invoke(data);
            }
            return result;
        }
    }
}
