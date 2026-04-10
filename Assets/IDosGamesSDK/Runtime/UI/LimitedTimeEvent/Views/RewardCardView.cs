using System;
using IDosGames.ClientModels;
using IDosGames.TitlePublicConfiguration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames.UI.LimitedTimeEvent
{
    /// <summary>
    /// Single reward card shown in FREE or PASS column.
    /// States: Locked | Claimable | Claimed
    /// </summary>
    public class RewardCardView : MonoBehaviour
    {
        [Header("Card Visual")]
        [SerializeField] private Image           _backgroundImage;
        [SerializeField] private Image           _rewardIconImage;
        [SerializeField] private TextMeshProUGUI _rewardAmountText;
        [SerializeField] private Sprite          _fallbackSprite;

        [Header("State Overlays")]
        [SerializeField] private GameObject      _claimedOverlay;     // Green checkmark
        [SerializeField] private GameObject      _lockedOverlay;      // Lock icon
        [SerializeField] private GameObject      _vipLockedOverlay;   // Lock + VIP badge

        [Header("Claim Button")]
        [SerializeField] private Button          _claimButton;
        [SerializeField] private TextMeshProUGUI _claimButtonText;

        [Header("Card Colors")]
        [SerializeField] private Color _colorDefault   = new Color(0.38f, 0.31f, 0.55f);
        [SerializeField] private Color _colorClaimed   = new Color(0.28f, 0.21f, 0.45f);
        [SerializeField] private Color _colorCanClaim  = new Color(0.25f, 0.65f, 0.35f);
        [SerializeField] private Color _colorLocked    = new Color(0.22f, 0.18f, 0.32f);
        [SerializeField] private Color _colorVipActive = new Color(0.7f,  0.5f,  0.05f);

        private bool _isActive;
        private Action _onClaim;

        // ─── Unity ────────────────────────────────────────────────────────────

        private void OnEnable()  => _isActive = true;
        private void OnDisable() => _isActive = false;

        // ─── Public Setup ─────────────────────────────────────────────────────

        /// <summary>Standard reward card (free or pass column with reward data).</summary>
        public void Setup(
            ItemOrCurrency reward,
            bool isClaimed,
            bool canClaim,
            bool isLocked,
            Action onClaim)
        {
            _onClaim = onClaim;

            // Amount text
            string amountStr = "";
            if (reward != null)
            {
                amountStr = reward.Amount > 0 ? FormatAmount((long)reward.Amount) : "";
            }
            _rewardAmountText.text = amountStr;

            // Icon
            _rewardIconImage.sprite = _fallbackSprite;
            if (reward != null && !string.IsNullOrEmpty(reward.ImagePath))
                _ = LoadRewardIcon(reward.ImagePath);

            // States
            _claimedOverlay.SetActive(isClaimed);
            _lockedOverlay.SetActive(isLocked && !isClaimed);
            _vipLockedOverlay.SetActive(false);

            // Claim button
            if (_claimButton != null)
            {
                bool showClaim = canClaim && !isClaimed && !isLocked;
                _claimButton.gameObject.SetActive(showClaim);
                if (showClaim)
                {
                    _claimButton.onClick.RemoveAllListeners();
                    _claimButton.onClick.AddListener(OnClaimButtonClicked);
                    if (_claimButtonText != null) _claimButtonText.text = "Claim";
                }
            }

            // Background color
            if (_backgroundImage != null)
            {
                if (isClaimed)       _backgroundImage.color = _colorClaimed;
                else if (canClaim)   _backgroundImage.color = _colorCanClaim;
                else if (isLocked)   _backgroundImage.color = _colorLocked;
                else                 _backgroundImage.color = _colorDefault;
            }
        }

        /// <summary>VIP locked card — shows lock + VIP indicator, no reward data.</summary>
        public void SetupVipLocked()
        {
            _rewardAmountText.text = "";
            _rewardIconImage.sprite = _fallbackSprite;

            _claimedOverlay.SetActive(false);
            _lockedOverlay.SetActive(false);
            _vipLockedOverlay.SetActive(true);

            if (_claimButton != null) _claimButton.gameObject.SetActive(false);
            if (_backgroundImage != null) _backgroundImage.color = _colorLocked;
        }

        // ─── Private ──────────────────────────────────────────────────────────

        private async System.Threading.Tasks.Task LoadRewardIcon(string path)
        {
            var sprite = await ImageLoader.GetSpriteAsync(path);
            if (_isActive && sprite != null)
                _rewardIconImage.sprite = sprite;
        }

        private void OnClaimButtonClicked()
        {
            if (_claimButton != null) _claimButton.interactable = false;
            _onClaim?.Invoke();
        }

        private static string FormatAmount(long amount)
        {
            if (amount >= 1_000_000) return $"{amount / 1_000_000f:F1}M";
            if (amount >= 1_000)     return $"{amount / 1_000f:F1}K";
            return amount.ToString("N0");
        }
    }
}
