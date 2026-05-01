using IDosGames.TitlePublicConfiguration;
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
            var skinItem = DataService.GetAvatarSkinItem(itemID);
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
            var defaultSkin = IDosGamesData.Config.TitlePublicConfiguration?.DefaultAvatarSkin?.Data;

            if (defaultSkin == null)
            {
                return;
            }

            var defaultSkinMap = new Dictionary<string, string>
            {
                { nameof(DefaultAvatarSkinData.Body),    defaultSkin.Body },
                { nameof(DefaultAvatarSkinData.Glasses), defaultSkin.Glasses },
                { nameof(DefaultAvatarSkinData.Hands),   defaultSkin.Hands },
                { nameof(DefaultAvatarSkinData.Hat),     defaultSkin.Hat },
                { nameof(DefaultAvatarSkinData.Mask),    defaultSkin.Mask },
                { nameof(DefaultAvatarSkinData.Pants),   defaultSkin.Pants },
                { nameof(DefaultAvatarSkinData.Shoes),   defaultSkin.Shoes },
                { nameof(DefaultAvatarSkinData.Torso),   defaultSkin.Torso },
            };

            foreach (var item in defaultSkinMap)
            {
                if (string.IsNullOrEmpty(item.Value)) continue;

                var defaultType = ConvertToClothingType(item.Key);
                if (defaultType == skinItem.ClothingType) continue;

                var defaultItem = DataService.GetAvatarSkinItem(item.Value);
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
