using IDosGames.TitlePublicConfiguration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IDosGames.UserProfile
{
    public class UserAvatar : MonoBehaviour
    {
        [SerializeField] private Gender _gender;
        [SerializeField] private Quaternion defaultRotation;
        [SerializeField] private GameObject _male;
        [SerializeField] private GameObject _female;
        [SerializeField] private GameObject _default;
        public Gender Gender => _gender;

        private List<CustomizationElement> castamizationElements = new List<CustomizationElement>();
        public Action<string> OnEquippedAvatarSkin;
        public Action<string> OnUnequippedAvatarSkin;
        public Action<string> OnInspectAvatarSkin;
        private Dictionary<ClothingType, string> _equippedSkins = new Dictionary<ClothingType, string>();
        private Dictionary<ClothingType, string> _tempEquippedSkins = new Dictionary<ClothingType, string>();

        public Dictionary<ClothingType, string> TempEquippedSkins => _tempEquippedSkins;

        private string inspectedSkin = null;
        public string InspectedSkin => inspectedSkin;

        private string temporarilyRemovedSkin = null;
        private DefaultAvatarSkin _startedData = null;

        public void Init(DefaultAvatarSkin data)
        {
            inspectedSkin = null;

            castamizationElements.Clear();
            _equippedSkins.Clear();

            _startedData = data;
            _gender = data.Gender;

            if (_gender == Gender.Male)
            {
                castamizationElements = FindAllCustomizationElements(_male.transform);
                foreach (var element in castamizationElements)
                {
                    element.Deactivate();
                }
                _male.transform.rotation = defaultRotation;
                _male.SetActive(true);

                _female.SetActive(false);
            }
            else if (_gender == Gender.Female)
            {
                castamizationElements = FindAllCustomizationElements(_female.transform);
                foreach (var element in castamizationElements)
                {
                    element.Deactivate();
                }
                _female.transform.rotation = defaultRotation;
                _female.SetActive(true);
                _male.SetActive(false);
            }

            var items = new Dictionary<ClothingType, string>
            {
                { ClothingType.Body, _startedData.Data.Body },
                { ClothingType.Pants, _startedData.Data.Pants },
                { ClothingType.Shoes, _startedData.Data.Shoes },
                { ClothingType.Torso, _startedData.Data.Torso },
                { ClothingType.Glasses, _startedData.Data.Glasses },
                { ClothingType.Hands, _startedData.Data.Hands },
                { ClothingType.Hat, _startedData.Data.Hat },
                { ClothingType.Mask, _startedData.Data.Mask }
            };

            var resultDictionary = items.Where(item => !string.IsNullOrEmpty(item.Value)).ToDictionary(item => item.Key.ToString(), item => item.Value);

            foreach (var item in resultDictionary)
            {
                var type = ConvertToClothingType(item.Key);
                _equippedSkins.Add(type, item.Value);
            }

            foreach (var kvp in _equippedSkins)
            {
                _tempEquippedSkins[kvp.Key] = kvp.Value;
            }

            foreach (var item in _equippedSkins.Values)
            {
                EquipSkin(item);
            }
        }

        public void ChangeAvatarGender(Gender gender)
        {
            if (inspectedSkin != null)
            {
                UnequipInspectSkin();
            }

            if (temporarilyRemovedSkin != null)
            {
                EquipRemovedSkin();
            }

            castamizationElements.Clear();
            _gender = gender;

            if (_gender == Gender.Male)
            {
                castamizationElements = FindAllCustomizationElements(_male.transform);
                foreach (var element in castamizationElements)
                {
                    element.Deactivate();
                }
                _male.SetActive(true);
                _female.SetActive(false);
            }
            else if (_gender == Gender.Female)
            {
                castamizationElements = FindAllCustomizationElements(_female.transform);
                foreach (var element in castamizationElements)
                {
                    element.Deactivate();
                }
                _female.SetActive(true);
                _male.SetActive(false);
            }

            Dictionary<ClothingType, string> copiedSkins = new Dictionary<ClothingType, string>(_tempEquippedSkins);
            foreach (var item in copiedSkins)
            {
                if (item.Key == ClothingType.Body)
                {
                    // Используем данные из _startedData  
                    string bodyItemID = _startedData.Data.Body;
                    EquipSkin(bodyItemID);
                }
                EquipSkin(item.Value);
            }
        }

        public void EquipSkin(string itemID)
        {
            var skinItem = UserDataService.GetAvatarSkinItem(itemID);

            if (_tempEquippedSkins.ContainsKey(skinItem.ClothingType))
            {
                _tempEquippedSkins.TryGetValue(skinItem.ClothingType, out string unequipSkinID);
                if (unequipSkinID != null)
                {
                    UnequipSkin(unequipSkinID);
                }

            }

            if (inspectedSkin != null)
            {
                var inspectedSkinItem = UserDataService.GetAvatarSkinItem(inspectedSkin);
                castamizationElements.FirstOrDefault(x => x.Type == inspectedSkinItem.ClothingType && x.AvatarMeshVersion.ToLower() == inspectedSkinItem.AvatarMeshVersion).Deactivate();
                OnUnequippedAvatarSkin?.Invoke(inspectedSkin);
                inspectedSkin = null;

            }

            var castamozationElement = castamizationElements.FirstOrDefault(x => x.Type == skinItem.ClothingType && x.AvatarMeshVersion.ToLower() == skinItem.AvatarMeshVersion);
            castamozationElement.Activate();
            if (_gender == Gender.Male)
            {
                castamozationElement.SetTexture(skinItem.TexturePath);
            }
            else
            {
                castamozationElement.SetTexture(skinItem.FemaleTexturePath);
            }

            _tempEquippedSkins.Add(skinItem.ClothingType, itemID);
            OnEquippedAvatarSkin?.Invoke(itemID);
        }

        public void UnequipSkin(string itemID)
        {
            var skinItem = UserDataService.GetAvatarSkinItem(itemID);
            if (_tempEquippedSkins.ContainsKey(skinItem.ClothingType))
            {

                castamizationElements.FirstOrDefault(x => x.Type == skinItem.ClothingType && x.AvatarMeshVersion.ToLower() == skinItem.AvatarMeshVersion).Deactivate();

                _tempEquippedSkins.Remove(skinItem.ClothingType);
                OnUnequippedAvatarSkin?.Invoke(itemID);
            }
        }

        public void InspectSkin(string itemID)
        {
            UnequipInspectSkin();


            var skinItem = UserDataService.GetAvatarSkinItem(itemID);
            if (_tempEquippedSkins.ContainsKey(skinItem.ClothingType))
            {
                temporarilyRemovedSkin = _tempEquippedSkins[skinItem.ClothingType];
                UnequipSkin(_tempEquippedSkins[skinItem.ClothingType]);

            }
            var castamozationElement = castamizationElements.FirstOrDefault(x => x.Type == skinItem.ClothingType && x.AvatarMeshVersion.ToLower() == skinItem.AvatarMeshVersion);
            castamozationElement.Activate();
            if (_gender == Gender.Male)
            {
                castamozationElement.SetTexture(skinItem.TexturePath);
            }
            else
            {
                castamozationElement.SetTexture(skinItem.FemaleTexturePath);
            }
            inspectedSkin = itemID;
            OnInspectAvatarSkin?.Invoke(itemID);
        }

        public bool IsSkinEquippedInTemp(string itemID)
        {
            foreach (var item in _tempEquippedSkins.Values)
            {
                if (item.Equals(itemID))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsSkinEquipAsInspect(string itemID)
        {
            return itemID == inspectedSkin;
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

        public bool AreChanges()
        {
            UnequipInspectSkin();
            EquipRemovedSkin();

            var originalData = new JObject
            {
                { "Gender", _startedData.Gender.ToString() },
                { ClothingType.Body.ToString(), _startedData.Data.Body },
                { ClothingType.Glasses.ToString(), _startedData.Data.Glasses },
                { ClothingType.Hands.ToString(), _startedData.Data.Hands },
                { ClothingType.Hat.ToString(), _startedData.Data.Hat },
                { ClothingType.Mask.ToString(), _startedData.Data.Mask },
                { ClothingType.Pants.ToString(), _startedData.Data.Pants },
                { ClothingType.Shoes.ToString(), _startedData.Data.Shoes },
                { ClothingType.Torso.ToString(), _startedData.Data.Torso }
            };

            var currentData = new JObject
            {
                { "Gender", _gender.ToString() },
                { ClothingType.Body.ToString(), _tempEquippedSkins.GetValueOrDefault(ClothingType.Body) },
                { ClothingType.Glasses.ToString(), _tempEquippedSkins.GetValueOrDefault(ClothingType.Glasses) },
                { ClothingType.Hands.ToString(), _tempEquippedSkins.GetValueOrDefault(ClothingType.Hands) },
                { ClothingType.Hat.ToString(), _tempEquippedSkins.GetValueOrDefault(ClothingType.Hat) },
                { ClothingType.Mask.ToString(), _tempEquippedSkins.GetValueOrDefault(ClothingType.Mask) },
                { ClothingType.Pants.ToString(), _tempEquippedSkins.GetValueOrDefault(ClothingType.Pants) },
                { ClothingType.Shoes.ToString(), _tempEquippedSkins.GetValueOrDefault(ClothingType.Shoes) },
                { ClothingType.Torso.ToString(), _tempEquippedSkins.GetValueOrDefault(ClothingType.Torso) }
            };

            bool areEqual = JToken.DeepEquals(originalData, currentData);
            return !areEqual;
        }

        public DefaultAvatarSkin GetUpdateData()
        {
            var data = new DefaultAvatarSkinData
            {
                Body = _tempEquippedSkins.GetValueOrDefault(ClothingType.Body),
                Glasses = _tempEquippedSkins.GetValueOrDefault(ClothingType.Glasses),
                Hands = _tempEquippedSkins.GetValueOrDefault(ClothingType.Hands),
                Hat = _tempEquippedSkins.GetValueOrDefault(ClothingType.Hat),
                Mask = _tempEquippedSkins.GetValueOrDefault(ClothingType.Mask),
                Pants = _tempEquippedSkins.GetValueOrDefault(ClothingType.Pants),
                Shoes = _tempEquippedSkins.GetValueOrDefault(ClothingType.Shoes),
                Torso = _tempEquippedSkins.GetValueOrDefault(ClothingType.Torso)
            };

            return new DefaultAvatarSkin
            {
                Gender = _gender,
                Data = data
            };
        }

        public void UnequipInspectSkin()
        {
            if (inspectedSkin != null)
            {
                var inspectedSkinItem = UserDataService.GetAvatarSkinItem(inspectedSkin);

                castamizationElements.FirstOrDefault(x => x.Type == inspectedSkinItem.ClothingType && x.AvatarMeshVersion.ToLower() == inspectedSkinItem.AvatarMeshVersion).Deactivate();
                OnUnequippedAvatarSkin?.Invoke(inspectedSkin);

                inspectedSkin = null;
            }
        }

        public void EquipRemovedSkin()
        {

            if (temporarilyRemovedSkin != null)
            {
                EquipSkin(temporarilyRemovedSkin);

            }
            temporarilyRemovedSkin = null;
        }

        private ClothingType ConvertToClothingType(string value)
        {
            if (Enum.IsDefined(typeof(ClothingType), value))
            {
                return (ClothingType)Enum.Parse(typeof(ClothingType), value);
            }

            return ClothingType.None;

        }

        public void RefreshAvatar()
        {
            if (inspectedSkin != null)
            {
                var inspectedSkinItem = UserDataService.GetAvatarSkinItem(inspectedSkin);

                castamizationElements.FirstOrDefault(x => x.Type == inspectedSkinItem.ClothingType && x.AvatarMeshVersion.ToLower() == inspectedSkinItem.AvatarMeshVersion).Deactivate();
                OnUnequippedAvatarSkin?.Invoke(inspectedSkin);
            }
            inspectedSkin = null;
            _tempEquippedSkins.Clear();
            foreach (var kvp in _equippedSkins)
            {
                _tempEquippedSkins.Add(kvp.Key, kvp.Value);
            }
            foreach (var item in _equippedSkins.Values)
            {
                EquipSkin(item);
            }
            temporarilyRemovedSkin = null;
        }
    }
}