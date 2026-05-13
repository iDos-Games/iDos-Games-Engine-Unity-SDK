using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames.UI.Leaderboard
{
    public enum LeaderboardMilestoneState { Locked, Reached, Claimed, Pending }

    public class LeaderboardMilestoneView : MonoBehaviour
    {
        [Header("Progress")]
        [SerializeField] private TextMeshProUGUI _progressText;
        [SerializeField] private Slider          _progressSlider;

        [Header("Info")]
        [SerializeField] private TextMeshProUGUI _displayNameText;
        [SerializeField] private Image           _iconImage;

        [Header("Rewards")]
        [SerializeField] private RectTransform   _rewardContainer;
        [SerializeField] private RewardItemView _rewardTemplate;

        [Header("State")]
        [SerializeField] private GameObject _checkIcon;
        [SerializeField] private GameObject _lockIcon;
        [SerializeField] private GameObject _pendingIcon;
        [SerializeField] private GameObject _featuredBadge;
        [SerializeField] private GameObject _dim;
        [SerializeField] private Button     _claimButton;
        [SerializeField] private GameObject _unclaimedRewardBadge;

        private Func<Task> _onClaim;
        private bool       _claiming;

        // ─────────────────────────────────────────────────────────────────

        public void Setup(
            string                  displayName,
            long                    requiredScore,
            long                    currentScore,
            long                    previousScore,
            List<ResourceEntry>     rewards,
            LeaderboardMilestoneState state,
            string                  iconPath,
            bool                    isFeatured,
            Func<Task>              onClaim)
        {
            if (string.IsNullOrEmpty(displayName)) { gameObject.SetActive(false); return; }

            gameObject.SetActive(true);
            _claiming = false;
            _onClaim  = onClaim;

            bool canClaim = state == LeaderboardMilestoneState.Reached;

            if (_displayNameText != null)
                _displayNameText.text = displayName;

            if (_progressText != null)
                _progressText.text = $"{FormatScore(Math.Min(currentScore, requiredScore))}/{FormatScore(requiredScore)}";

            if (_progressSlider != null)
            {
                long  segment = requiredScore - previousScore;
                float t       = segment > 0 ? (float)(currentScore - previousScore) / segment : 1f;
                _progressSlider.value = Mathf.Clamp01(t);
            }

            if (_iconImage != null && !string.IsNullOrEmpty(iconPath))
                LoadIconAsync(iconPath);

            PopulateRewards(rewards);

            if (_checkIcon   != null) _checkIcon.SetActive(state == LeaderboardMilestoneState.Claimed);
            if (_lockIcon    != null) _lockIcon.SetActive(state  == LeaderboardMilestoneState.Locked);
            if (_pendingIcon != null) _pendingIcon.SetActive(state == LeaderboardMilestoneState.Pending);
            if (_featuredBadge != null) _featuredBadge.SetActive(isFeatured);

            if (_dim != null)
                _dim.SetActive(state == LeaderboardMilestoneState.Locked);

            if (_unclaimedRewardBadge != null)
                _unclaimedRewardBadge.SetActive(canClaim);

            if (_claimButton != null)
            {
                _claimButton.gameObject.SetActive(canClaim);
                _claimButton.interactable = canClaim;
                _claimButton.onClick.RemoveAllListeners();
                if (canClaim) _claimButton.onClick.AddListener(OnClaimClicked);
            }
        }

        // ─────────────────────────────────────────────────────────────────

        private void PopulateRewards(List<ResourceEntry> rewards)
        {
            if (_rewardContainer == null || _rewardTemplate == null) return;

            for (int i = _rewardContainer.childCount - 1; i >= 0; i--)
            {
                var child = _rewardContainer.GetChild(i);
                if (child == _rewardTemplate.transform) continue;
                Destroy(child.gameObject);
            }

            bool hasRewards = rewards != null && rewards.Count > 0;
            _rewardContainer.gameObject.SetActive(hasRewards);
            if (!hasRewards) return;

            foreach (var reward in rewards)
            {
                var row = Instantiate(_rewardTemplate, _rewardContainer);
                row.Setup(reward);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(_rewardContainer);
        }

        private static string FormatScore(long score)
        {
            if (score >= 1_000_000) return $"{score / 1_000_000f:0.#}M";
            if (score >= 1_000)     return $"{score / 1_000f:0.#}K";
            return score.ToString();
        }

        // ─────────────────────────────────────────────────────────────────

        private async void OnClaimClicked()
        {
            if (_claiming || _onClaim == null) return;
            _claiming = true;
            if (_claimButton != null) _claimButton.interactable = false;

            try   { await _onClaim(); }
            catch (Exception ex) { Debug.LogError($"[LeaderboardMilestone] Claim error: {ex.Message}"); }
            finally
            {
                if (this != null && _claimButton != null) _claimButton.interactable = true;
                _claiming = false;
            }
        }

        private async void LoadIconAsync(string path)
        {
            try
            {
                var sprite = await ImageLoader.GetSpriteAsync(path);
                if (this != null && _iconImage != null && sprite != null)
                    _iconImage.sprite = sprite;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[LeaderboardMilestoneView] Icon load failed: {ex.Message}");
            }
        }
    }
}
