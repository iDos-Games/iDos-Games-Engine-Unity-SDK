using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace IDosGames
{
	public class WalletTransactionRequest
	{
		public string TitleID { get; set; }
        public string WebAppLink { get; set; }
        public string UserID { get; set; }
		public string ClientSessionTicket { get; set; }
		public string EntityToken { get; set; }
		public string AuthContext { get; set; }
        public string BuildKey { get; set; }

        public int ChainID { get; set; }

		[JsonProperty("WalletAddress")]
		public string ConnectedWalletAddress { get; set; }

		public string TransactionHash { get; set; }

		public int Amount { get; set; }

		public string SkinID { get; set; }

		[JsonProperty("VirtualCurrencyID"), JsonConverter(typeof(StringEnumConverter))]
		public VirtualCurrencyID CurrencyID { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public CryptoTransactionType TransactionType { get; set; }

		[JsonProperty("TransactionDirection"), JsonConverter(typeof(StringEnumConverter))]
		public TransactionDirection Direction { get; set; }
	}
}