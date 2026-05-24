using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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
                priceText.text = $"{repairCost}";
                upgradeButton.interactable = true;
            }
            else
            {
                long cost = CalculateCost(costGrowthFactor);
                priceText.text = $"{cost}";
                upgradeButton.interactable = true;
            }
        }

        private static string ResolveImageUrl(BuildingDefinition definition, int currentLevel)
        {
            var assetPaths = definition?.AssetPaths;
            if (assetPaths == null || assetPaths.Count == 0)
                return null;

            if (assetPaths.TryGetValue($"level_{currentLevel}", out var perLevel) && !string.IsNullOrWhiteSpace(perLevel))
                return perLevel;

            if (assetPaths.TryGetValue("icon", out var icon) && !string.IsNullOrWhiteSpace(icon))
                return icon;

            foreach (var value in assetPaths.Values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }

            return null;
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
            long baseCost = GetBaseBuildCostAmount(_definition?.BaseBuildCost);
            if (baseCost <= 0)
                return 0;

            double cost = baseCost * System.Math.Pow(costGrowthFactor, _currentLevel) * (_currentLevel + 1);
            return (long)System.Math.Ceiling(cost);
        }

        private static long GetBaseBuildCostAmount(ResourceConsume consume)
        {
            var entries = consume?.Standard?.Entries;
            if (entries == null) return 0;

            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i] != null && entries[i].Amount.HasValue)
                    return entries[i].Amount.Value;
            }

            return 0;
        }

        private async void OnUpgradeClicked()
        {
            upgradeButton.interactable = false;
            await GameLoopService.BoardLoopBuild(SlotIndex);
        }
    }
}
