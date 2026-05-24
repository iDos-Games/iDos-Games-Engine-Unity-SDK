using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
    /// <summary>
    /// One row in <see cref="CharacterStatPopup"/>. Shows full info for a single
    /// upgradable stat and provides the per-stat upgrade button.
    /// </summary>
    public class CharacterStatPopupItem : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI nameText;      // e.g. "HP 5 level"
        [SerializeField] private TextMeshProUGUI valueText;     // current effect value
        [SerializeField] private GameObject upgradeArrow;       // arrow object (hidden at MAX)
        [SerializeField] private TextMeshProUGUI nextValueText; // value after upgrade (set color = green in prefab)
        [SerializeField] private TextMeshProUGUI costText;      // upgrade cost or "MAX"
        [SerializeField] private Button upgradeButton;

        private string _characterID;
        private string _statID;
        private string _loadedIconPath;

        public void Bind(string statID, StatDefinition def, CharacterModel model,
                         CharacterDefinition charDef, string characterID)
        {
            _characterID = characterID;
            _statID = statID;

            int statLevel = 0;
            model?.StatLevels?.TryGetValue(statID, out statLevel);

            int effectiveMax = CharacterStatMath.ComputeEffectiveMaxLevel(def, charDef, model);
            bool atMax = effectiveMax > 0 && statLevel >= effectiveMax;

            if (nameText != null)
            {
                string displayName = !string.IsNullOrEmpty(def?.DisplayName) ? def.DisplayName : statID;
                nameText.text = $"{displayName} {statLevel} level";
            }

            double currentValue = CharacterStatMath.ComputeValueAtLevel(def, statLevel);
            if (valueText != null) valueText.text = $"{currentValue:0.##}";

            if (atMax)
            {
                if (upgradeArrow != null) upgradeArrow.SetActive(false);
                if (nextValueText != null) nextValueText.gameObject.SetActive(false);
            }
            else
            {
                double nextValue = CharacterStatMath.ComputeValueAtLevel(def, statLevel + 1);
                if (upgradeArrow != null) upgradeArrow.SetActive(true);
                if (nextValueText != null)
                {
                    nextValueText.gameObject.SetActive(true);
                    nextValueText.text = $"{nextValue:0.##}";
                }
            }

            long cost = atMax ? 0 : CharacterStatMath.ComputeUpgradeCost(def, statLevel + 1);

            if (costText != null)
                costText.text = atMax ? "MAX" : (cost > 0 ? cost.ToString() : "Free");

            if (upgradeButton != null)
            {
                upgradeButton.onClick.RemoveAllListeners();
                upgradeButton.onClick.AddListener(OnUpgradeClicked);
                upgradeButton.interactable = !atMax;
            }

            string iconPath = def?.AssetPaths != null
                              && def.AssetPaths.TryGetValue("icon", out var p)
                              && !string.IsNullOrWhiteSpace(p)
                ? p : null;

            if (iconPath != _loadedIconPath)
            {
                _loadedIconPath = iconPath;
                _ = LoadIcon(iconPath);
            }
        }

        private async Task LoadIcon(string path)
        {
            if (string.IsNullOrEmpty(path) || icon == null) return;
            var sprite = await ImageLoader.GetSpriteAsync(path);
            if (sprite != null && this != null && icon != null && _loadedIconPath == path)
                icon.sprite = sprite;
        }

        private async void OnUpgradeClicked()
        {
            if (string.IsNullOrEmpty(_characterID) || string.IsNullOrEmpty(_statID)) return;

            if (upgradeButton != null) upgradeButton.interactable = false;

            var result = await CharacterService.UpgradeStatLevel(_characterID, _statID);

            // CharacterService patches IDosGamesData on success and fires OnStatLevelUpgraded
            // → the parent popup catches it and re-binds us. On failure, re-enable the button.
            if (this == null || result == null) return;
            if (!result.Success && upgradeButton != null) upgradeButton.interactable = true;
        }
    }
}
