using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
    /// <summary>
    /// Visual tile of one item. Used in two modes:
    /// <list type="bullet">
    /// <item><see cref="EquipmentItemMode.InventoryGrid"/> — tile inside the bottom scroll grid.
    ///   Class icon hidden, equipped badge shown when the item is equipped elsewhere.</item>
    /// <item><see cref="EquipmentItemMode.SlotChild"/> — tile spawned/enabled inside an
    ///   <see cref="EquipmentSlotView"/>. Class icon shown, equipped badge hidden.</item>
    /// </list>
    /// For unstackable items <c>levelText</c> shows <c>"Lv.N"</c>; for stackable items it shows the count.
    /// </summary>
    public class EquipmentItem : MonoBehaviour
    {
        [Header("Click")]
        [SerializeField] private Button button;

        [Header("Icon")]
        [SerializeField] private Image itemIcon;
        [SerializeField] private Image classIcon;

        [Header("Color Replacement")]
        [SerializeField] private Image bgImage;
        [SerializeField] private Image cornerDecoImage;
        [SerializeField] private Image lightImage;
        [SerializeField] private Image glowImage;
        [SerializeField] private Image circleFrameImage;

        [Header("Level pill")]
        [SerializeField] private TextMeshProUGUI levelText;

        [Header("Equipped state")]
        [Tooltip("Shown only in InventoryGrid mode when the item is equipped somewhere.")]
        [SerializeField] private GameObject equippedBadge;

        private Action<string> _onSelected;
        private string _instanceID;
        private string _loadedIconPath;

        public string ItemInstanceID => _instanceID;

        private void Awake()
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(InvokeClick);
            }
        }

        // -------------------- Public binders --------------------

        /// <summary>Bind for an unstackable instance (real <c>UnstackableItemInstanceState</c>).</summary>
        public void BindUnstackable(
            UnstackableItemInstanceState inst,
            ItemDefinition def,
            EquipmentItemMode mode,
            bool isEquipped,
            ItemRarityVisual rarity,
            Sprite classIconSprite,
            Action<string> onSelected)
        {
            if (inst == null || def == null) { gameObject.SetActive(false); return; }

            _instanceID = inst.ItemInstanceID;
            _onSelected = onSelected;

            ApplyRarity(rarity);
            ApplyMode(mode, isEquipped, classIconSprite);

            if (levelText != null)
            {
                levelText.gameObject.SetActive(true);
                levelText.text = $"Lv.{Mathf.Max(1, inst.Level)}";
            }

            LoadIcon(def);
        }

        /// <summary>Bind for a stackable item (one tile per ItemID with the inventory amount).</summary>
        public void BindStackable(
            string itemID,
            long amount,
            ItemDefinition def,
            EquipmentItemMode mode,
            ItemRarityVisual rarity,
            Sprite classIconSprite,
            Action<string> onSelected)
        {
            if (def == null) { gameObject.SetActive(false); return; }

            _instanceID = itemID; // for stackable items the "id" is the ItemID
            _onSelected = onSelected;

            ApplyRarity(rarity);
            ApplyMode(mode, isEquipped: false, classIconSprite);

            if (levelText != null)
            {
                levelText.gameObject.SetActive(true);
                levelText.text = $"x{amount}";
            }

            LoadIcon(def);
        }

        public void Clear()
        {
            _instanceID = null;
            _loadedIconPath = null;
            if (itemIcon != null) itemIcon.sprite = null;
            if (classIcon != null) classIcon.gameObject.SetActive(false);
            if (equippedBadge != null) equippedBadge.SetActive(false);
            if (levelText != null) levelText.gameObject.SetActive(false);
        }

        // -------------------- Mode + rarity --------------------

        private void ApplyMode(EquipmentItemMode mode, bool isEquipped, Sprite classIconSprite)
        {
            bool isSlot = mode == EquipmentItemMode.SlotChild;

            if (classIcon != null)
            {
                classIcon.gameObject.SetActive(isSlot && classIconSprite != null);
                if (isSlot && classIconSprite != null) classIcon.sprite = classIconSprite;
            }

            if (equippedBadge != null)
                equippedBadge.SetActive(!isSlot && isEquipped);
        }

        private void ApplyRarity(ItemRarityVisual rarity)
        {
            if (rarity == null) return;
            if (bgImage != null)          bgImage.color          = rarity.BgColor;
            if (cornerDecoImage != null)  cornerDecoImage.color  = rarity.CornerDecoColor;
            if (lightImage != null)       lightImage.color       = rarity.LightColor;
            if (glowImage != null)        glowImage.color        = rarity.GlowColor;
            if (circleFrameImage != null) circleFrameImage.color = rarity.CircleFrameColor;
        }

        // -------------------- Icon --------------------

        private void LoadIcon(ItemDefinition def)
        {
            string iconPath = ResolveIconPath(def);
            if (iconPath != _loadedIconPath)
            {
                _loadedIconPath = iconPath;
                _ = LoadIconAsync(iconPath);
            }
        }

        private static string ResolveIconPath(ItemDefinition def)
        {
            if (def?.AssetPaths == null) return null;
            if (def.AssetPaths.TryGetValue("icon", out var icon) && !string.IsNullOrWhiteSpace(icon)) return icon;
            foreach (var v in def.AssetPaths.Values)
                if (!string.IsNullOrWhiteSpace(v)) return v;
            return null;
        }

        private async Task LoadIconAsync(string path)
        {
            if (string.IsNullOrEmpty(path) || itemIcon == null) return;
            var sprite = await ImageLoader.GetSpriteAsync(path);
            if (sprite != null && this != null && itemIcon != null && _loadedIconPath == path)
                itemIcon.sprite = sprite;
        }

        private void InvokeClick() => _onSelected?.Invoke(_instanceID);
    }

    public enum EquipmentItemMode
    {
        InventoryGrid,
        SlotChild,
    }
}
