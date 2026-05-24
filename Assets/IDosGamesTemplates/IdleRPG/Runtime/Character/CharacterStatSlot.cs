using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
    /// <summary>
    /// Compact stat display used in <see cref="CharacterDetailPanel"/>'s 4 fixed slots.
    /// Click opens <see cref="CharacterStatPopup"/> via the supplied callback.
    /// </summary>
    public class CharacterStatSlot : MonoBehaviour
    {
        [Header("Click")]
        [SerializeField] private Button button;

        [Header("Refs")]
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI valueText;

        private string _loadedIconPath;
        private Action _onClick;

        public void Bind(string statID, StatDefinition def, int statLevel, Action onClick)
        {
            _onClick = onClick;

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(InvokeClick);
            }

            if (nameText != null)
                nameText.text = !string.IsNullOrEmpty(def?.DisplayName) ? def.DisplayName : statID;

            if (valueText != null)
                valueText.text = $"{CharacterStatMath.ComputeValueAtLevel(def, statLevel):0.##}";

            string iconPath = TryGetIconPath(def);
            if (iconPath != _loadedIconPath)
            {
                _loadedIconPath = iconPath;
                _ = LoadIcon(iconPath);
            }
        }

        private void InvokeClick() => _onClick?.Invoke();

        private static string TryGetIconPath(StatDefinition def)
        {
            if (def?.AssetPaths == null) return null;
            return def.AssetPaths.TryGetValue("icon", out var p) && !string.IsNullOrWhiteSpace(p) ? p : null;
        }

        private async Task LoadIcon(string path)
        {
            if (string.IsNullOrEmpty(path) || icon == null) return;
            var sprite = await ImageLoader.GetSpriteAsync(path);
            if (sprite != null && this != null && icon != null && _loadedIconPath == path)
                icon.sprite = sprite;
        }
    }
}
