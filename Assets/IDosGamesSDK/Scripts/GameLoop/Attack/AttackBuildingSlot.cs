using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using IDosGames.TitlePublicConfiguration;
using IDosGames.ClientModels;

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

            levelText.text = $"Lv.{state.Level}";
            damagedBadge.SetActive(state.IsDamaged);

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onSelected(_slotIndex));

            _ = LoadImage(def, state.Level);
        }

        public void SetInteractable(bool value) => button.interactable = value;

        private async System.Threading.Tasks.Task LoadImage(BuildingDefinition def, int level)
        {
            if (def?.ImageUrls == null || def.ImageUrls.Count == 0) return;

            int idx = Mathf.Clamp(Mathf.Max(0, level - 1), 0, def.ImageUrls.Count - 1);
            var sprite = await ImageLoader.GetSpriteAsync(def.ImageUrls[idx]);

            if (sprite != null && buildingImage != null)
                buildingImage.sprite = sprite;
        }
    }
}
