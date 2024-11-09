using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace IDosGames
{
    [RequireComponent(typeof(Button))]
    public class SkinInventoryItem : Item
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _rarityBackground;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _amount;

        [SerializeField] private GameObject _amountObject;

        public virtual void Fill(Action action, SkinCatalogItem item)
        {
            ResetButton(action);

            if (IsExternalUrl(item.ImagePath))
            {
                LoadExternalImage(item.ImagePath);
            }
            else
            {
                _icon.sprite = Resources.Load<Sprite>(item.ImagePath);
            }

            _rarityBackground.color = Rarity.GetColor(item.Rarity);

            var amount = UserInventory.GetItemAmount(item.ItemID);
            UpdateAmount(amount);
        }

        private bool IsExternalUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
                   (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        private async void LoadExternalImage(string url)
        {
            var sprite = await ImageLoader.LoadImageAsync(url);
            if (sprite != null)
            {
                _icon.sprite = sprite;
            }
            else
            {
                Debug.LogError($"Failed to load external image from url: {url}");
            }
        }

        public void UpdateAmount(int amount)
        {
            _amount.text = amount.ToString();

            if (_amountObject != null)
            {
                _amountObject.SetActive(amount > 0);
            }
        }

        private void ResetButton(Action action)
        {
            if (action == null)
            {
                return;
            }

            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(new UnityAction(action));
        }
    }
}
