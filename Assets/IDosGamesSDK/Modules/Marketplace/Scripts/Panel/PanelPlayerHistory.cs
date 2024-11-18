using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

#pragma warning disable 0414
namespace IDosGames
{
	public class PanelPlayerHistory : MonoBehaviour
	{
		[Range(1, 100)][SerializeField] private int _itemsInOnePage = 20;
		public int ItemsInOnePage => _itemsInOnePage;

		[SerializeField] private MarketplacePlayerHistoryItem _offerItem;
		[SerializeField] private Transform _parent;
		[SerializeField] private LoadMoreButton _loadMoreButtonPrefab;
		[SerializeField] private TMP_Text _voidText;
		private LoadMoreButton _loadMoreButton;

		public bool IsNeedUpdate { get; set; } = true;

		private string _continuationToken;
		private bool _destoyChildrenOnInstantiate = true;

#if IDOSGAMES_MARKETPLACE
		private void OnEnable()
		{
			if (IsNeedUpdate)
			{
                RefreshData();
			}
		}

        public void RefreshData()
        {
            if (string.IsNullOrEmpty(IGSUserData.MarketplaceHistory))
            {
                Refresh();
            }
            else
            {
                string history = IGSUserData.MarketplaceHistory;
                _continuationToken = null;
                _destoyChildrenOnInstantiate = true;
                ProcessRequestDataResult(history);
                IsNeedUpdate = false;
            }
        }

        public async void Refresh()
		{
			_continuationToken = null;
			_destoyChildrenOnInstantiate = true;
			var result = await RequestData();
			ProcessRequestDataResult(result);
			IsNeedUpdate = false;
		}

		private async Task<string> RequestData()
		{
			Loading.ShowTransparentPanel();

			var requestBody = new MarketplaceGetDataRequest
			{
				Panel = MarketplacePanel.PlayerHistory,
				ItemsInOnePage = _itemsInOnePage,
				ContinuationToken = _continuationToken?.ToString()
			};

			var result = await IGSService.GetDataFromMarketplace(requestBody);

            IGSUserData.MarketplaceHistory = result;

            Loading.HideAllPanels();

			return result;
		}

		private void ProcessRequestDataResult(string result)
		{
			if (result == null)
			{
				Message.Show(MessageCode.FAILED_TO_LOAD_DATA);
			}

			var jObjectResult = JsonConvert.DeserializeObject<JObject>(result);

			if (jObjectResult.ContainsKey("Message"))
			{
				Message.Show(jObjectResult["Message"].ToString());
				return;
			}

			var items = JsonConvert.DeserializeObject<List<MarketplaceActiveOffer>>(jObjectResult["Items"].ToString());

			_continuationToken = jObjectResult["ContinuationToken"]?.ToString();

            if (items.Count == 0)
			{
				_voidText.gameObject.SetActive(true);
			}
			else
			{
				_voidText.gameObject.SetActive(false);
			}

			InstantiateItems(items);
		}

		private void InstantiateItems(List<MarketplaceActiveOffer> items)
		{
			if (_destoyChildrenOnInstantiate)
			{
				foreach (Transform child in _parent)
				{
					Destroy(child.gameObject);
				}
			}

			foreach (var item in items)
			{
				var offer = Instantiate(_offerItem, _parent);

				Enum.TryParse(item.CurrencyID, out VirtualCurrencyID virtualCurrencyID);

				var currencyName = virtualCurrencyID == VirtualCurrencyID.IG ? JsonProperty.IGT.ToUpper() : JsonProperty.IGC.ToUpper();

				Sprite currencyIcon = Resources.Load<Sprite>(UserDataService.CURRENCY_ICONS_IMAGE_PATH + currencyName);

				offer.Fill(item, currencyIcon);
			}

			StartCoroutine(SetLoadMoreButton());
		}

		private IEnumerator SetLoadMoreButton()
		{
			yield return null;

            if (!string.IsNullOrEmpty(_continuationToken))
            {
				if (_loadMoreButton == null)
				{
					_loadMoreButton = Instantiate(_loadMoreButtonPrefab, _parent);
				}

				_loadMoreButton.ResetButton(LoadMore);
			}
			else
			{
				if (_loadMoreButton != null)
				{
					Destroy(_loadMoreButton.gameObject);
				}
			}
		}

		private async void LoadMore()
		{
			_destoyChildrenOnInstantiate = false;
			var result = await RequestData();
			ProcessRequestDataResult(result);
		}
#endif

    }
}
