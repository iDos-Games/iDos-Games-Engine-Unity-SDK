using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	public class PanelMarketplaceCreateOffer : MonoBehaviour
	{
		[SerializeField] private TMP_Dropdown _currencyDropdown;
		[SerializeField] private TMP_InputField _priceInputField;
		[SerializeField] private MarketplaceCommissionPanel _commisionPanel;
		[SerializeField] private MarketplaceSelectSkinButton _marketplaceSelectSkinButton;
		[SerializeField] private Button _createButton;

		public VirtualCurrencyID SelectedCurrencyID { get; private set; }
		public int SelectedPrice { get; private set; }

		public static Action OfferCreated;

#if IDOSGAMES_MARKETPLACE
		private void Start()
		{
			_currencyDropdown.onValueChanged.AddListener(OnCurrencyChanged);
			_priceInputField.onValueChanged.AddListener(OnPriceChanged);
			_marketplaceSelectSkinButton.ValueChanged += UpdateCreateButtonInteractable;
			ResetCreateButton();
		}

		private void OnEnable()
		{
			ResetPanel();
		}

		private void OnCurrencyChanged(int value)
		{
			SelectedCurrencyID = value == 0 ? VirtualCurrencyID.IG : VirtualCurrencyID.CO;
			UpdateCreateButtonInteractable();
		}

		private void OnPriceChanged(string value)
		{
			int.TryParse(value, out int price);

			if (price < MarketplaceWindow.MIN_OFFER_PRICE)
			{
				_priceInputField.text = MarketplaceWindow.MIN_OFFER_PRICE.ToString();
				SelectedPrice = MarketplaceWindow.MIN_OFFER_PRICE;
			}
			else
			{
				SelectedPrice = price;
			}

			UpdateCreateButtonInteractable();
			_commisionPanel.UpdatePlayerGetText(SelectedPrice);
		}

		private void ResetPanel()
		{
			SelectedCurrencyID = VirtualCurrencyID.IG;
			SelectedPrice = MarketplaceWindow.MIN_OFFER_PRICE;

			_currencyDropdown.value = 0;
			_priceInputField.text = MarketplaceWindow.MIN_OFFER_PRICE.ToString();
			_marketplaceSelectSkinButton.ResetButton();
			UpdateCreateButtonInteractable();
			_commisionPanel.UpdatePlayerGetText(MarketplaceWindow.MIN_OFFER_PRICE);
		}

		private void UpdateCreateButtonInteractable()
		{
			_createButton.interactable = _marketplaceSelectSkinButton.SelectedSkin != null;
		}

		private void ResetCreateButton()
		{
			_createButton.onClick.RemoveAllListeners();
			_createButton.onClick.AddListener(CreateOffer);
		}

		private async Task<string> RequestCreateOffer()
		{
			Loading.ShowTransparentPanel();

			var request = new MarketplaceActionRequest
			{
				Action = MarketplaceAction.CreateOffer,
				ItemID = _marketplaceSelectSkinButton.SelectedSkin.ItemID,
				CurrencyID = SelectedCurrencyID,
				Price = SelectedPrice
			};

			var result = await IGSService.TryDoMarketplaceAction(request);

			Loading.HideAllPanels();

			return result;
		}

		private async void CreateOffer()
		{
			var result = await RequestCreateOffer();

			if (result == null)
			{
				Message.Show(MessageCode.FAILED_TO_LOAD_DATA);
				return;
			}

			var jObjectResult = JsonConvert.DeserializeObject<JObject>(result);

			if (jObjectResult.ContainsKey("Message"))
			{
				Message.Show(jObjectResult["Message"].ToString());
			}

			ResetPanel();
			UserDataService.RequestUserAllData();

			OfferCreated?.Invoke();
		}
#endif

    }
}
