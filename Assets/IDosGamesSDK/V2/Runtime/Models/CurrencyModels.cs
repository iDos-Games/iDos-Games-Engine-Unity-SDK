using System;

namespace IDosGames
{
    /// <summary>
    /// Common request for all Currency module actions. SourceAmount is wire-encoded as
    /// string to preserve precision through JSON — the server parses it as long for
    /// <see cref="CurrencyAction.Convert"/> and as decimal for <see cref="CurrencyAction.CryptoConvert"/>.
    ///
    /// Fields by action:
    /// - Convert: SourceType, SourceID, TargetType, TargetID, SourceAmount (integer-as-string), TransactionID.
    /// - CryptoConvert: SourceType, SourceID, TargetType, TargetID, SourceAmount (decimal-as-string, e.g. "0.5"), TransactionID.
    /// </summary>
    [Serializable]
    public class CurrencyRequest : BaseRequest
    {
        /// <summary>Source currency type: Virtual (off-chain) or Crypto (on-chain).</summary>
        public CurrencyType SourceType { get; set; }

        /// <summary>Source currency ID in TitlePublicConfiguration.Currency.VirtualCurrencies or CryptoCurrencies.</summary>
        public string SourceID { get; set; }

        /// <summary>Target currency type.</summary>
        public CurrencyType TargetType { get; set; }

        /// <summary>Target currency ID.</summary>
        public string TargetID { get; set; }

        /// <summary>
        /// Source amount to convert (fee included), encoded as decimal-string for wire-level
        /// precision (e.g. "100" for VC, "0.5" for half ETH). The server-side parser picks the
        /// numeric type per action.
        /// </summary>
        public string SourceAmount { get; set; }

        /// <summary>
        /// Idempotency key (typically client UUID). Used by the server's reason field to guard
        /// against repeated debits on network retries.
        /// </summary>
        public string TransactionID { get; set; }
    }

    /// <summary>
    /// Response for <see cref="CurrencyAction.Convert"/> (VC↔VC). All amount fields are integer
    /// (long) — fractional VC is not representable.
    /// </summary>
    [Serializable]
    public class ConvertResponse
    {
        /// <summary>Server time at which the conversion was applied (UTC).</summary>
        public DateTime ServerTimeUtc { get; set; }

        public CurrencyType SourceType { get; set; }
        public string SourceID { get; set; }
        public CurrencyType TargetType { get; set; }
        public string TargetID { get; set; }

        /// <summary>Total source units debited from the player (fee included).</summary>
        public long SourceSpent { get; set; }

        /// <summary>Conversion fee charged, in source currency units.</summary>
        public long FeeAmount { get; set; }

        /// <summary>Applied rate: target units per 1 source unit after fee deduction.</summary>
        public decimal RateApplied { get; set; }

        /// <summary>Target units credited to the player.</summary>
        public long TargetCredited { get; set; }
    }

    /// <summary>
    /// Response for <see cref="CurrencyAction.CryptoConvert"/>. All amount fields are decimal
    /// to support fractional crypto values. When TargetType is Virtual, TargetCredited still
    /// stores an integer value (the server truncates) but in a decimal slot.
    /// </summary>
    [Serializable]
    public class CryptoConvertResponse
    {
        public DateTime ServerTimeUtc { get; set; }

        public CurrencyType SourceType { get; set; }
        public string SourceID { get; set; }
        public CurrencyType TargetType { get; set; }
        public string TargetID { get; set; }

        /// <summary>Total source units debited (fee included).</summary>
        public decimal SourceSpent { get; set; }

        /// <summary>Conversion fee charged, in source currency units.</summary>
        public decimal FeeAmount { get; set; }

        /// <summary>Applied rate: target units per 1 source unit after fee deduction.</summary>
        public decimal RateApplied { get; set; }

        /// <summary>Target units credited. Stores integer value when TargetType is Virtual.</summary>
        public decimal TargetCredited { get; set; }
    }

    /// <summary>Actions supported by Currency module.</summary>
    public enum CurrencyAction
    {
        /// <summary>VC↔VC conversion with integer amounts (long).</summary>
        Convert,

        /// <summary>
        /// Conversion involving at least one crypto currency (Crypto→VC, VC→Crypto, Crypto→Crypto).
        /// Accepts decimal source amounts — supports fractional crypto (e.g. 0.5 ETH).
        /// </summary>
        CryptoConvert,
    }
}
