using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace IDosGames.UserProfile
{
    public class AvatarItemsPanel : MonoBehaviour
    {
        [SerializeField] private UserProfileWindow _avatarWindow;
        [SerializeField] private AvatarItem _item;
        [SerializeField] private RectTransform _content;
        [Header("Tabs")]
        [SerializeField] private TabItem _bodyTab;
        [SerializeField] private TabItem _glassesTab;
        [SerializeField] private TabItem _handsTab;
        [SerializeField] private TabItem _hatTab;
        [SerializeField] private TabItem _maskTab;
        [SerializeField] private TabItem _pantsTab;
        [SerializeField] private TabItem _shoesTab;
        [SerializeField] private TabItem _torsoTab;

        [SerializeField] private TabItem _myItemTab;
        [SerializeField] private TabItem _catalogItemTab;

        [Header("VoidText")]
        [SerializeField] private TMP_Text _voidInventoryText;
        [SerializeField] private TMP_Text _voidCatalogText;

        [SerializeField] private ScrollRect _scrollRect;
        public ScrollRect ScrollRect => _scrollRect;

        [SerializeField] private AvatarItemMenu _avatarMenu;

        private List<AvatarItem> _panelItems = new List<AvatarItem>();
        private TabItem _lastActiveTab;

        private ClothingType _clothingType;

        private void OnEnable()
        {
            SelectBodyTab();
        }



        private void ShowSkinMenu(Transform transform, AvatarSkinCatalogItem item)
        {
            _avatarMenu.ShowPopUp(transform, item);
        }

        public void ResetScrollParentVerticalPosition()
        {
            _scrollRect.verticalNormalizedPosition = 1f;
        }

        public void RefreshItem(string itemID)
        {
            foreach (var item in _panelItems)
            {
                if (item.AvatarData.ItemID == itemID)
                {
                    var isEquip = _avatarWindow.UserAvatar.IsSkinEquippedInTemp(itemID);
                    item.SetActivateCheckMark(isEquip);
                    if (!isEquip)
                    {
                        item.SetActivateCheckMark(_avatarWindow.UserAvatar.IsSkinEquipAsInspect(itemID));
                    }


                }
                else
                {
                    item.SetActivateCheckMark(false);
                }

            }
        }

        private void InitItems(List<AvatarSkinCatalogItem> skins)
        {
            _panelItems.Clear();

            foreach (Transform child in _content)
            {
                Destroy(child.gameObject);
            }

            foreach (var skin in skins)
            {

                var skinItem = Instantiate(_item, _content);
                skinItem.Fill(() => ShowSkinMenu(skinItem.transform, skin), skin);
                skinItem.gameObject.SetActive(true);
                _panelItems.Add(skinItem);
                var isEquip = _avatarWindow.UserAvatar.IsSkinEquippedInTemp(skin.ItemID);

                skinItem.SetActivateCheckMark(isEquip);
                if (!isEquip)
                {
                    skinItem.SetActivateCheckMark(_avatarWindow.UserAvatar.IsSkinEquipAsInspect(skin.ItemID));
                }
            }
            ResetScrollParentVerticalPosition();


        }

        public void OpenUserInventoryItems()
        {
            _myItemTab.Select();
            _catalogItemTab.Deselect();
            UpdateUserInventoryItemsItems();
        }

        public void OpenCatalogItems()
        {
            _myItemTab.Deselect();
            _catalogItemTab.Select();
            UpdateCatalogItems();
        }

        private void UpdateUserInventoryItemsItems()
        {
            _voidCatalogText.gameObject.SetActive(false);

            List<AvatarSkinCatalogItem> skins = new();

            foreach (var skin in UserDataService.AllAvatarSkinsInCatalog)
            {
                if (skin.AvatarType == _clothingType.ToString().ToLower())
                {
                    var amount = UserInventory.GetItemAmount(skin.ItemID);
                    if (amount > 0)
                    {
                        skins.Add(skin);

                    }
                }
            }
            if (skins.Count == 0)
            {
                _voidInventoryText.gameObject.SetActive(true);
            }
            else
            {
                _voidInventoryText.gameObject.SetActive(false);
            }

            skins = skins.OrderBy(o => o.Rarity).Reverse().ToList(); //Sorted by rarity
            InitItems(skins);
        }

        private void UpdateCatalogItems()
        {
            _voidInventoryText.gameObject.SetActive(false);
            List<AvatarSkinCatalogItem> skins = new();


            foreach (var skin in UserDataService.AllAvatarSkinsInCatalog)
            {
                if (skin.AvatarType == _clothingType.ToString().ToLower())
                {
                    skins.Add(skin);
                }
            }

            if (skins.Count == 0)
            {
                _voidCatalogText.gameObject.SetActive(true);
            }
            else
            {
                _voidCatalogText.gameObject.SetActive(false);
            }

            skins = skins.OrderBy(o => o.Rarity).Reverse().ToList(); //Sorted by rarity
            InitItems(skins);
        }

        public void SelectBodyTab()
        {
            _bodyTab.Select();
            _lastActiveTab?.Deselect();
            _lastActiveTab = _bodyTab;
            _clothingType = ClothingType.Body;
            OpenUserInventoryItems();
            _avatarWindow.MoveCameraTo(_clothingType);
            _avatarWindow.UserAvatar.UnequipInspectSkin();
            _avatarWindow.UserAvatar.EquipRemovedSkin();

        }

        public void SelectGlassesTab()
        {
            _glassesTab.Select();
            _lastActiveTab?.Deselect();
            _lastActiveTab = _glassesTab;
            _clothingType = ClothingType.Glasses;
            OpenUserInventoryItems();
            _avatarWindow.MoveCameraTo(_clothingType);
            _avatarWindow.UserAvatar.UnequipInspectSkin();
            _avatarWindow.UserAvatar.EquipRemovedSkin();

        }

        public void SelectHandsTab()
        {
            _handsTab.Select();
            _lastActiveTab?.Deselect();
            _lastActiveTab = _handsTab;
            _clothingType = ClothingType.Hands;
            OpenUserInventoryItems();
            _avatarWindow.MoveCameraTo(_clothingType);
            _avatarWindow.UserAvatar.UnequipInspectSkin();
            _avatarWindow.UserAvatar.EquipRemovedSkin();
        }

        public void SelectHatTab()
        {
            _hatTab.Select();
            _lastActiveTab?.Deselect();
            _lastActiveTab = _hatTab;
            _clothingType = ClothingType.Hat;
            OpenUserInventoryItems();
            _avatarWindow.MoveCameraTo(_clothingType);
            _avatarWindow.UserAvatar.UnequipInspectSkin();
            _avatarWindow.UserAvatar.EquipRemovedSkin();
        }

        public void SelectMaskTab()
        {
            _maskTab.Select();
            _lastActiveTab?.Deselect();
            _lastActiveTab = _maskTab;
            _clothingType = ClothingType.Mask;
            OpenUserInventoryItems();
            _avatarWindow.MoveCameraTo(_clothingType);
            _avatarWindow.UserAvatar.UnequipInspectSkin();
            _avatarWindow.UserAvatar.EquipRemovedSkin();
        }

        public void SelectPantsTab()
        {
            _pantsTab.Select();
            _lastActiveTab?.Deselect();
            _lastActiveTab = _pantsTab;
            _clothingType = ClothingType.Pants;
            OpenUserInventoryItems();
            _avatarWindow.MoveCameraTo(_clothingType);
            _avatarWindow.UserAvatar.UnequipInspectSkin();
            _avatarWindow.UserAvatar.EquipRemovedSkin();
        }

        public void SelectShoesTab()
        {
            _shoesTab.Select();
            _lastActiveTab?.Deselect();
            _lastActiveTab = _shoesTab;
            _clothingType = ClothingType.Shoes;
            OpenUserInventoryItems();
            _avatarWindow.MoveCameraTo(_clothingType);
            _avatarWindow.UserAvatar.UnequipInspectSkin();
            _avatarWindow.UserAvatar.EquipRemovedSkin();
        }

        public void SelectTorsoTab()
        {
            _torsoTab.Select();
            _lastActiveTab?.Deselect();
            _lastActiveTab = _torsoTab;
            _clothingType = ClothingType.Torso;
            OpenUserInventoryItems();
            _avatarWindow.MoveCameraTo(_clothingType);
            _avatarWindow.UserAvatar.UnequipInspectSkin();
            _avatarWindow.UserAvatar.EquipRemovedSkin();
        }

    }
}
