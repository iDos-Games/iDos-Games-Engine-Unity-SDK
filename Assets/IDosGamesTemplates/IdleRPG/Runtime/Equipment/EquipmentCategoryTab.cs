using System;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
    /// <summary>
    /// One tab in <see cref="EquipmentPanel"/>'s bottom category strip. Each tab is bound
    /// to a single <see cref="ItemDefinition.ItemClass"/> value via the inspector.
    /// </summary>
    public class EquipmentCategoryTab : MonoBehaviour
    {
        [Header("Identity")]
        [Tooltip("Value compared (case-insensitive) against ItemDefinition.ItemClass.")]
        [SerializeField] private string itemClass;

        [Header("Refs")]
        [SerializeField] private Button button;
        [SerializeField] private Image iconImage;
        [SerializeField] private GameObject selectedIndicator;

        private Action<string> _onSelected;

        public string ItemClass => itemClass;

        private void Awake()
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(InvokeClick);
            }
        }

        public void Bind(Sprite icon, bool selected, Action<string> onSelected)
        {
            _onSelected = onSelected;
            if (iconImage != null && icon != null) iconImage.sprite = icon;
            SetSelected(selected);
        }

        public void SetSelected(bool selected)
        {
            if (selectedIndicator != null) selectedIndicator.SetActive(selected);
        }

        private void InvokeClick() => _onSelected?.Invoke(itemClass);
    }
}
