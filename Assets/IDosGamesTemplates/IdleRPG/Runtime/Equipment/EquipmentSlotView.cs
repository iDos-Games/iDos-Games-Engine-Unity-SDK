using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
    /// <summary>
    /// One of the equipment slots around the character in <see cref="EquipmentPanel"/>.
    /// Click opens <see cref="EquipmentInfoPopup"/> for the equipped instance (or in "empty" mode
    /// to let the player pick something to put here).
    /// </summary>
    public class EquipmentSlotView : MonoBehaviour
    {
        [Header("Identity")]
        [Tooltip("Slot ID — must match a key in the active character's CharacterEquipment.Slots.")]
        [SerializeField] private string slotID;

        [Header("Click")]
        [SerializeField] private Button button;

        [Header("Frame & icon")]
        [SerializeField] private Image frameImage;
        [SerializeField] private Image itemIcon;
        [SerializeField] private GameObject emptyState;

        [Header("Level pill")]
        [SerializeField] private GameObject levelPill;
        [SerializeField] private TextMeshProUGUI levelText;

        [Header("Lock overlay")]
        [SerializeField] private GameObject lockedOverlay;

        private Action<string, string> _onClick;
        private string _loadedIconPath;
        private string _instanceID;

        public string SlotID => slotID;

        private void Awake()
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(InvokeClick);
            }
        }

        /// <summary>
        /// Bind a slot view. When <paramref name="inst"/> is null the slot renders as empty.
        /// When <paramref name="isLocked"/> is true (character level &lt; rule.MinCharacterLevel),
        /// the lock overlay is shown and the click is blocked.
        /// </summary>
        public void Bind(
            EquippedItem equipped,
            UnstackableItemInstanceState inst,
            ItemDefinition def,
            bool isLocked,
            ItemRarityVisual rarity,
            Action<string, string> onClick)
        {
            _onClick = onClick;
            _instanceID = inst?.ItemInstanceID;

            bool hasItem = inst != null && def != null;

            if (emptyState != null) emptyState.SetActive(!hasItem);
            if (itemIcon != null) itemIcon.enabled = hasItem;
            if (levelPill != null) levelPill.SetActive(hasItem);
            if (lockedOverlay != null) lockedOverlay.SetActive(isLocked);

            if (button != null) button.interactable = !isLocked;

            if (frameImage != null && rarity != null)
            {
                if (rarity.FrameSprite != null) frameImage.sprite = rarity.FrameSprite;
                frameImage.color = rarity.FrameTint;
            }

            if (hasItem)
            {
                if (levelText != null) levelText.text = $"Lv.{Mathf.Max(1, inst.Level)}";

                string iconPath = ResolveIconPath(def);
                if (iconPath != _loadedIconPath)
                {
                    _loadedIconPath = iconPath;
                    _ = LoadIcon(iconPath);
                }
            }
            else
            {
                _loadedIconPath = null;
                if (itemIcon != null) itemIcon.sprite = null;
            }
        }

        private void InvokeClick() => _onClick?.Invoke(slotID, _instanceID);

        private static string ResolveIconPath(ItemDefinition def)
        {
            if (def?.AssetPaths == null) return null;
            if (def.AssetPaths.TryGetValue("icon", out var icon) && !string.IsNullOrWhiteSpace(icon)) return icon;
            foreach (var v in def.AssetPaths.Values)
                if (!string.IsNullOrWhiteSpace(v)) return v;
            return null;
        }

        private async Task LoadIcon(string path)
        {
            if (string.IsNullOrEmpty(path) || itemIcon == null) return;
            var sprite = await ImageLoader.GetSpriteAsync(path);
            if (sprite != null && this != null && itemIcon != null && _loadedIconPath == path)
                itemIcon.sprite = sprite;
        }
    }
}
