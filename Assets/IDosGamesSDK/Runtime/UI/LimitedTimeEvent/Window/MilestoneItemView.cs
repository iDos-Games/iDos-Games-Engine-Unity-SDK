using System;
using System.Linq;
using System.Threading.Tasks;
using IDosGames.ClientModels;
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

            if (_numText != null)
            {
                var reward = def.Rewards?.FirstOrDefault();
                _numText.text = reward?.Amount != null
                    ? reward.Amount.Value.ToString("N0")
                    : string.Empty;
            }

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
