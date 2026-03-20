using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using IDosGames.TitlePublicConfiguration;

namespace IDosGames
{
    public class BuildingCard : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image buildingImage;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private List<GameObject> stars;
        [SerializeField] private GameObject damagedOverlay;

        public int SlotIndex { get; private set; }

        private BuildingDefinition _definition;
        private int _currentLevel;
        private bool _isDamaged;
        private bool _maxRewardClaimed;
        private string _loadedImageUrl;

        public void Initialize(int slotIndex)
        {
            SlotIndex = slotIndex;

            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(OnUpgradeClicked);
        }

        public void UpdateView(BuildingDefinition definition, int currentLevel, bool isDamaged, bool maxRewardClaimed, double costGrowthFactor)
        {
            _definition = definition;
            _currentLevel = currentLevel;
            _isDamaged = isDamaged;
            _maxRewardClaimed = maxRewardClaimed;

            string imageUrl = ResolveImageUrl(_definition, _currentLevel);
            if (string.IsNullOrEmpty(imageUrl))
                imageUrl = "Sprites/Currency/IGT";

            if (imageUrl != _loadedImageUrl)
            {
                _loadedImageUrl = imageUrl;
                LoadBuildingImage(imageUrl);
            }

            for (int i = 0; i < stars.Count; i++)
            {
                stars[i].SetActive(i < _currentLevel);
            }

            if (damagedOverlay != null)
            {
                damagedOverlay.SetActive(_isDamaged);
            }

            bool isMaxLevel = _currentLevel >= _definition.MaxLevel;

            if (isMaxLevel)
            {
                priceText.text = _maxRewardClaimed ? "Done" : "Claim";
                upgradeButton.interactable = !_maxRewardClaimed;
            }
            else if (_isDamaged)
            {
                long repairCost = CalculateCost(costGrowthFactor);
                priceText.text = $"Repair: {repairCost}";
                upgradeButton.interactable = true;
            }
            else
            {
                long cost = CalculateCost(costGrowthFactor);
                priceText.text = $"{cost} Coins";
                upgradeButton.interactable = true;
            }
        }

        private static string ResolveImageUrl(BuildingDefinition definition, int currentLevel)
        {
            if (definition?.ImageUrls == null || definition.ImageUrls.Count == 0)
                return null;

            if (definition.ImageUrls.Count == 1)
                return definition.ImageUrls[0];

            int levelIndex = Mathf.Max(0, currentLevel - 1);
            levelIndex = Mathf.Clamp(levelIndex, 0, definition.ImageUrls.Count - 1);

            return definition.ImageUrls[levelIndex];
        }

        private async void LoadBuildingImage(string imageUrl)
        {
            string path = string.IsNullOrEmpty(imageUrl) ? "Sprites/Currency/IGT" : imageUrl;

            var sprite = await ImageLoader.GetSpriteAsync(path);

            if (sprite != null && this != null && buildingImage != null)
            {
                buildingImage.sprite = sprite;
            }
        }

        private long CalculateCost(double costGrowthFactor)
        {
            if (_definition.BaseBuildCost == null || !_definition.BaseBuildCost.Amount.HasValue)
                return 0;

            double baseCost = _definition.BaseBuildCost.Amount.Value;
            double cost = baseCost * System.Math.Pow(costGrowthFactor, _currentLevel) * (_currentLevel + 1);
            return (long)System.Math.Ceiling(cost);
        }

        private async void OnUpgradeClicked()
        {
            upgradeButton.interactable = false;
            await GameLoopService.BoardLoopBuild(SlotIndex);
        }
    }
}
