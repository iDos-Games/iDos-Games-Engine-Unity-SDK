using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using IDosGames.TitlePublicConfiguration;

namespace IDosGames
{
    public class LootboxItemView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private Button _buyButton;
        [SerializeField] private TextMeshProUGUI _priceText;

        // Данные
        private LootboxDefinition _data;
        private Action<LootboxDefinition, int> _onPurchaseCallback;
        private int _selectedOptionId;

        public void Setup(LootboxDefinition data, Action<LootboxDefinition, int> onPurchase)
        {
            _data = data;
            _onPurchaseCallback = onPurchase;

            _titleText.text = data.LootboxID;

            // Логика загрузки иконки (пример)
            // if (!string.IsNullOrEmpty(data.LootboxImagePath))
            //      _iconImage.sprite = Resources.Load<Sprite>(data.LootboxImagePath); 

            SetupPrice();

            _buyButton.onClick.RemoveAllListeners();
            _buyButton.onClick.AddListener(OnBuyClicked);
        }

        private void SetupPrice()
        {
            if (_data.PriceOptions != null && _data.PriceOptions.Count > 0)
            {
                // Берем первую опцию цены для простоты
                var option = _data.PriceOptions[0];
                _selectedOptionId = option.OptionID;

                // Формируем строку цены
                string priceString = "";
                if (option.RequiredResources != null)
                {
                    foreach (var res in option.RequiredResources)
                    {
                        // Используем ItemID если это предмет, или CurrencyID если валюта
                        string name = !string.IsNullOrEmpty(res.ItemID) ? res.ItemID : res.CurrencyID;
                        priceString += $"{res.Amount} {name} ";
                    }
                }

                _priceText.text = priceString;
                _buyButton.interactable = true;
            }
            else
            {
                _priceText.text = "Error";
                _buyButton.interactable = false;
            }
        }

        private void OnBuyClicked()
        {
            _onPurchaseCallback?.Invoke(_data, _selectedOptionId);
        }
    }
}
