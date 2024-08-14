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
        private JObject _startedData = null;

        public void Init(JToken data)
        {
            inspectedSkin = null;

            castamizationElements.Clear();
            _equippedSkins.Clear();

            JObject json = (JObject)data;

            _startedData = json;
            var gender = json.GetValue("Gender").Value<string>();
            Enum.TryParse(gender, out Gender avatarGender);
            _gender = avatarGender;
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


            var jArray = json.GetValue("Data").Value<JArray>();


            Dictionary<string, string> resultDictionary = jArray
                .Select(item => (JProperty)item.First)
                .ToDictionary(property => property.Name, property => property.Value.ToString());

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
            Dictionary<ClothingType, string> coppiedSkins = new Dictionary<ClothingType, string>();
            foreach (var item in _tempEquippedSkins)
            {
                coppiedSkins.Add(item.Key, item.Value);
            }
            foreach (var item in coppiedSkins)
            {
                if (item.Key == ClothingType.Body)
                {


                    var titleData = UserDataService.GetTitleData(TitleDataKey.default_avatar_skin);
                    JObject data = JsonConvert.DeserializeObject<JObject>(titleData);
                    JArray jArray = data.GetValue("Data").Value<JArray>();
                    string bodyItemID = jArray
                .Select(item => (JProperty)item.First)
                .Where(property => property.Name == "Body")
                .Select(property => property.Value.ToString())
                .FirstOrDefault();

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
            var updatedData = GetUpdateData();

            bool areEqual = JToken.DeepEquals(_startedData, updatedData);
            return !areEqual;
        }

        public JObject GetUpdateData()
        {
            JArray jArray = new JArray();
            foreach (var item in _tempEquippedSkins)
            {
                JObject keyValuePairs = new JObject
            {
                { item.Key.ToString(), item.Value }
            };

                jArray.Add(keyValuePairs);
            }

            JObject updateData = new JObject()
        {
            { "Gender",_gender.ToString() },
            { "Data",jArray }
        };

            return updateData;
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