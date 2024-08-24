using IDosGames.UserProfile;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IDosGames
{
    public class InventoryWindow : MonoBehaviour
    {
        public static Action EquippedSkinsChanged;

        [SerializeField] private InventoryWindowView _view;
        [SerializeField] private SkinInventoryItemFull _skinItemPrefab;
        [SerializeField] private Transform _parent;
        [SerializeField] private PopUpSaveChanges _popUpSaveChanges;
        [SerializeField] private SkinInspectionRoom _skinInspectionRoom;

        private List<string> _temporaryEquippedSkins = new();
        private List<SkinInventoryItemFull> _activeSkinItems = new();

        private void OnEnable()
        {
            InititalizeTemporaryEquippedSkins();

            _view.SelectMyItemsTab();
            _view.HideAllVoidTexts();
            UpdateUserInventoryItemsItems();

            UserDataService.EquippedSkinsUpdated += OnSuccessUpdateEquippedSkins;
        }

        private void OnDisable()
        {
            UserDataService.EquippedSkinsUpdated -= OnSuccessUpdateEquippedSkins;
        }

        public int GetCurrentProfitAmount()
        {
            var amount = 0;

            foreach (var itemID in _temporaryEquippedSkins)
            {
                var skinItem = UserDataService.GetCachedSkinItem(itemID);

                if (skinItem != null)
                {
                    amount += (int)skinItem.Profit;
                }
            }

            return amount;
        }

        public void TryCloseWindow()
        {
            if (AreEquippedSkinsChanged())
            {
                _popUpSaveChanges.gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        public void SaveChanges()
        {
            UpdateEquippedSkins();
        }

        public void EquipSkin(string itemID)
        {
            //unequip item with same object type if exists
            var itemObjectType = UserDataService.GetCachedSkinItem(itemID).ObjectType;

            string itemIDWithSameObjectType = string.Empty;

            foreach (var equippedSkinID in _temporaryEquippedSkins)
            {
                if (UserDataService.GetCachedSkinItem(equippedSkinID).ObjectType == itemObjectType)
                {
                    itemIDWithSameObjectType = equippedSkinID;
                    break;
                }
            }

            if (itemIDWithSameObjectType != string.Empty)
            {
                UnequipSkin(itemIDWithSameObjectType);
            }
            //end of unequip item with same object type

            _temporaryEquippedSkins.Add(itemID);

            var item = _activeSkinItems.FirstOrDefault(x => x.SkinData.ItemID == itemID);

            if (item != null)
            {
                item.SetActivateCheckMark(true);
            }

            _view.UpdateCurrentProfitAmountText(GetCurrentProfitAmount());
        }

        public void UnequipSkin(string itemID)
        {
            _temporaryEquippedSkins.Remove(itemID);

            var item = _activeSkinItems.FirstOrDefault(x => x.SkinData.ItemID == itemID);

            if (item != null)
            {
                item.SetActivateCheckMark(false);
            }

            _view.UpdateCurrentProfitAmountText(GetCurrentProfitAmount());
        }

        public void InspectSkin(string itemID)
        {
            _skinInspectionRoom.OpenRoom(itemID);
        }

        public void OpenUserInventoryItems()
        {
            _view.SelectMyItemsTab();
            UpdateUserInventoryItemsItems();
        }

        public void OpenCatalogItems()
        {
            _view.SelectCatalogTab();
            UpdateCatalogItems();
        }

        public void UpdateEquippedSkins()
        {
            UserDataService.UpdateEquippedSkins(_temporaryEquippedSkins);
        }

        public void OnSuccessUpdateEquippedSkins()
        {
            gameObject.SetActive(false);
        }

        private void InititalizeTemporaryEquippedSkins()
        {
            _temporaryEquippedSkins = new(UserDataService.EquippedSkins);
            _view.UpdateCurrentProfitAmountText(GetCurrentProfitAmount());
        }

        private void UpdateUserInventoryItemsItems()
        {
            _view.SetActivateCatalogVoidText(false);

            List<SkinCatalogItem> skins = new();

            foreach (var skin in UserDataService.AllSkinsInCatalog)
            {
                if (skin is AvatarSkinCatalogItem)
                {
                    continue;
                }
                var amount = UserInventory.GetItemAmount(skin.ItemID);

                if (amount > 0)
                {
                    skins.Add(skin);
                }
            }

            _view.SetActivateInventoryVoidText(skins.Count == 0);

            skins = skins.OrderBy(o => o.Rarity).Reverse().ToList(); //Sorted by rarity

            InstatiateNewItems(skins);
        }

        private void UpdateCatalogItems()
        {
            _view.SetActivateInventoryVoidText(false);

            List<SkinCatalogItem> skins = new List<SkinCatalogItem>();

            foreach (var skin in UserDataService.AllSkinsInCatalog)
            {
                if (skin is AvatarSkinCatalogItem)
                {
                    continue;
                }
                skins.Add(skin);
            }

            _view.SetActivateCatalogVoidText(skins.Count == 0);

            skins = skins.OrderBy(o => o.Rarity).Reverse().ToList(); //Sorted by rarity

            InstatiateNewItems(skins);
        }

        private void InstatiateNewItems(List<SkinCatalogItem> skins)
        {
            _activeSkinItems.Clear();

            foreach (Transform child in _parent)
            {
                Destroy(child.gameObject);
            }

            foreach (var skin in skins)
            {
                var skinItem = Instantiate(_skinItemPrefab, _parent);
                skinItem.Fill(() => _view.ShowSkinMenu(skinItem.transform, skin), skin);
                skinItem.gameObject.SetActive(true);
                skinItem.SetActivateCheckMark(IsSkinEquippedInTemp(skin.ItemID));

                _activeSkinItems.Add(skinItem);
            }

            _view.ResetScrollParentVerticalPosition();
        }

        private bool AreEquippedSkinsChanged()
        {
            if (UserDataService.EquippedSkins.Count != _temporaryEquippedSkins.Count)
            {
                return true;
            }

            foreach (var item in UserDataService.EquippedSkins)
            {
                if (_temporaryEquippedSkins.Contains(item) == false)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsSkinEquippedInTemp(string itemID)
        {
            return _temporaryEquippedSkins.Contains(itemID);
        }
    }
}
