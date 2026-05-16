using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
    public class AttackBuildingSlot : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image buildingImage;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private GameObject damagedBadge;

        private int _slotIndex;

        public void Bind(int slotIndex, BuildingDefinition def, BuildingState state, Action<int> onSelected)
        {
            _slotIndex = slotIndex;

            button.interactable = true;

            levelText.text = $"Lv.{state.Level}";
            damagedBadge.SetActive(state.IsDamaged);

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onSelected(_slotIndex));

            _ = LoadImage(def, state.Level);
        }

        public void SetInteractable(bool value) => button.interactable = value;

        private async System.Threading.Tasks.Task LoadImage(BuildingDefinition def, int level)
        {
            string path = ResolveBuildingAssetPath(def?.AssetPaths, level);
            if (string.IsNullOrEmpty(path)) return;

            var sprite = await ImageLoader.GetSpriteAsync(path);

            if (sprite != null && buildingImage != null)
                buildingImage.sprite = sprite;
        }

        private static string ResolveBuildingAssetPath(Dictionary<string, string> assetPaths, int level)
        {
            if (assetPaths == null || assetPaths.Count == 0)
                return null;

            if (assetPaths.TryGetValue($"level_{level}", out var perLevel) && !string.IsNullOrWhiteSpace(perLevel))
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
    }
}
