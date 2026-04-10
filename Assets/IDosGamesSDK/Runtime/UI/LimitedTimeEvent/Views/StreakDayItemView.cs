using System;
using IDosGames.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames.UI.LimitedTimeEvent
{
    /// <summary>Single streak day item in horizontal scroll.</summary>
    public class StreakDayItemView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _dayLabel;
        [SerializeField] private Image           _rewardIcon;
        [SerializeField] private TextMeshProUGUI _rewardAmountText;
        [SerializeField] private GameObject      _claimedMark;
        [SerializeField] private Button          _claimButton;
        [SerializeField] private GameObject      _lockedOverlay;
        [SerializeField] private Sprite          _fallbackSprite;
        [SerializeField] private Image           _background;
        [SerializeField] private Color           _colorActive  = new Color(1f, 0.75f, 0f);
        [SerializeField] private Color           _colorDefault = new Color(0.35f, 0.28f, 0.50f);
        [SerializeField] private Color           _colorClaimed = new Color(0.22f, 0.55f, 0.28f);

        private bool _isActive;

        private void OnEnable()  => _isActive = true;
        private void OnDisable() => _isActive = false;

        public void Setup(
            EventStreakReward streakReward,
            bool isClaimed,
            bool canClaim,
            Action onClaim)
        {
            _dayLabel.text = $"Day {streakReward.RequiredStreakDays}";

            // Reward display
            var rewards = streakReward.Rewards;
            if (rewards != null && rewards.Count > 0)
            {
                var r = rewards[0];
                _rewardAmountText.text = r.Amount > 0 ? r.Amount.ToString() : "";
                _rewardIcon.sprite = _fallbackSprite;
                if (!string.IsNullOrEmpty(r.ImagePath))
                    _ = LoadIcon(r.ImagePath);
            }
            else
            {
                _rewardAmountText.text = $"+{streakReward.BonusTokens}";
            }

            _claimedMark.SetActive(isClaimed);
            _lockedOverlay.SetActive(!canClaim && !isClaimed);

            if (_claimButton != null)
            {
                _claimButton.gameObject.SetActive(canClaim && !isClaimed);
                _claimButton.onClick.RemoveAllListeners();
                if (canClaim && !isClaimed)
                    _claimButton.onClick.AddListener(() => onClaim?.Invoke());
            }

            if (_background != null)
            {
                _background.color = isClaimed ? _colorClaimed
                                  : canClaim  ? _colorActive
                                  : _colorDefault;
            }
        }

        private async System.Threading.Tasks.Task LoadIcon(string path)
        {
            var sprite = await ImageLoader.GetSpriteAsync(path);
            if (_isActive && sprite != null) _rewardIcon.sprite = sprite;
        }
    }
}
