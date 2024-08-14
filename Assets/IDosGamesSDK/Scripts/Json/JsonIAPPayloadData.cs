using Newtonsoft.Json;

namespace IDosGames
{
	public class JsonIAPPayloadData
	{
		public JsonIAPPurchaseData JsonPurchaseData;
		public string signature;
		public string json;

		public static JsonIAPPayloadData ConvertFromJson(string json)
		{
			var payload = JsonConvert.DeserializeObject<JsonIAPPayloadData>(json);
			payload.JsonPurchaseData = JsonConvert.DeserializeObject<JsonIAPPurchaseData>(payload.json);

			return payload;
		}
	}
}