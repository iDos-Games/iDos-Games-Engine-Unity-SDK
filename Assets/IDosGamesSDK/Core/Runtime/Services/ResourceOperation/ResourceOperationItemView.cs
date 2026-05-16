using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
    public class ResourceOperationItemView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _amountText;
        [SerializeField] private Sprite _defaultIcon;

        private int _iconLoadVersion;

        public void Set(ResourceEntry entry)
        {
            _iconLoadVersion++;

            if (entry == null)
            {
                _nameText.text = "Unknown";
                _amountText.text = string.Empty;
                _icon.sprite = _defaultIcon;
                return;
            }

            var (assetPath, displayName) = ResourceEntryAssetResolver.Resolve(entry);

            _nameText.text = displayName;
            _amountText.text = entry.Amount.HasValue ? $"x{entry.Amount.Value}" : string.Empty;
            _icon.sprite = _defaultIcon;

            _ = SetIconAsync(assetPath, _iconLoadVersion);
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
    }
}
