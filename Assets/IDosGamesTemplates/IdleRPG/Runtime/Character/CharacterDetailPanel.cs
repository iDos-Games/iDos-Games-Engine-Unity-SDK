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

        [Header("Color Replacement")]
        [SerializeField] private Image bgImage;
        [SerializeField] private Image gradientImage;
        [SerializeField] private List<Image> basicFrameImages;

        [Header("Content")]
        [SerializeField] private Image characterImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI loreText;

        [Header("Class icon")]
        [SerializeField] private Image classIcon;

        [Header("Level badge")]
        [SerializeField] private Image badgeImage;
        [SerializeField] private Sprite badgeNormalSprite;
        [SerializeField] private Sprite badgeMaxSprite;

        [Header("Stars")]
        [SerializeField] private List<GameObject> stars;
        [SerializeField] private List<GameObject> specialStars;

        [Header("Stat slots (4 fixed)")]
        [SerializeField] private List<CharacterStatSlot> statSlots;
        [SerializeField] private CharacterStatPopup statPopup;

        [Header("Upgrade")]
        [SerializeField] private Button upgradeButton;
        [SerializeField] private TextMeshProUGUI upgradeCostText;
        [SerializeField] private Button upgradeStatsButton;

        [Header("Back")]
        [SerializeField] private Button backButton;

        [Header("Class icons (per ClassID)")]
        [SerializeField] private List<CharacterClassIcon> classIcons;

        [Header("Rarity label")]
        [SerializeField] private Image rarityLabelImage;
        [SerializeField] private TextMeshProUGUI rarityLabelText;
        [SerializeField] private List<CharacterRarityLabel> rarityLabels;

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

            if (upgradeStatsButton != null)
            {
                upgradeStatsButton.onClick.RemoveAllListeners();
                upgradeStatsButton.onClick.AddListener(OpenStatPopup);
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
            if (loreText != null) loreText.text = def?.Identity?.Lore ?? string.Empty;

            ApplyBadge(IsMaxLevel(model, def));
            ApplyRarity(def?.Classification?.RarityID);
            ApplyClassIcon(def?.Classification?.ClassID ?? model.Class);
            ApplyUpgradeButton(model, def);
            ApplyStatSlots(model, def);

            string imgPath = ResolveBigImagePath(def);
            if (imgPath != _loadedImagePath)
            {
                _loadedImagePath = imgPath;
                _ = LoadCharacterImage(imgPath);
            }
        }

        private void ApplyRarity(string rarityID)
        {
            if (rarityLabels == null || rarityLabels.Count == 0) return;

            CharacterRarityLabel label = null;
            if (!string.IsNullOrEmpty(rarityID))
            {
                for (int i = 0; i < rarityLabels.Count; i++)
                {
                    var l = rarityLabels[i];
                    if (l != null && string.Equals(l.RarityID, rarityID, System.StringComparison.OrdinalIgnoreCase))
                    {
                        label = l;
                        break;
                    }
                }
            }
            if (label == null) label = rarityLabels[0];
            if (label == null) return;

            if (rarityLabelImage != null && label.LabelSprite != null)
                rarityLabelImage.sprite = label.LabelSprite;

            if (rarityLabelText != null)
                rarityLabelText.text = label.DisplayName ?? string.Empty;

            if (bgImage != null)       bgImage.color       = label.BgColor;
            if (gradientImage != null) gradientImage.color = label.GradientColor;

            if (basicFrameImages != null)
            {
                for (int i = 0; i < basicFrameImages.Count; i++)
                    if (basicFrameImages[i] != null)
                        basicFrameImages[i].color = label.BasicFrameColor;
            }

            ApplyStars(label.UseSpecialStars ? specialStars : stars, label.StarCount);
            ApplyStars(label.UseSpecialStars ? stars : specialStars, 0);
        }

        private static void ApplyStars(List<GameObject> list, int activeCount)
        {
            if (list == null) return;
            for (int i = 0; i < list.Count; i++)
                if (list[i] != null)
                    list[i].SetActive(i < activeCount);
        }

        private void ApplyClassIcon(string classID)
        {
            if (classIcon == null) return;
            if (classIcons == null || classIcons.Count == 0) return;
            if (string.IsNullOrEmpty(classID)) return;

            for (int i = 0; i < classIcons.Count; i++)
            {
                var c = classIcons[i];
                if (c != null && string.Equals(c.ClassID, classID, System.StringComparison.OrdinalIgnoreCase))
                {
                    if (c.Icon != null) classIcon.sprite = c.Icon;
                    return;
                }
            }
        }

        private void ApplyBadge(bool isMax)
        {
            if (badgeImage == null) return;

            var sprite = isMax ? badgeMaxSprite : badgeNormalSprite;
            if (sprite != null) badgeImage.sprite = sprite;
        }

        private static bool IsMaxLevel(CharacterModel model, CharacterDefinition def)
        {
            if (model == null || def?.Levels == null || def.Levels.Count == 0) return false;

            int max = 0;
            foreach (var key in def.Levels.Keys)
                if (int.TryParse(key, out var n) && n > max)
                    max = n;

            return max > 0 && model.Level >= max;
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

        private void ApplyStatSlots(CharacterModel model, CharacterDefinition def)
        {
            if (statSlots == null || statSlots.Count == 0) return;

            int slotIndex = 0;
            if (def?.Stats != null)
            {
                foreach (var kv in def.Stats)
                {
                    if (slotIndex >= statSlots.Count) break;
                    if (kv.Value == null) continue;

                    var slot = statSlots[slotIndex];
                    if (slot == null) { slotIndex++; continue; }

                    int statLevel = 0;
                    model?.StatLevels?.TryGetValue(kv.Key, out statLevel);

                    slot.gameObject.SetActive(true);
                    slot.Bind(kv.Key, kv.Value, statLevel, OpenStatPopup);
                    slotIndex++;
                }
            }

            for (int i = slotIndex; i < statSlots.Count; i++)
                if (statSlots[i] != null)
                    statSlots[i].gameObject.SetActive(false);
        }

        private void OpenStatPopup()
        {
            if (statPopup != null && !string.IsNullOrEmpty(_characterID))
                statPopup.Show(_characterID);
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
