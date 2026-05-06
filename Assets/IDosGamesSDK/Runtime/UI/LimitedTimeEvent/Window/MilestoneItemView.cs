using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IDosGames.ClientModels;
using IDosGames.TitlePublicConfiguration;
using IDosGames.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames.UI.LimitedTimeEvent
{
    public enum MilestoneItemState { Locked, Reached, Claimed }

    public class MilestoneItemView : MonoBehaviour
    {
        [SerializeField] private Image           _itemIcon;
        [SerializeField] private TextMeshProUGUI _numText;
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

            if (_itemIcon != null && def.AssetPaths?.Count > 0)
                LoadIconAsync(def.AssetPaths[0]);

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
            var rewards = new List<ItemOrCurrency>();
            if (!isPremiumColumn)
            {
                if (def.Rewards != null) rewards.AddRange(def.Rewards);
            }
            else
            {
                if (def.PremiumRewards != null)
                {
                    foreach (var pr in def.PremiumRewards)
                    {
                        if (pr.Rewards != null) rewards.AddRange(pr.Rewards);
                    }
                }
            }

            bool hasRewards = rewards.Count > 0;
            bool useContainer = hasRewards && _rewardContainer != null && _rewardRowTemplate != null;

            if (_numText != null)
            {
                if (hasRewards && !useContainer)
                {
                    _numText.text = rewards[0].Amount.GetValueOrDefault().ToString("N0");
                }
                else
                {
                    _numText.text = string.Empty;
                }
            }

            if (!useContainer) return;

            _rewardContainer.gameObject.SetActive(hasRewards);

            for (int i = _rewardContainer.childCount - 1; i >= 0; i--)
            {
                var child = _rewardContainer.GetChild(i);
                if (child == _rewardRowTemplate) continue;
                DestroyImmediate(child.gameObject);
            }

            if (!hasRewards) return;

            foreach (var reward in rewards)
            {
                var row = Instantiate(_rewardRowTemplate, _rewardContainer);
                row.gameObject.SetActive(true);

                if (row.AmountText != null)
                {
                    bool isCurrency = reward.Type == null || reward.Type == ItemType.VirtualCurrency;
                    string prefix = isCurrency ? "+" : "x";
                    long amountValue = reward.Amount.GetValueOrDefault();
                    string amount = amountValue > 0 ? amountValue.ToString("N0") : string.Empty;

                    if (isCurrency)
                    {
                        row.AmountText.text = $"{prefix}{amount}";
                    }
                    else
                    {
                        string name = reward.Name ?? reward.ItemID ?? "Item";
                        row.AmountText.text = string.IsNullOrEmpty(amount) ? name : $"{name} {prefix}{amount}";
                    }
                }

                if (row.Icon != null && !string.IsNullOrEmpty(reward.ImagePath))
                    LoadRewardIconAsync(reward.ImagePath, row.Icon);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(_rewardContainer);
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
                Debug.LogWarning($"[MilestoneItemView] Reward icon load failed: {ex.Message}");
            }
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

        private async void LoadIconAsync(string path)
        {
            var sprite = await ImageLoader.GetSpriteAsync(path);
            if (this != null && _itemIcon != null && sprite != null)
                _itemIcon.sprite = sprite;
        }
    }
}

