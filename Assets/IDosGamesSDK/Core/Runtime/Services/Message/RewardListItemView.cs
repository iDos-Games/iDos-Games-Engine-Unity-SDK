using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
    public class RewardListItemView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _amountText;
        [SerializeField] private Sprite _defaultIcon;

        private int _iconLoadVersion;

        public void Set(ItemOrCurrency reward)
        {
            _iconLoadVersion++;

            if (reward == null)
            {
                _nameText.text = "Unknown";
                _amountText.text = string.Empty;
                _icon.sprite = _defaultIcon;
                return;
            }

            _nameText.text = GetRewardName(reward);
            _amountText.text = GetRewardAmount(reward);
            _icon.sprite = _defaultIcon;

            _ = SetIconAsync(reward.ImagePath, _iconLoadVersion);
        }

        private async Task SetIconAsync(string imagePath, int loadVersion)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                if (loadVersion == _iconLoadVersion)
                {
                    _icon.sprite = _defaultIcon;
                }

                return;
            }

            try
            {
                var sprite = await ImageLoader.GetSpriteAsync(imagePath);

                if (this == null || loadVersion != _iconLoadVersion)
                {
                    return;
                }

                _icon.sprite = sprite != null ? sprite : _defaultIcon;
            }
            catch
            {
                if (this == null || loadVersion != _iconLoadVersion)
                {
                    return;
                }

                _icon.sprite = _defaultIcon;
            }
        }

        private string GetRewardName(ItemOrCurrency reward)
        {
            if (!string.IsNullOrWhiteSpace(reward.Name))
                return reward.Name;

            if (!string.IsNullOrWhiteSpace(reward.ItemID))
                return reward.ItemID;

            if (!string.IsNullOrWhiteSpace(reward.CurrencyID))
                return reward.CurrencyID;

            return "Unknown";
        }

        private string GetRewardAmount(ItemOrCurrency reward)
        {
            if (!reward.Amount.HasValue)
                return string.Empty;

            return $"x{reward.Amount.Value}";
        }
    }
}
