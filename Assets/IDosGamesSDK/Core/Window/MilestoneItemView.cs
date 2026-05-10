using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IDosGames.UI;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames.UI.LimitedTimeEvent
{
    public enum MilestoneItemState { Locked, Reached, Claimed }

    public class MilestoneItemView : MonoBehaviour
    {
        [SerializeField] private RectTransform   _rewardContainer;
        [SerializeField] private RewardItemView  _rewardRowTemplate;
        [SerializeField] private GameObject      _iconCheck;
        [SerializeField] private GameObject      _dim;
        [SerializeField] private GameObject      _iconLock;
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
            bool canClaim      = state == MilestoneItemState.Reached && !premiumLocked;

            PopulateRewards(def, isPremiumColumn);

            if (_iconCheck != null) _iconCheck.SetActive(state == MilestoneItemState.Claimed && !premiumLocked);
            if (_dim       != null) _dim.SetActive(state == MilestoneItemState.Locked || premiumLocked);
            if (_iconLock  != null) _iconLock.SetActive(premiumLocked);

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

            PopulateRewardsInternal(rewards, def.AssetPaths);
        }

        private void PopulateRewardsInternal(List<ResourceEntry> rewards, Dictionary<string, string> assetPaths)
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
                row.gameObject.SetActive(true);

                string id = reward.ItemID ?? reward.CurrencyID;
                string iconPath = null;
                if (!string.IsNullOrEmpty(id) && assetPaths != null && assetPaths.ContainsKey(id))
                {
                    iconPath = assetPaths[id];
                }
                else if (assetPaths != null && assetPaths.Count > 0)
                {
                    iconPath = assetPaths.Values.FirstOrDefault();
                }

                if (!string.IsNullOrEmpty(iconPath))
                {
                    LoadRewardIconAsync(row, iconPath);
                }

                if (row.AmountText != null)
                {
                    bool isCurrency = reward.Type == null || reward.Type == ResourceEntryType.VirtualCurrency;
                    string prefix = isCurrency ? "+" : "x";
                    long amountValue = reward.Amount ?? 0;
                    string amount = amountValue > 0 ? amountValue.ToString("N0") : string.Empty;

                    if (isCurrency)
                    {
                        row.AmountText.text = $"{prefix}{amount}";
                    }
                    else
                    {
                        string name = reward.ItemID ?? reward.CurrencyID ?? "Item";
                        row.AmountText.text = string.IsNullOrEmpty(amount) ? name : $"{name} {prefix}{amount}";
                    }
                }
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(_rewardContainer);
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        }

        private async void LoadRewardIconAsync(RewardItemView row, string path)
        {
            if (string.IsNullOrEmpty(path) || row == null) return;
            var sprite = await ImageLoader.GetSpriteAsync(path);
            if (this != null && row != null && row.Icon != null && sprite != null)
                row.Icon.sprite = sprite;
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
