using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using IDosGames.ClientModels;
using IDosGames.TitlePublicConfiguration;

namespace IDosGames.UI.Quest
{
    public class QuestItemView : MonoBehaviour
    {
        [Header("Content")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descText;
        [SerializeField] private Image           _iconImage;

        [Header("Objectives")]
        [SerializeField] private RectTransform      _objectiveContainer; // parent that holds spawned objective rows
        [SerializeField] private QuestObjectiveView _objectiveTemplate;  // inactive template

        [Header("Rewards")]
        [SerializeField] private RectTransform _rewardContainer;    // ItemFrame — holds reward rows
        [SerializeField] private RectTransform _rewardRowTemplate;  // RewardRow_1 — inactive template

        [Header("Status")]
        [SerializeField] private GameObject _completedBadge;
        [SerializeField] private GameObject _claimedBadge;
        [SerializeField] private GameObject _expiredBadge;

        [Header("Claim Button")]
        [SerializeField] private Button          _claimButton;
        [SerializeField] private TextMeshProUGUI _claimButtonText;

        // ─────────────────────────────────────────────────────────────

        public void Setup(
            string title,
            string description,
            string iconPath,
            QuestStatus status,
            bool canClaim,
            List<ObjectiveUIItem> objectives,
            List<ItemOrCurrency>  rewards,
            UnityAction onClaimClicked)
        {
            if (_titleText != null) _titleText.text = title;
            if (_descText  != null) _descText.text  = description;

            if (_iconImage != null && !string.IsNullOrEmpty(iconPath))
                LoadIconAsync(iconPath);

            if (_completedBadge != null) _completedBadge.SetActive(status == QuestStatus.Completed);
            if (_claimedBadge   != null) _claimedBadge.SetActive(status == QuestStatus.Claimed);
            if (_expiredBadge   != null) _expiredBadge.SetActive(status == QuestStatus.Expired);

            if (_claimButton != null)
            {
                _claimButton.interactable = canClaim;
                _claimButton.gameObject.SetActive(status == QuestStatus.Completed);

                _claimButton.onClick.RemoveAllListeners();
                if (canClaim && onClaimClicked != null)
                    _claimButton.onClick.AddListener(onClaimClicked);
            }

            PopulateObjectives(objectives);
            PopulateRewards(rewards);
            SetVisualState(status);
        }

        // ─────────────────────────────────────────────────────────────

        private void PopulateObjectives(List<ObjectiveUIItem> objectives)
        {
            if (_objectiveContainer == null || _objectiveTemplate == null) return;

            bool hasObjectives = objectives != null && objectives.Count > 0;
            _objectiveContainer.gameObject.SetActive(hasObjectives);

            // Destroy all non-template children
            for (int i = _objectiveContainer.childCount - 1; i >= 0; i--)
            {
                var child = _objectiveContainer.GetChild(i);
                if (child == _objectiveTemplate.transform) continue;
                DestroyImmediate(child.gameObject);
            }

            if (!hasObjectives) return;

            foreach (var obj in objectives)
            {
                var row = Instantiate(_objectiveTemplate, _objectiveContainer);
                row.gameObject.SetActive(true);
                row.Setup(obj);
            }

            // Rebuild layout
            LayoutRebuilder.ForceRebuildLayoutImmediate(_objectiveContainer);
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
            }

        private void PopulateRewards(List<ItemOrCurrency> rewards)
        {
            if (_rewardContainer == null || _rewardRowTemplate == null) return;

            bool hasRewards = rewards != null && rewards.Count > 0;
            _rewardContainer.gameObject.SetActive(hasRewards);

            // Destroy all non-template children
            for (int i = _rewardContainer.childCount - 1; i >= 0; i--)
            {
                var child = _rewardContainer.GetChild(i);
                if (child == _rewardRowTemplate) continue;
                DestroyImmediate(child.gameObject);
            }

            if (!hasRewards) return;
            
            var glg = _rewardContainer.GetComponent<GridLayoutGroup>();
            if (glg != null)
            {
                // We use Flexible constraint in the prefab now. 
                // But we can still nudge it for very large or very small counts.
                if (rewards.Count == 1)
                {
                    glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                    glg.constraintCount = 1;
                }
                else
                {
                    glg.constraint = GridLayoutGroup.Constraint.Flexible;
                }
            }

            foreach (var reward in rewards)
            {
                var row = Instantiate(_rewardRowTemplate, _rewardContainer);
                row.gameObject.SetActive(true);

                var icon = row.GetComponentInChildren<Image>(true);
                var text = row.GetComponentInChildren<TextMeshProUGUI>(true);

                if (text != null)
                {
                    bool isCurrency = reward.Type == null || reward.Type == ItemType.VirtualCurrency;
                    text.text = isCurrency
                        ? $"+{reward.Amount}"
                        : (reward.Name ?? reward.ItemID ?? $"+{reward.Amount}");
                }

                if (icon != null && !string.IsNullOrEmpty(reward.ImagePath))
                    LoadRewardIconAsync(reward.ImagePath, icon);
            }
            
            // Rebuild layout to ensure everything fits perfectly
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rewardContainer);
            // Also rebuild parent to account for reward container height changes
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
        }

        // ─────────────────────────────────────────────────────────────

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

        private async void LoadRewardIconAsync(string path, Image target)
        {
            try
            {
                var sprite = await ImageLoader.GetSpriteAsync(path);
                if (this != null && target != null && sprite != null)
                    target.sprite = sprite;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[QuestItemView] Reward icon load failed: {ex.Message}");
            }
        }

        private void SetVisualState(QuestStatus status)
        {
        }
    }
}
