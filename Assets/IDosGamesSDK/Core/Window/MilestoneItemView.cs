using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IDosGames.UI;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames.UI.LimitedTimeEvent
{
    public enum MilestoneItemState { Locked, Reached, Pending, Claimed }

    public class MilestoneItemView : MonoBehaviour
    {
        [SerializeField] private RectTransform   _rewardContainer;
        [SerializeField] private RewardItemView  _rewardRowTemplate;
        [SerializeField] private GameObject      _iconCheck;
        [SerializeField] private GameObject      _dim;
        [SerializeField] private GameObject      _iconLock;
        [SerializeField] private GameObject      _pendingIcon;
        [SerializeField] private Button          _claimButton;

        private Func<Task> _onClaim;
        private bool       _claiming;

        public void Setup(
            EventMilestoneDefinition def,
            MilestoneItemState       state,
            bool                     isPremiumColumn,
            bool                     hasPremiumPass,
            Func<Task>               onClaim)
        {
            if (def == null) { gameObject.SetActive(false); return; }

            gameObject.SetActive(true);

            _claiming = false;
            _onClaim  = onClaim;

            bool premiumLocked = isPremiumColumn && !hasPremiumPass;
            bool isPending     = state == MilestoneItemState.Pending;
            bool canClaim      = state == MilestoneItemState.Reached && !premiumLocked;

            PopulateRewards(def, isPremiumColumn);

            if (_iconCheck   != null) _iconCheck.SetActive(state == MilestoneItemState.Claimed && !premiumLocked);
            if (_dim         != null) _dim.SetActive(state == MilestoneItemState.Locked || premiumLocked);
            if (_iconLock    != null) _iconLock.SetActive(premiumLocked);
            if (_pendingIcon != null) _pendingIcon.SetActive(isPending && !premiumLocked);

            if (_claimButton != null)
            {
                _claimButton.gameObject.SetActive(canClaim);
                _claimButton.interactable = canClaim;
                _claimButton.onClick.RemoveAllListeners();
                if (canClaim) _claimButton.onClick.AddListener(OnClaimClicked);
            }
        }

        private void PopulateRewards(EventMilestoneDefinition def, bool isPremiumColumn)
        {
            var rewards = new List<ResourceEntry>();
            if (!isPremiumColumn)
            {
                var entries = def.Rewards?.Standard?.Entries;
                if (entries != null) rewards.AddRange(entries);
            }
            else
            {
                var premiumTiers = def.Rewards?.PremiumTiers;
                if (premiumTiers != null)
                {
                    foreach (var tier in premiumTiers)
                    {
                        var entries = tier.Resources?.Entries;
                        if (entries != null) rewards.AddRange(entries);
                    }
                }
            }

            PopulateRewardsInternal(rewards);
        }

        private void PopulateRewardsInternal(List<ResourceEntry> rewards)
        {
            bool hasRewards   = rewards != null && rewards.Count > 0;
            bool useContainer = hasRewards && _rewardContainer != null && _rewardRowTemplate != null;

            if (!useContainer) return;

            _rewardContainer.gameObject.SetActive(true);

            for (int i = _rewardContainer.childCount - 1; i >= 0; i--)
            {
                var child = _rewardContainer.GetChild(i);
                if (child.gameObject == _rewardRowTemplate.gameObject) continue;
                DestroyImmediate(child.gameObject);
            }

            foreach (var reward in rewards)
            {
                var row = Instantiate(_rewardRowTemplate, _rewardContainer);
                row.Setup(reward);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        }

        private async void OnClaimClicked()
        {
            if (_claiming || _onClaim == null) return;
            _claiming = true;
            if (_claimButton != null) _claimButton.interactable = false;

            Loading.ShowTransparentPanel();
            try
            {
                await _onClaim();
            }
            finally
            {
                Loading.HideAllPanels();
                if (this != null && _claimButton != null)
                    _claimButton.interactable = true;
                _claiming = false;
            }
        }

    }
}
