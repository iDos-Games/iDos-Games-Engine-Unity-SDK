using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace IDosGames
{
    public class IGSRequest
    {
        public string TitleID { get; set; }
        public string UserID { get; set; }
        public string FunctionName { get; set; }
        public string OS { get; set; }
        public string Device { get; set; }
        public string DeviceID { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ClientSessionTicket { get; set; }
        public string EntityToken { get; set; }
        public string CatalogVersion { get; set; }
        public string FriendID { get; set; }
        public string LeaderboardID { get; set; }
        public string SecretKey { get; set; }
        public bool DevBuild { get; set; }

        // IAP Validations
        public string Receipt { get; set; }

        // Marketplace
        public string MarketplacePanel { get; set; }
        public string MaxItemCount { get; set; }
        public string MarketplaceAction { get; set; }
        public string ContinuationToken { get; set; }
        public string ItemID { get; set; }
        public string SortOrder { get; set; }
        public string OrderBy { get; set; }
        public string VirtualCurrencyID { get; set; }
        public string PriceFrom { get; set; }
        public string PriceTo { get; set; }
        public string ID { get; set; }
        public string Price { get; set; }

        // Crypto Wallet
        public string TransactionType { get; set; }
        public string TransactionDirection { get; set; }
        public string ChainID { get; set; }
        public string TransactionHash { get; set; }
        public string Amount { get; set; }
        public string WalletAddress { get; set; }
        public string SkinID { get; set; }

        public FunctionParameters FunctionParameter { get; set; }

#if UNITY_EDITOR
        // For Admin API
        public List<FileUpload> Files { get; set; }
#endif

    }

#if UNITY_EDITOR
    public class FileUpload
    {
        public string FilePath { get; set; }
        public byte[] FileContent { get; set; }

        public FileUpload(string filePath, byte[] fileContent)
        {
            FilePath = filePath;
            FileContent = fileContent;
        }
    }
#endif

    public class FunctionParameters
    {
        
        // UpdateCustomReadOnlyData  
        public string Key { get; set; }
        public object Value { get; set; }

        // SubtractVirtualCurrency  
        public string CurrencyID { get; set; }
        public int? Amount { get; set; }

        // GrantChainFreeOfferItems GrantChainItemsAfterIAPPurchase  
        public string OfferID { get; set; }
        public string Level { get; set; }
        public string ChainID { get; set; }

        // GrantSingleItemsAfterIAPPurchase  
        public string PriceType { get; set; }

        // StartWeekdaysEvent StartWeekendEvent  
        public string SecretKey { get; set; }

        // AddWeeklyEventPoints  
        public int? Points { get; set; }

        // ActivateReferralCode  
        public string ReferralCode { get; set; }

        // ClaimIGCReward
        public int? IntValue { get; set; }

        // UpdateEquippedSkins GrantSkinProfitFromEquippedSkins  
        public JArray ItemIDs { get; set; }
        public int? Multiplier { get; set; }

        // GetFreeDailyReward, BuyItemSpecialOffer, BuyItemForVirtualCurrency, BuyItemDailyOffer, GrantItemsAfterIAPPurchase  
        public string ItemID { get; set; }
    }
}
