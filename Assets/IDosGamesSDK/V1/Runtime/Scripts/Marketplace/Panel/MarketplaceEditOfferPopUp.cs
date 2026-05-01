using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	public class MarketplaceEditOfferPopUp : MonoBehaviour
	{
		[SerializeField] private Button _saveButton;
		[SerializeField] private TMP_Dropdown _currencyDropdown;
		[SerializeField] private TMP_InputField _priceInputField;

		private VirtualCurrencyID _previousCurrencyID;
		private int _previousPrice;

		private VirtualCurrencyID _newCurrencyID;
		private int _newPrice;

#if IDOSGAMES_MARKETPLACE
		private void Start()
		{
			_currencyDropdown.onValueChanged.AddListener(OnCurrencyChanged);
			_priceInputField.onValueChanged.AddListener(OnPriceChanged);
		}

		public void Show(Action<VirtualCurrencyID, int> saveAction, VirtualCurrencyID currencyID, int price)
		{
			ResetSaveButton(saveAction);

			_previousCurrencyID = currencyID;
			_newCurrencyID = currencyID;
			_previousPrice = price;
			_newPrice = price;

			UpdateSaveButtonInteractable();

			_currencyDropdown.value = currencyID == VirtualCurrencyID.IG ? 0 : 1;
			_priceInputField.text = price.ToString();

			gameObject.SetActive(true);
		}

		private void OnCurrencyChanged(int value)
		{
			_newCurrencyID = value == 0 ? VirtualCurrencyID.IG : VirtualCurrencyID.CO;
			UpdateSaveButtonInteractable();
		}

		private void OnPriceChanged(string value)
		{
			int.TryParse(value, out int price);

			if (price < MarketplaceWindow.MIN_OFFER_PRICE)
			{
				_priceInputField.text = MarketplaceWindow.MIN_OFFER_PRICE.ToString();
				_newPrice = MarketplaceWindow.MIN_OFFER_PRICE;
			}
			else
			{
				_newPrice = price;
			}

			UpdateSaveButtonInteractable();
		}

		private void UpdateSaveButtonInteractable()
		{
			_saveButton.interactable = _previousCurrencyID != _newCurrencyID || _previousPrice != _newPrice;
		}

		private void ResetSaveButton(Action<VirtualCurrencyID, int> saveAction)
		{
			_saveButton.onClick.RemoveAllListeners();
			_saveButton.onClick.AddListener(() => saveAction?.Invoke(_newCurrencyID, _newPrice));
			_saveButton.onClick.AddListener(() => gameObject.SetActive(false));
		}
#endif
    }
}
