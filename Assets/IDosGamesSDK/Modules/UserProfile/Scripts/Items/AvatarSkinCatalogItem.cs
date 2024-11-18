using IDosGames.ClientModels;
using Newtonsoft.Json.Linq;
using System;

namespace IDosGames.UserProfile
{
    public class AvatarSkinCatalogItem : SkinCatalogItem
    {

        public string AvatarType { get; private set; }
        public string AvatarMeshVersion { get; private set; }
        public ClothingType ClothingType { get; private set; }
        public string FemaleTexturePath { get; private set; }
        public AvatarSkinCatalogItem(CatalogItem item, JObject customData) : base(item, customData)
        {
            customData.TryGetValue(JsonProperty.CATALOG_ITEM_OBJECT_TYPE, out JToken objectType).ToString();
            customData.TryGetValue(JsonProperty.CATALOG_ITEM_COLLECTION, out JToken collection).ToString();
            customData.TryGetValue(JsonProperty.CATALOG_ITEM_IMAGE_PATH, out JToken imagePath).ToString();
            customData.TryGetValue(JsonProperty.CATALOG_ITEM_TEXTURE_PATH, out JToken texturePath).ToString();
            customData.TryGetValue(JsonProperty.CATALOG_ITEM_NFT_ID, out JToken nftIDData).ToString();
            customData.TryGetValue(JsonProperty.CATALOG_ITEM_AUTHOR_ID, out JToken authorID).ToString();
            customData.TryGetValue(JsonProperty.CATALOG_ITEM_FEMALE_TEXTURE_PATH, out JToken femaleTexturePath).ToString();

            ObjectType = $"{objectType}";
            Collection = $"{collection}";
            ImagePath = $"{imagePath}";
            TexturePath = $"{texturePath}";
            AuthorID = $"{authorID}";
            FemaleTexturePath = $"{femaleTexturePath}";

            int.TryParse($"{nftIDData}", out int nftID);
            NFTID = nftID;

            Profit = UserDataService.GetSkinProfitByCollection(Collection);
            Rarity = UserDataService.GetSkinRarityByCollection(Collection);


            AvatarType = ObjectType.Split("_")[0].ToString();
            AvatarMeshVersion = ObjectType.Split("_")[1].ToString();
            var avatartype = UppercaseFirstLetter(AvatarType);
            ClothingType = ConvertToClothingType(avatartype);

        }


        private ClothingType ConvertToClothingType(string value)
        {
            if (Enum.IsDefined(typeof(ClothingType), value))
            {
                return (ClothingType)Enum.Parse(typeof(ClothingType), value);
            }

            return ClothingType.None;

        }

        private string UppercaseFirstLetter(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            string result = char.ToUpper(input[0]) + input.Substring(1).ToLower();

            return result;
        }

    }
}
