using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
    /// <summary>
    /// One tile in <see cref="EquipmentPanel"/>'s scroll grid. Click opens
    /// <see cref="EquipmentInfoPopup"/> for this instance.
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
        [Tooltip("Shown when the item is already equipped on some character.")]
        [SerializeField] private GameObject equippedBadge;

        private Action<string> _onSelected;
        private string _instanceID;
        private string _loadedIconPath;

        public string ItemInstanceID => _instanceID;

        public void Bind(
            UnstackableItemInstanceState inst,
            ItemDefinition def,
            bool isEquipped,
            ItemRarityVisual rarity,
            Action<string> onSelected)
        {
            if (inst == null) return;

            _instanceID = inst.ItemInstanceID;
            _onSelected = onSelected;

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(InvokeClick);
            }

            if (levelText != null) levelText.text = $"Lv.{Mathf.Max(1, inst.Level)}";
            if (equippedBadge != null) equippedBadge.SetActive(isEquipped);

            string iconPath = ResolveIconPath(def);
            if (iconPath != _loadedIconPath)
            {
                _loadedIconPath = iconPath;
                _ = LoadIcon(iconPath);
            }
        }

        private void InvokeClick() => _onSelected?.Invoke(_instanceID);

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
