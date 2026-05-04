using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames.UI.Quest
{
    public class QuestTabView : MonoBehaviour
    {
        [Header("Visual")]
        [SerializeField] private TextMeshProUGUI _labelText;
        [SerializeField] private Image           _backgroundImage;
        [SerializeField] private GameObject      _completedIndicator;

        private static readonly Color ActiveColor   = new Color(0x31 / 255f, 0x81 / 255f, 1f);
        private static readonly Color InactiveColor = new Color(0x36 / 255f, 0x36 / 255f, 0x4E / 255f);

        /// <summary>CycleID цикла или "" для перманентных квестов.</summary>
        public string TabId { get; private set; }

        public void Setup(string tabId, string label, Action onClick)
        {
            TabId = tabId;

            if (_labelText != null)
                _labelText.text = label;

            var btn = GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => onClick?.Invoke());
            }

            SetSelected(false);
        }

        public void SetSelected(bool selected)
        {
            if (_backgroundImage != null) _backgroundImage.color = selected ? ActiveColor : InactiveColor;
        }

        public void SetHasCompleted(bool hasCompleted)
        {
            if (_completedIndicator != null) _completedIndicator.SetActive(hasCompleted);
        }
    }
}
