using IDosGames.ClientModels;
using Newtonsoft.Json.Linq;

namespace IDosGames
{
	public class SkinCatalogItem : ItemFromCatalog
	{
		public string Collection { get; protected set; }
		public string ImagePath { get; protected set; }
		public string TexturePath { get; protected set; }
		public float Profit { get; protected set; }
		public RarityType Rarity { get; protected set; }
		public int NFTID { get; protected set; }
		public string ObjectType { get; protected set; }
		public string AuthorID { get; protected set; }

		public SkinCatalogItem(CatalogItem item, JObject customData) : base(item)
		{
			customData.TryGetValue(JsonProperty.CATALOG_ITEM_OBJECT_TYPE, out JToken objectType).ToString();
			customData.TryGetValue(JsonProperty.CATALOG_ITEM_COLLECTION, out JToken collection).ToString();
			customData.TryGetValue(JsonProperty.CATALOG_ITEM_IMAGE_PATH, out JToken imagePath).ToString();
			customData.TryGetValue(JsonProperty.CATALOG_ITEM_TEXTURE_PATH, out JToken texturePath).ToString();
			customData.TryGetValue(JsonProperty.CATALOG_ITEM_NFT_ID, out JToken nftIDData).ToString();
			customData.TryGetValue(JsonProperty.CATALOG_ITEM_AUTHOR_ID, out JToken authorID).ToString();

			ObjectType = $"{objectType}";
			Collection = $"{collection}";
			ImagePath = $"{imagePath}";
			TexturePath = $"{texturePath}";
			AuthorID = $"{authorID}";

			int.TryParse($"{nftIDData}", out int nftID);
			NFTID = nftID;

			Profit = UserDataService.GetSkinProfitByCollection(Collection);
			Rarity = UserDataService.GetSkinRarityByCollection(Collection);
		}
	}
}