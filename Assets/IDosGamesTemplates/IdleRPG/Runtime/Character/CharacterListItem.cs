using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
    [Serializable]
    public class CharacterRarityVisual
    {
        public string RarityID;

        [Header("Background (3 layers)")]
        public Color BgColor = Color.white;
        public Color GradientColor = Color.white;
        public Color GlowColor = Color.white;

        [Header("Stars")]
        public int StarCount = 1;
        public bool UseSpecialStars;
    }

    [Serializable]
    public class CharacterClassIcon
    {
        public string ClassID;
        public Sprite Icon;
    }

    public class CharacterListItem : MonoBehaviour
    {
        [Header("Click")]
        [Tooltip("Button covering the whole card. See script summary for setup.")]
        [SerializeField] private Button cardButton;

        [Header("Background (3 layers)")]
        [SerializeField] private Image bgImage;
        [SerializeField] private Image gradientImage;
        [SerializeField] private Image glowImage;

        [Header("Class icon")]
        [SerializeField] private Image classIcon;

        [Header("Text")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI levelText;

        [Header("Level badge")]
        [SerializeField] private Image badgeImage;
        [SerializeField] private Sprite badgeNormalSprite;
        [SerializeField] private Sprite badgeMaxSprite;

        [Header("Stars")]
        [SerializeField] private List<GameObject> stars;
        [SerializeField] private List<GameObject> specialStars;

        [Header("Locked overlay")]
        [Tooltip("Darkening overlay shown when the character is not unlocked. Should block raycasts.")]
        [SerializeField] private GameObject lockedOverlay;

        [Header("Unlock")]
        [Tooltip("Button shown on top of the locked overlay. Calls CharacterService.UnlockCharacter on click.")]
        [SerializeField] private Button unlockButton;
        [Tooltip("Optional cost label inside the unlock button. Shows the unlock cost amount or 'Free'.")]
        [SerializeField] private TextMeshProUGUI unlockCostText;

        [Header("Rarity palette (per RarityID)")]
        [SerializeField] private List<CharacterRarityVisual> rarityVisuals;

        [Header("Class icons (per ClassID)")]
        [SerializeField] private List<CharacterClassIcon> classIcons;

        private string _characterID;
        private Action<string> _onSelected;

        public string CharacterID => _characterID;

        public void Bind(CharacterModel model, CharacterDefinition def, bool isLocked, Action<string> onSelected)
        {
            if (model == null) return;

            _characterID = model.CharacterID;
            _onSelected = onSelected;

            if (cardButton != null)
            {
                cardButton.onClick.RemoveAllListeners();
                cardButton.onClick.AddListener(OnClicked);
                cardButton.interactable = !isLocked;
            }

            if (lockedOverlay != null) lockedOverlay.SetActive(isLocked);

            ApplyUnlockButton(isLocked, def);

            if (nameText != null)
            {
                nameText.text = !string.IsNullOrEmpty(model.Name)
                    ? model.Name
                    : def?.Identity?.DisplayName ?? model.CharacterID;
            }

            if (levelText != null)
                levelText.text = model.Level.ToString();

            ApplyBadge(IsMaxLevel(model, def));
            ApplyRarityVisual(ResolveRarity(def?.Classification?.RarityID));
            ApplyClassIcon(def?.Classification?.ClassID ?? model.Class);
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

        private void ApplyBadge(bool isMax)
        {
            if (badgeImage == null) return;

            var sprite = isMax ? badgeMaxSprite : badgeNormalSprite;
            if (sprite != null) badgeImage.sprite = sprite;
        }

        private void OnClicked() => _onSelected?.Invoke(_characterID);

        private void ApplyUnlockButton(bool isLocked, CharacterDefinition def)
        {
            if (unlockButton == null) return;

            var cost = def?.Unlock?.Cost;
            bool canUnlockByCost = isLocked && cost != null;

            unlockButton.gameObject.SetActive(canUnlockByCost);
            unlockButton.onClick.RemoveAllListeners();

            if (!canUnlockByCost) return;

            unlockButton.interactable = true;
            unlockButton.onClick.AddListener(OnUnlockClicked);

            if (unlockCostText != null)
            {
                long amount = ExtractCost(cost);
                unlockCostText.text = amount > 0 ? amount.ToString() : "Free";
            }
        }

        private async void OnUnlockClicked()
        {
            if (string.IsNullOrEmpty(_characterID)) return;
            if (unlockButton == null) return;

            unlockButton.interactable = false;

            var result = await CharacterService.UnlockCharacter(_characterID);

            // CharacterService patches IDosGamesData.User on success and fires OnCharacterUpdated,
            // which triggers CharacterListPanel.Refresh and re-binds this item (isLocked=false).
            // On failure we just re-enable the button so the user can retry.
            if (this == null || result == null) return;
            if (!result.Success && unlockButton != null)
                unlockButton.interactable = true;
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

        private CharacterRarityVisual ResolveRarity(string rarityID)
        {
            if (rarityVisuals == null || rarityVisuals.Count == 0) return null;

            if (!string.IsNullOrEmpty(rarityID))
            {
                for (int i = 0; i < rarityVisuals.Count; i++)
                {
                    var v = rarityVisuals[i];
                    if (v != null && string.Equals(v.RarityID, rarityID, StringComparison.OrdinalIgnoreCase))
                        return v;
                }
            }
            return rarityVisuals[0];
        }

        private void ApplyRarityVisual(CharacterRarityVisual visual)
        {
            if (visual == null) return;

            if (bgImage != null)       bgImage.color       = visual.BgColor;
            if (gradientImage != null) gradientImage.color = visual.GradientColor;
            if (glowImage != null)     glowImage.color     = visual.GlowColor;

            ApplyStars(visual.UseSpecialStars ? specialStars : stars, visual.StarCount);
            ApplyStars(visual.UseSpecialStars ? stars : specialStars, 0);
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
                if (c != null && string.Equals(c.ClassID, classID, StringComparison.OrdinalIgnoreCase))
                {
                    if (c.Icon != null) classIcon.sprite = c.Icon;
                    return;
                }
            }
        }
    }
}
