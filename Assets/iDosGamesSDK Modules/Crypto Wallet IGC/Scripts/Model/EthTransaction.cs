using Newtonsoft.Json;

namespace IDosGames
{
	public class EthTransaction
	{
		[JsonProperty("from")]
		public string From { get; set; }

		[JsonProperty("to")]
		public string To { get; set; }

		[JsonProperty("gas", NullValueHandling = NullValueHandling.Ignore)]
		public string Gas { get; set; }

		[JsonProperty("gasPrice", NullValueHandling = NullValueHandling.Ignore)]
		public string GasPrice { get; set; }

		[JsonProperty("value")]
		public string Value { get; set; }

		[JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
		public string Data { get; set; } = "0x";
	}
}
