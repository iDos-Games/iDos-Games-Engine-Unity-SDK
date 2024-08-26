using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

#pragma warning disable 0414
namespace IDosGames
{
	public class PanelGroupedOffers : MonoBehaviour
	{
		private const float FAKE_LOADING_DURATION = 0.5f;
		[Range(1, 100)][SerializeField] private int _itemsInOnePage = 20;
		public int ItemsInOnePage => _itemsInOnePage;

		[SerializeField] private MarketplaceWindow _marketplaceWindow;
		[SerializeField] private GroupedOfferItem _groupedOfferItem;
		[SerializeField] private Transform _parent;
		[SerializeField] private LoadMoreButton _loadMoreButtonPrefab;
		[SerializeField] private TMP_Text _voidText;
		[SerializeField] private GroupedOfferMenu _offerMenu;
		private LoadMoreButton _loadMoreButton;

		public bool IsNeedUpdate { get; set; } = true;

		private List<MarketplaceGroupedOffer> _allOffers = new();

		private int _currentPage = 0;

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
            if (string.IsNullOrEmpty(IGSUserData.MarketplaceGroupedOffers))
            {
                Refresh();
            }
            else
            {
                string offers = IGSUserData.MarketplaceGroupedOffers;
                ProcessRequestResult(offers);
                IsNeedUpdate = false;
            }
        }

        public async void Refresh()
		{
			var requestResult = await RequestData();
			ProcessRequestResult(requestResult);
			IsNeedUpdate = false;
		}

		private async Task<string> RequestData()
		{
			Loading.ShowTransparentPanel();

			var request = new MarketplaceGetDataRequest
			{
				Panel = MarketplacePanel.GroupedOffers,
				ItemsInOnePage = _itemsInOnePage
			};

			var result = await IGSService.GetDataFromMarketplace(request);

			IGSUserData.MarketplaceGroupedOffers = result;

            Loading.HideAllPanels();

			return result;
		}

		private void ProcessRequestResult(string result)
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

			_allOffers = JsonConvert.DeserializeObject<List<MarketplaceGroupedOffer>>(jObjectResult["Items"].ToString());

			if (_allOffers.Count == 0)
			{
				_voidText.gameObject.SetActive(true);
			}
			else
			{
				_voidText.gameObject.SetActive(false);
			}

			_allOffers = _allOffers.OrderByDescending(x => x.OfferCount).ToList();

			InstantiateItems();
		}

		private void InstantiateItems(bool destroyChildren = true)
		{
			if (destroyChildren)
			{
				foreach (Transform child in _parent)
				{
					Destroy(child.gameObject);
				}

				_currentPage = 0;
			}

			for (int i = _currentPage * _itemsInOnePage; i < (_currentPage + 1) * _itemsInOnePage; i++)
			{
				if (i >= _allOffers.Count)
				{
					break;
				}

				var item = _allOffers[i];
				var offer = Instantiate(_groupedOfferItem, _parent);

				offer.Fill(() => _offerMenu.ShowPopUp(offer.transform, item.ItemID), item.ItemID, item.OfferCount);
			}

			bool loadMore = (_currentPage + 1) * _itemsInOnePage < _allOffers.Count;
			_currentPage++;

			StartCoroutine(SetLoadMoreButton(loadMore));
		}

		private IEnumerator SetLoadMoreButton(bool loadMore)
		{
			yield return null;

			if (loadMore)
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

		private void LoadMore()
		{
			StartCoroutine(LoadMoreCoroutine());
		}

		private IEnumerator LoadMoreCoroutine()
		{
			Loading.ShowTransparentPanel();
			yield return new WaitForSeconds(FAKE_LOADING_DURATION);
			InstantiateItems(false);
			Loading.HideAllPanels();
		}
#endif

    }
}
