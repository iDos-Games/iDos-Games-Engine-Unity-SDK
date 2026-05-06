using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IDosGames.ClientModels;
using IDosGames.TitlePublicConfiguration;
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

        // ─────────────────────────────────────────────────────────────

        public void Setup(
            string displayName,
            long requiredCount,
            long currentCount,
            long previousCount,
            List<ItemOrCurrency> rewards,
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
                _progressText.text = $"{currentCount}/{requiredCount}";

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

        private void PopulateRewards(List<ItemOrCurrency> rewards)
        {
            if (_rewardContainer == null || _rewardRowTemplate == null) return;

            for (int i = _rewardContainer.childCount - 1; i >= 0; i--)
            {
                var child = _rewardContainer.GetChild(i);
                if (child == _rewardRowTemplate.transform) continue;
                DestroyImmediate(child.gameObject);
            }

            bool hasRewards = rewards != null && rewards.Count > 0;
            _rewardContainer.gameObject.SetActive(hasRewards);
            if (!hasRewards) return;

            foreach (var reward in rewards)
            {
                var row = Instantiate(_rewardRowTemplate, _rewardContainer);
                row.Setup(BuildRewardLabel(reward));

                if (row.Icon != null && !string.IsNullOrEmpty(reward.ImagePath))
                    LoadRewardIconAsync(reward.ImagePath, row.Icon);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(_rewardContainer);
        }

        private static string BuildRewardLabel(ItemOrCurrency reward)
        {
            long amount     = reward.Amount ?? 0;
            bool isCurrency = reward.Type == null || reward.Type == ItemType.VirtualCurrency;

            if (isCurrency)
                return amount > 0 ? $"+{amount}" : string.Empty;

            string name = reward.Name ?? reward.ItemID ?? "Item";
            return amount > 1 ? $"{name} x{amount}" : name;
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

        private async void LoadRewardIconAsync(string path, Image target)
        {
            try
            {
                var sprite = await ImageLoader.GetSpriteAsync(path);
                if (this != null && target != null && sprite != null)
                    target.sprite = sprite;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[QuestMilestoneItemView] Reward icon load failed: {ex.Message}");
            }
        }
    }
}
