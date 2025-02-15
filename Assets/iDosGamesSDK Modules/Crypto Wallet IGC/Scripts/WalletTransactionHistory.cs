using Newtonsoft.Json.Linq;
using System.Linq;
using UnityEngine;

namespace IDosGames
{
	public class WalletTransactionHistory
	{
		public const int MAX_AMOUNT_OF_HISTORY_ITEMS = 500;
		public static string PLAYER_PREFS_WALLET_TRANSACTION_HISTORY = "WALLET_TRANSACTION_HISTORY" + AuthService.UserID;

		public static void SaveNewItem(int chainID, string hash, TransactionDirection direction, string itemName, int amount, string imagePath)
		{
			JArray historyArray = GetHistoryArray();

			var item = new JObject
			{
				[JsonProperty.CHAIN_ID] = chainID,
				[JsonProperty.HASH] = hash,
				[JsonProperty.DIRECTION] = direction.ToString(),
				[JsonProperty.NAME] = itemName,
				[JsonProperty.AMOUNT] = amount,
				[JsonProperty.IMAGE_PATH] = imagePath
			};

			historyArray.AddFirst(item);
			historyArray = JArray.FromObject(historyArray.Take(MAX_AMOUNT_OF_HISTORY_ITEMS).ToArray());

			PlayerPrefs.SetString(PLAYER_PREFS_WALLET_TRANSACTION_HISTORY, historyArray.ToString());
			PlayerPrefs.Save();
		}

		public static JArray GetHistoryArray()
		{
			return JArray.Parse(PlayerPrefs.GetString(PLAYER_PREFS_WALLET_TRANSACTION_HISTORY, "[]"));
		}
	}
}