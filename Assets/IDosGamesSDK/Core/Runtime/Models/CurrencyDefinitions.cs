using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace IDosGames
{
    [Serializable]
    public class CurrencyDefinitions
    {
        public Dictionary<string, VirtualCurrencyDefinition> VirtualCurrencies { get; set; }
        public Dictionary<string, CryptoCurrencyDefinition> CryptoCurrencies { get; set; }
    }

    // VirtualCurrencyDefinition
    [Serializable]
    public class VirtualCurrencyDefinition
    {
        public string CurrencyID { get; set; }
        public string DisplayName { get; set; }
        public Dictionary<string, string> AssetPaths { get; set; }
        public VirtualCurrencyEconomy Economy { get; set; }
        public RechargeConfig Recharge { get; set; }
        public CurrencyConversion Conversion { get; set; }
        public VirtualCurrencyPermissions Permissions { get; set; }
        public CurrencyAudit Audit { get; set; }
        public CurrencyStatus Status { get; set; }
    }

    [Serializable]
    public class VirtualCurrencyEconomy
    {
        public decimal ValueInUSD { get; set; }
        public DateTime? ValueInUSDUpdatedAt { get; set; }
        public long InitialDeposit { get; set; }
        public long MinBalance { get; set; }
        public long MaxBalance { get; set; }
        public long? DailyEarnLimit { get; set; }
        public long? DailySpendLimit { get; set; }
    }

    [Serializable]
    public class RechargeConfig
    {
        public long Rate { get; set; }
        public long Max { get; set; }
        public long Period { get; set; }
        public bool RechargeOffline { get; set; }
    }

    [Serializable]
    public class VirtualCurrencyPermissions
    {
        public bool IsTradable { get; set; }
        public bool IsPurchasable { get; set; }
        public bool IsRefundable { get; set; }
    }

    [Serializable]
    public class CurrencyConversion
    {
        public bool Enabled { get; set; }
        public ConversionRateMode RateMode { get; set; }
        public decimal FeePercent { get; set; }
        public List<ConversionTarget> Targets { get; set; }
    }

    [Serializable]
    public class ConversionTarget
    {
        public CurrencyType TargetCurrencyType { get; set; }
        public string TargetCurrencyID { get; set; }
        public decimal? Rate { get; set; }
        public long? MinAmount { get; set; }
        public long? MaxAmount { get; set; }
        public long? DailyLimit { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum CurrencyType
    {
        Virtual,
        Crypto,
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ConversionRateMode
    {
        Automatic,
        Manual,
    }

    [Serializable]
    public class CurrencyAudit
    {
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum CurrencyStatus
    {
        Active,
        Hidden,
        Deprecated,
        Maintenance
    }

    // CryptoCurrencyDefinition
    [Serializable]
    public class CryptoCurrencyDefinition
    {
        public string CurrencyID { get; set; }
        public string DisplayName { get; set; }
        public Dictionary<string, string> AssetPaths { get; set; }
        public int DisplayDecimals { get; set; }
        public decimal ValueInUSD { get; set; }
        public DateTime? ValueInUSDUpdatedAt { get; set; }
        public List<CryptoNetworkBinding> Networks { get; set; }
        public CryptoLimits Limits { get; set; }
        public CryptoCurrencyPermissions Permissions { get; set; }
        public CurrencyConversion Conversion { get; set; }
        public CurrencyAudit Audit { get; set; }
        public CurrencyStatus Status { get; set; }
    }

    [Serializable]
    public class CryptoNetworkBinding
    {
        public string NetworkID { get; set; }
        public string ContractAddress { get; set; }
        public int Decimals { get; set; }
        public decimal MinDeposit { get; set; }
        public decimal MinWithdraw { get; set; }
        public decimal WithdrawFee { get; set; }
        public bool DepositsEnabled { get; set; }
        public bool WithdrawalsEnabled { get; set; }
    }

    [Serializable]
    public class CryptoLimits
    {
        public decimal? DailyWithdrawUsd { get; set; }
        public decimal? MonthlyWithdrawUsd { get; set; }
        public decimal? KycRequiredAboveUsd { get; set; }
    }

    [Serializable]
    public class CryptoCurrencyPermissions
    {
        public bool DepositsEnabled { get; set; }
        public bool WithdrawalsEnabled { get; set; }
        public bool SpendableInGame { get; set; }
        public bool ConvertibleToVirtual { get; set; }
    }
}
