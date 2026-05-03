using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using IDosGames.ClientModels;

namespace IDosGames.UI.Quest
{
    public class QuestItemView : MonoBehaviour
    {
        [Header("Content")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descText;
        [SerializeField] private Image _iconImage;

        [Header("Progress")]
        [SerializeField] private Slider _progressSlider;
        [SerializeField] private TextMeshProUGUI _progressText;

        [Header("Reward")]
        [SerializeField] private TextMeshProUGUI _rewardText;
        [SerializeField] private Image _rewardIcon;

        [Header("Status")]
        [SerializeField] private GameObject _completedBadge;
        [SerializeField] private GameObject _claimedBadge;
        [SerializeField] private GameObject _expiredBadge;

        [Header("Claim Button")]
        [SerializeField] private Button _claimButton;
        [SerializeField] private TextMeshProUGUI _claimButtonText;

        public void Setup(
            string title,
            string description,
            string iconPath,
            long current,
            long target,
            QuestStatus status,
            bool canClaim,
            long rewardAmount,
            UnityAction onClaimClicked)
        {
            if (_titleText != null) _titleText.text = title;
            if (_descText != null) _descText.text = description;
            if (_progressText != null) _progressText.text = $"{current}/{target}";
            if (_rewardText != null) _rewardText.text = $"+{rewardAmount}";

            if (_progressSlider != null && target > 0)
                _progressSlider.value = Mathf.Clamp01((float)current / target);

            if (_iconImage != null && !string.IsNullOrEmpty(iconPath))
                LoadIconAsync(iconPath);

            if (_completedBadge != null) _completedBadge.SetActive(status == QuestStatus.Completed);
            if (_claimedBadge != null) _claimedBadge.SetActive(status == QuestStatus.Claimed);
            if (_expiredBadge != null) _expiredBadge.SetActive(status == QuestStatus.Expired);

            if (_claimButton != null)
            {
                _claimButton.interactable = canClaim;
                _claimButton.gameObject.SetActive(status == QuestStatus.Completed);
                
                _claimButton.onClick.RemoveAllListeners();
                if (canClaim && onClaimClicked != null)
                    _claimButton.onClick.AddListener(onClaimClicked);
            }

            SetVisualState(status);
        }

        private async void LoadIconAsync(string path)
        {
            try
            {
                var sprite = await ImageLoader.GetSpriteAsync(path);
                if (this != null && _iconImage != null && sprite != null)
                    _iconImage.sprite = sprite;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[QuestItemView] Icon load failed: {ex.Message}");
            }
        }

        private void SetVisualState(QuestStatus status)
        {
        }
    }
}