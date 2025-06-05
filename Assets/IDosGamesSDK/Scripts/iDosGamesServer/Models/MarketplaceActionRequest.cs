#if IDOSGAMES_MARKETPLACE
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace IDosGames
{
	public class MarketplaceActionRequest
	{
        public string TitleID { get; set; }
        public string WebAppLink { get; set; }
        public string UserID { get; set; }
        public string ClientSessionTicket { get; set; }
        public string EntityToken { get; set; }
        public string AuthContext { get; set; }
        public string BuildKey { get; set; }

        [JsonProperty("MarketplaceAction"), JsonConverter(typeof(StringEnumConverter))]
		public MarketplaceAction Action { get; set; }

		[JsonProperty("VirtualCurrencyID"), JsonConverter(typeof(StringEnumConverter))]
		public VirtualCurrencyID CurrencyID { get; set; }

		public string ItemID { get; set; }

		public int Price { get; set; }

		[JsonProperty("ID")]
		public string OfferID { get; set; }
	}
}
#endif