using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IDosGames.UserProfile
{
    public class InspectAvatar : MonoBehaviour
    {

        [SerializeField] private GameObject _male;
        [SerializeField] private GameObject _female;
        [SerializeField] private Transform _root;
        [SerializeField] private Vector3 _defaultPosition;

        public GameObject Male => _male;
        public GameObject Female => _female;

        private List<CustomizationElement> castamizationElements = new List<CustomizationElement>();

        public void InspectAvatarSkin(string itemID)
        {
            var skinItem = UserDataService.GetAvatarSkinItem(itemID);


            var model = Instantiate(_male, _root);
            castamizationElements = FindAllCustomizationElements(model.transform);
            foreach (var element in castamizationElements)
            {
                element.Deactivate();
            }
            SetModelTransform(model.transform);


            EquipSkin(skinItem);
            SetDefaultSkins(skinItem);

        }

        private void SetModelTransform(Transform transform)
        {
            transform.SetLocalPositionAndRotation(_defaultPosition, Quaternion.Euler(Vector3.zero));
        }
        private void EquipSkin(AvatarSkinCatalogItem skinItem)
        {

            var castamozationElement = castamizationElements.FirstOrDefault(x => x.Type == skinItem.ClothingType && x.AvatarMeshVersion.ToLower() == skinItem.AvatarMeshVersion);
            castamozationElement.Activate();
            castamozationElement.SetTexture(skinItem.TexturePath);
        }

        private void SetDefaultSkins(AvatarSkinCatalogItem skinItem)
        {

            var defaultSkin = UserDataService.GetCachedTitlePublicConfig(TitleDataKey.DefaultAvatarSkin);
            JObject json = JsonConvert.DeserializeObject<JObject>(defaultSkin);



            var jArray = json.GetValue("Data").Value<JArray>();


            Dictionary<string, string> resultDictionary = jArray
                .Select(item => (JProperty)item.First)
                .ToDictionary(property => property.Name, property => property.Value.ToString());

            foreach (var item in resultDictionary)
            {
                var defaultType = ConvertToClothingType(item.Key);
                if (defaultType == skinItem.ClothingType)
                {
                    continue;
                }
                var defaultItem = UserDataService.GetAvatarSkinItem(item.Value);
                EquipSkin(defaultItem);

            }




        }
        public List<CustomizationElement> FindAllCustomizationElements(Transform transform)
        {
            List<CustomizationElement> customizationElements = new List<CustomizationElement>();
            FindCustomizationElementsRecursive(customizationElements, transform);
            return customizationElements;
        }
        private static void FindCustomizationElementsRecursive(List<CustomizationElement> elements, Transform parent)
        {
            foreach (Transform child in parent)
            {
                CustomizationElement element = child.GetComponent<CustomizationElement>();
                if (element != null)
                {
                    elements.Add(element);
                }

                FindCustomizationElementsRecursive(elements, child);
            }
        }

        private ClothingType ConvertToClothingType(string value)
        {
            if (Enum.IsDefined(typeof(ClothingType), value))
            {
                return (ClothingType)Enum.Parse(typeof(ClothingType), value);
            }

            return ClothingType.None;

        }
    }
}