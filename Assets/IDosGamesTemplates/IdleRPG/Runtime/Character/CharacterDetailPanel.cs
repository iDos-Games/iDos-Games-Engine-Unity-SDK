using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
    public class CharacterDetailPanel : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField] private GameObject root;
        [SerializeField] private Image backgroundMask;
        [SerializeField] private Image frame;

        [Header("Content")]
        [SerializeField] private Image characterImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI classText;
        [SerializeField] private TextMeshProUGUI powerText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private List<GameObject> stars;

        [Header("Resource bar (Top)")]
        [SerializeField] private TextMeshProUGUI resourceText1;
        [SerializeField] private TextMeshProUGUI resourceText2;

        [Header("Upgrade")]
        [SerializeField] private Button upgradeButton;
        [SerializeField] private TextMeshProUGUI upgradeCostText;

        [Header("Back")]
        [SerializeField] private Button backButton;

        [Header("Rarity palette")]
        [SerializeField] private List<CharacterRarityVisual> rarityVisuals;

        private string _characterID;
        private string _loadedImagePath;

        private void Awake()
        {
            if (root == null) root = gameObject;
            root.SetActive(false);

            if (upgradeButton != null)
            {
                upgradeButton.onClick.RemoveAllListeners();
                upgradeButton.onClick.AddListener(OnUpgradeClicked);
            }

            if (backButton != null)
            {
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(Hide);
            }
        }

        private void OnEnable()
        {
            CharacterService.OnCharacterLevelUpgraded += HandleCharacterLevelUpgraded;
            CharacterService.OnStatLevelUpgraded      += HandleStatLevelUpgraded;

            if (IDosGamesData.User != null)
                IDosGamesData.User.OnCharacterUpdated += RefreshFromCache;
        }

        private void OnDisable()
        {
            CharacterService.OnCharacterLevelUpgraded -= HandleCharacterLevelUpgraded;
            CharacterService.OnStatLevelUpgraded      -= HandleStatLevelUpgraded;

            if (IDosGamesData.User != null)
                IDosGamesData.User.OnCharacterUpdated -= RefreshFromCache;
        }

        public void Show(string characterID)
        {
            if (string.IsNullOrEmpty(characterID)) return;

            _characterID = characterID;
            _loadedImagePath = null;
            EnsureRoot().SetActive(true);
            RefreshFromCache();
        }

        public void Hide()
        {
            EnsureRoot().SetActive(false);
            _characterID = null;
            _loadedImagePath = null;
        }

        private GameObject EnsureRoot()
        {
            if (root == null) root = gameObject;
            return root;
        }

        private void RefreshFromCache()
        {
            if (string.IsNullOrEmpty(_characterID)) return;

            var characters = IDosGamesData.User?.State?.Character?.Characters;
            if (characters == null || !characters.TryGetValue(_characterID, out var model) || model == null)
            {
                Hide();
                return;
            }

            CharacterDefinition def = null;
            var defs = IDosGamesData.Config?.TitlePublicConfiguration?.Character?.Definitions;
            if (defs != null) defs.TryGetValue(_characterID, out def);

            if (nameText != null)
            {
                nameText.text = !string.IsNullOrEmpty(model.Name)
                    ? model.Name
                    : def?.Identity?.DisplayName ?? model.CharacterID;
            }
            if (levelText != null) levelText.text = $"Lv. {model.Level}";
            if (classText != null) classText.text = def?.Classification?.ClassID ?? model.Class ?? string.Empty;
            if (powerText != null) powerText.text = model.Power.ToString();
            if (descriptionText != null) descriptionText.text = def?.Identity?.Description ?? string.Empty;

            ApplyRarityVisual(Resolve(def?.Classification?.RarityID));
            ApplyUpgradeButton(model, def);
            ApplyResourceBar(model, def);

            string imgPath = ResolveBigImagePath(def);
            if (imgPath != _loadedImagePath)
            {
                _loadedImagePath = imgPath;
                _ = LoadCharacterImage(imgPath);
            }
        }

        private void ApplyRarityVisual(CharacterRarityVisual visual)
        {
            if (visual == null) return;

            if (backgroundMask != null) backgroundMask.color = visual.BgColor;
            
            if (stars != null)
            {
                for (int i = 0; i < stars.Count; i++)
                    if (stars[i] != null)
                        stars[i].SetActive(i < visual.StarCount);
            }
        }

        private void ApplyUpgradeButton(CharacterModel model, CharacterDefinition def)
        {
            if (upgradeButton == null) return;

            int nextLevel = Mathf.Max(1, model.Level + 1);
            CharacterLevelDefinition nextDef = null;
            if (def?.Levels != null)
                def.Levels.TryGetValue(nextLevel.ToString(), out nextDef);

            if (nextDef == null)
            {
                if (upgradeCostText != null) upgradeCostText.text = "MAX";
                upgradeButton.interactable = false;
                return;
            }

            long cost = ExtractCost(nextDef.UpgradeCost);
            if (upgradeCostText != null)
                upgradeCostText.text = cost > 0 ? cost.ToString() : "Free";
            upgradeButton.interactable = true;
        }

        private void ApplyResourceBar(CharacterModel model, CharacterDefinition def)
        {
            string line1 = null;
            string line2 = null;

            if (def?.Stats != null && def.Stats.Count > 0)
            {
                int slot = 0;
                foreach (var kv in def.Stats)
                {
                    var statDef = kv.Value;
                    if (statDef == null) continue;

                    int statLevel = 0;
                    model.StatLevels?.TryGetValue(kv.Key, out statLevel);

                    double value = statDef.BaseStatValue;
                    if (statLevel > 1)
                        value = statDef.BaseStatValue * (1.0 + statDef.StatScalingFactor * (statLevel - 1));

                    string label = !string.IsNullOrEmpty(statDef.DisplayName) ? statDef.DisplayName : kv.Key;
                    string line = $"{label}: {value:0.##}";

                    if (slot == 0) line1 = line;
                    else if (slot == 1) line2 = line;
                    else break;
                    slot++;
                }
            }

            if (resourceText1 != null) resourceText1.text = line1 ?? string.Empty;
            if (resourceText2 != null) resourceText2.text = line2 ?? string.Empty;
        }

        private CharacterRarityVisual Resolve(string rarityID)
        {
            if (rarityVisuals == null || rarityVisuals.Count == 0) return null;

            if (!string.IsNullOrEmpty(rarityID))
            {
                for (int i = 0; i < rarityVisuals.Count; i++)
                {
                    var v = rarityVisuals[i];
                    if (v != null && string.Equals(v.RarityID, rarityID, System.StringComparison.OrdinalIgnoreCase))
                        return v;
                }
            }
            return rarityVisuals[0];
        }

        private static long ExtractCost(ResourceConsume consume)
        {
            var entries = consume?.Standard?.Entries;
            if (entries == null) return 0;
            for (int i = 0; i < entries.Count; i++)
                if (entries[i] != null && entries[i].Amount.HasValue)
                    return entries[i].Amount.Value;
            return 0;
        }

        private static string ResolveBigImagePath(CharacterDefinition def)
        {
            var assetPaths = def?.Identity?.AssetPaths;
            if (assetPaths == null || assetPaths.Count == 0) return null;

            if (assetPaths.TryGetValue("portrait", out var portrait) && !string.IsNullOrWhiteSpace(portrait))
                return portrait;
            if (assetPaths.TryGetValue("fullArt", out var fullArt) && !string.IsNullOrWhiteSpace(fullArt))
                return fullArt;
            if (assetPaths.TryGetValue("icon", out var icon) && !string.IsNullOrWhiteSpace(icon))
                return icon;

            foreach (var value in assetPaths.Values)
                if (!string.IsNullOrWhiteSpace(value))
                    return value;

            return null;
        }

        private async System.Threading.Tasks.Task LoadCharacterImage(string path)
        {
            if (string.IsNullOrEmpty(path)) return;

            var sprite = await ImageLoader.GetSpriteAsync(path);
            if (sprite != null && this != null && characterImage != null && _loadedImagePath == path)
                characterImage.sprite = sprite;
        }

        private async void OnUpgradeClicked()
        {
            if (string.IsNullOrEmpty(_characterID)) return;

            if (upgradeButton != null) upgradeButton.interactable = false;

            var result = await CharacterService.UpgradeCharacterLevel(_characterID);

            // CharacterService patches IDosGamesData.User on success and fires OnCharacterUpdated,
            // which triggers RefreshFromCache. On failure we re-enable the button via Refresh.
            if (this == null || result == null) return;
            if (!result.Success) RefreshFromCache();
        }

        private void HandleCharacterLevelUpgraded(UpgradeCharacterLevelResponse data)
        {
            if (data != null && data.CharacterID == _characterID)
                RefreshFromCache();
        }

        private void HandleStatLevelUpgraded(UpgradeStatLevelResponse data)
        {
            if (data != null && data.CharacterID == _characterID)
                RefreshFromCache();
        }
    }
}
