using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames.UI.Quest
{
    public enum QuestMilestoneState { Locked, Reached, Claimed }

    public class QuestMilestoneItemView : MonoBehaviour
    {
        [SerializeField] private Image           _itemIcon;
        [SerializeField] private TextMeshProUGUI _progressText;
        [SerializeField] private TextMeshProUGUI _rewardText;
        [SerializeField] private GameObject      _iconCheck;
        [SerializeField] private GameObject      _dim;
        [SerializeField] private GameObject      _iconLock;
        [SerializeField] private Button          _claimButton;

        private Func<Task> _onClaim;
        private bool       _claiming;

        public void Setup(
            string displayName,
            long requiredCount,
            long currentCount,
            long rewardAmount,
            QuestMilestoneState state,
            string iconPath,
            Func<Task> onClaim)
        {
            if (string.IsNullOrEmpty(displayName)) { gameObject.SetActive(false); return; }

            gameObject.SetActive(true);
            _claiming = false;
            _onClaim = onClaim;

            bool canClaim = state == QuestMilestoneState.Reached;

            if (_progressText != null) _progressText.text = $"{currentCount}/{requiredCount}";
            if (_rewardText != null) _rewardText.text = rewardAmount > 0 ? $"+{rewardAmount}" : string.Empty;

            if (_itemIcon != null && !string.IsNullOrEmpty(iconPath))
                LoadIconAsync(iconPath);

            if (_iconCheck != null) _iconCheck.SetActive(state == QuestMilestoneState.Claimed);
            if (_dim != null)       _dim.SetActive(state == QuestMilestoneState.Locked);
            if (_iconLock != null)  _iconLock.SetActive(state == QuestMilestoneState.Locked);

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

            try { await _onClaim(); }
            catch (Exception ex) { Debug.LogError($"[QuestMilestone] Claim error: {ex.Message}"); }
            finally
            {
                if (this != null && _claimButton != null) _claimButton.interactable = true;
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