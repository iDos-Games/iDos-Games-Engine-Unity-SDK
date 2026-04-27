using System.Linq;
using IDosGames.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames.UI.LimitedTimeEvent
{
    public enum MilestoneItemState { Locked, Reached, Claimed }

    public class MilestoneItemView : MonoBehaviour
    {
        // BubbleFrame05 children
        [SerializeField] private Image           _itemIcon;   // ItemIcon
        [SerializeField] private TextMeshProUGUI _numText;    // Text_Num
        [SerializeField] private GameObject      _iconCheck;  // Icon_Check  \u2014 claimed state
        [SerializeField] private GameObject      _dim;        // Dim         \u2014 locked state
        [SerializeField] private GameObject      _iconLock;   // Icon_Lock   \u2014 premium locked (Right column only)

        public void Setup(
            EventMilestoneDefinition def,
            MilestoneItemState       state,
            bool                     isPremiumColumn,
            bool                     hasPremiumPass)
        {
            if (def == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            bool premiumLocked = isPremiumColumn && !hasPremiumPass;

            // Reward icon (\u0441\u043e\u0431\u044b\u0442\u0438\u0439\u043d\u044b\u0435 asset paths)
            if (_itemIcon != null && def.AssetPaths?.Count > 0)
                LoadIconAsync(def.AssetPaths[0]);

            // Reward amount
            if (_numText != null)
            {
                var reward = def.Rewards?.FirstOrDefault();
                _numText.text = reward?.Amount != null
                    ? reward.Amount.Value.ToString("N0")
                    : string.Empty;
            }

            // Overlays
            if (_iconCheck != null) _iconCheck.SetActive(state == MilestoneItemState.Claimed);
            if (_dim       != null) _dim.SetActive(state == MilestoneItemState.Locked || premiumLocked);
            if (_iconLock  != null) _iconLock.SetActive(premiumLocked);
        }

        private async void LoadIconAsync(string path)
        {
            var sprite = await ImageLoader.GetSpriteAsync(path);
            if (this != null && _itemIcon != null && sprite != null)
                _itemIcon.sprite = sprite;
        }
    }
}
