#if IDOSGAMES_MARKETPLACE
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace IDosGames
{
	public class MarketplaceGetDataRequest
	{
		[JsonProperty("MarketplacePanel"), JsonConverter(typeof(StringEnumConverter))]
		public MarketplacePanel Panel { get; set; }

		public string TitleID { get; set; }
        public string WebAppLink { get; set; }
        public string UserID { get; set; }
        public string ClientSessionTicket { get; set; }
        public string EntityToken { get; set; }
		public string BuildKey { get; set; }

        [JsonProperty("MaxItemCount")]
		public int ItemsInOnePage { get; set; }

		public string ContinuationToken { get; set; }

		public string ItemID { get; set; }

		[JsonProperty("VirtualCurrencyID")]
		public string CurrencyID { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public MarketplaceSortOrder SortOrder { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public MarketplaceOrderBy OrderBy { get; set; }
	}
}
#endif