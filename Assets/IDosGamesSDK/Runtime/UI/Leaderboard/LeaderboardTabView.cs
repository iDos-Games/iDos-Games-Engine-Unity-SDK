using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames.UI.Leaderboard
{
    public class LeaderboardTabView : MonoBehaviour
    {
        [Header("Visual")]
        [SerializeField] private TextMeshProUGUI _labelText;
        [SerializeField] private Image           _backgroundImage;
        [SerializeField] private GameObject      _notificationDot;
        [SerializeField] private GameObject      _topAccent;

        private static readonly Color ActiveBg     = new Color(0x00 / 255f, 0xA8 / 255f, 0xFF / 255f, 1f);
        private static readonly Color InactiveBg   = new Color(0x18 / 255f, 0x1E / 255f, 0x3A / 255f, 1f);
        private static readonly Color TextActive   = new Color(1f, 1f, 1f, 1f);
        private static readonly Color TextInactive = new Color(1f, 1f, 1f, 0.50f);

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
            SetHasNotification(false);
        }

        public void SetSelected(bool selected)
        {
            if (_backgroundImage != null) _backgroundImage.color = selected ? ActiveBg : InactiveBg;
            if (_topAccent       != null) _topAccent.SetActive(selected);
            if (_labelText       != null)
            {
                _labelText.color     = selected ? TextActive : TextInactive;
                _labelText.fontStyle = selected ? FontStyles.Bold : FontStyles.Normal;
            }
        }

        // Show orange dot when the tab has unclaimed rewards or milestones.
        public void SetHasNotification(bool hasNotification)
        {
            if (_notificationDot != null) _notificationDot.SetActive(hasNotification);
        }
    }
}
