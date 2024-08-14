using System;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames.UserProfile
{
    public class AvatarItemMenuPopUp : MonoBehaviour
    {
        [SerializeField] private AvatarItemMenu _itemMenu;
        [SerializeField] private UserProfileWindow _avatarWindow;
        [SerializeField] private Button _inspectButton;
        [SerializeField] private Button _equipButton;
        [SerializeField] private Button _unequipButton;

        public void SetButtons(AvatarSkinCatalogItem item)
        {
            bool isEqipped = _avatarWindow.UserAvatar.IsSkinEquippedInTemp(item.ItemID);
            SetEquipButton(item.ItemID, isEqipped);
            SetUnequipButton(item.ItemID, isEqipped);
            SetInspectButton(item.ItemID);
        }

        public void SetPosition(Transform target)
        {
            transform.position = target.position;
        }

        private void SetInspectButton(string itemID)
        {
            _inspectButton.onClick.RemoveAllListeners();
            _inspectButton.onClick.AddListener(() => _avatarWindow.InspectSkin(itemID));
            _inspectButton.onClick.AddListener(() => _itemMenu.gameObject.SetActive(false));
        }

        private void SetEquipButton(string itemID, bool isEqipped)
        {
            int amountInInventory = UserInventory.GetItemAmount(itemID);
            if (!isEqipped && amountInInventory > 0)
            {
                _equipButton.onClick.RemoveAllListeners();
                _equipButton.onClick.AddListener(() => _avatarWindow.EquipSkin(itemID));
                _equipButton.onClick.AddListener(() => _itemMenu.gameObject.SetActive(false));

                _equipButton.gameObject.SetActive(true);
            }
            else
            {
                _equipButton.gameObject.SetActive(false);
            }
        }
        private void SetUnequipButton(string itemID, bool isEqipped)
        {

            var skinItem = UserDataService.GetAvatarSkinItem(itemID);
            var avatarType = UppercaseFirstLetter(skinItem.AvatarType);
            var type = ConvertToClothingType(avatarType);

            if (type == ClothingType.Body || type == ClothingType.Torso || type == ClothingType.Shoes || type == ClothingType.Pants)
            {

                _unequipButton.gameObject.SetActive(false);
                return;
            }
            _unequipButton.gameObject.SetActive(isEqipped);

            if (isEqipped)
            {
                _unequipButton.onClick.RemoveAllListeners();
                _unequipButton.onClick.AddListener(() => _avatarWindow.UnequipSkin(itemID));
                _unequipButton.onClick.AddListener(() => _itemMenu.gameObject.SetActive(false));
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