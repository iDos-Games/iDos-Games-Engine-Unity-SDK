using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IDosGames.UI;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IDosGames.UI.Quest
{
    public enum QuestMilestoneState { Locked, Reached, Claimed }

    public class QuestMilestoneItemView : MonoBehaviour
    {
        [Header("Progress")]
        [SerializeField] private TextMeshProUGUI _progressText;
        [SerializeField] private Slider          _progressSlider;

        [Header("Icon")]
        [SerializeField] private Image           _itemIcon;

        [Header("Rewards")]
        [SerializeField] private RectTransform   _rewardContainer;
        [SerializeField] private RewardItemView  _rewardRowTemplate;

        [Header("State")]
        [SerializeField] private GameObject      _iconCheck;
        [SerializeField] private GameObject      _dim;
        [SerializeField] private GameObject      _iconLock;
        [SerializeField] private Button          _claimButton;

        private Func<Task> _onClaim;
        private bool       _claiming;
        private readonly List<RewardItemView> _spawnedRewards = new();

        // ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (_rewardContainer == null) return;
            for (int i = _rewardContainer.childCount - 1; i >= 0; i--)
                Destroy(_rewardContainer.GetChild(i).gameObject);
        }

        // ─────────────────────────────────────────────────────────────

        public void Setup(
            string displayName,
            long requiredCount,
            long currentCount,
            long previousCount,
            List<ResourceEntry> rewards,
            QuestMilestoneState state,
            string iconPath,
            Func<Task> onClaim)
        {
            if (string.IsNullOrEmpty(displayName)) { gameObject.SetActive(false); return; }

            gameObject.SetActive(true);
            _claiming = false;
            _onClaim  = onClaim;

            bool canClaim = state == QuestMilestoneState.Reached;

            if (_progressText != null)
                _progressText.text = $"{Math.Min(currentCount, requiredCount)}/{requiredCount}";

            if (_progressSlider != null)
            {
                long  segment = requiredCount - previousCount;
                float t       = segment > 0 ? (float)(currentCount - previousCount) / segment : 1f;
                _progressSlider.value = Mathf.Clamp01(t);
            }

            if (_itemIcon != null && !string.IsNullOrEmpty(iconPath))
                LoadIconAsync(iconPath);

            PopulateRewards(rewards);

            if (_iconCheck != null) _iconCheck.SetActive(state == QuestMilestoneState.Claimed);
            if (_dim       != null) _dim.SetActive(state == QuestMilestoneState.Locked);
            if (_iconLock  != null) _iconLock.SetActive(state == QuestMilestoneState.Locked);

            if (_claimButton != null)
            {
                _claimButton.gameObject.SetActive(canClaim);
                _claimButton.interactable = canClaim;
                _claimButton.onClick.RemoveAllListeners();
                if (canClaim) _claimButton.onClick.AddListener(OnClaimClicked);
            }
        }

        // ─────────────────────────────────────────────────────────────

        private void PopulateRewards(List<ResourceEntry> rewards)
        {
            if (_rewardContainer == null || _rewardRowTemplate == null) return;

            int needed = rewards?.Count ?? 0;
            _rewardContainer.gameObject.SetActive(needed > 0);

            if (needed == 0) return;

            while (_spawnedRewards.Count < needed)
                _spawnedRewards.Add(Instantiate(_rewardRowTemplate, _rewardContainer));

            for (int i = 0; i < needed; i++)
            {
                _spawnedRewards[i].gameObject.SetActive(true);
                _spawnedRewards[i].Setup(rewards[i]);
            }

            for (int i = needed; i < _spawnedRewards.Count; i++)
                _spawnedRewards[i].gameObject.SetActive(false);

            LayoutRebuilder.ForceRebuildLayoutImmediate(_rewardContainer);
        }

        // ─────────────────────────────────────────────────────────────

        private async void OnClaimClicked()
        {
            if (_claiming || _onClaim == null) return;
            _claiming = true;
            if (_claimButton != null) _claimButton.interactable = false;

            try   { await _onClaim(); }
            catch (Exception ex) { Debug.LogError($"[QuestMilestone] Claim error: {ex.Message}"); }
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
                if (this != null && _itemIcon != null && sprite != null)
                    _itemIcon.sprite = sprite;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[QuestMilestoneItemView] Icon load failed: {ex.Message}");
            }
        }
    }
}
