using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

#pragma warning disable 0414
#pragma warning disable 0067
namespace IDosGames
{
    public class PanelOffersByItemID : MonoBehaviour
    {
        public string ItemID { get; private set; }

        [Range(1, 100)][SerializeField] private int _itemsInOnePage = 20;
        public int ItemsInOnePage => _itemsInOnePage;

        [SerializeField] private MarketplaceOfferItem _offerItem;
        [SerializeField] private Transform _parent;
        [SerializeField] private LoadMoreButton _loadMoreButtonPrefab;
        [SerializeField] private TMP_Text _voidText;
        [SerializeField] private PopUpConfirmation _popUpConfirmation;
        [SerializeField] private GroupedOfferItem _offerItemOnTopView;
        [SerializeField] private OffersByItemIDFilterPanel _filter;
        private LoadMoreButton _loadMoreButton;

        private string _continuationToken;
        private bool _destoyChildrenOnInstantiate = true;

        public static event Action OfferBuyed;

#if IDOSGAMES_MARKETPLACE
        public async Task OpenPanel(string itemID)
        {
            _continuationToken = null;
            _destoyChildrenOnInstantiate = true;
            ItemID = itemID;

            _offerItemOnTopView.Fill(null, itemID, 0);

            var result = await RequestData();
            ProcessRequestDataResult(result);

            gameObject.SetActive(true);
        }

        public async void ApplyFilters()
        {
            _continuationToken = null;
            _destoyChildrenOnInstantiate = true;
            var result = await RequestData();
            ProcessRequestDataResult(result);
        }

        public async void Refresh()
        {
            _filter.ResetAll();
            _continuationToken = null;
            _destoyChildrenOnInstantiate = true;
            var result = await RequestData();
            ProcessRequestDataResult(result);
        }

        private async Task<string> RequestData()
        {
            Loading.ShowTransparentPanel();

            string currencyID = _filter.CurrencyIDValue == 0 ? null : ((VirtualCurrencyID)_filter.CurrencyIDValue - 1).ToString();

            var requestBody = new MarketplaceGetDataRequest
            {
                Panel = MarketplacePanel.ActiveOffersByItemID,
                ItemID = ItemID,
                CurrencyID = currencyID,
                ItemsInOnePage = _itemsInOnePage,
                ContinuationToken = _continuationToken,
                SortOrder = _filter.SortOrder,
                OrderBy = _filter.OrderBy
            };

            var result = await IGSService.GetDataFromMarketplace(requestBody);

            Loading.HideAllPanels();

            return result;
        }

        private async Task<string> RequestBuyOffer(string id)
        {
            Loading.ShowTransparentPanel();

            var request = new MarketplaceActionRequest
            {
                Action = MarketplaceAction.BuyOffer,
                OfferID = id
            };

            var result = await IGSService.TryDoMarketplaceAction(request);

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

                offer.Fill(() => ShowConfirmationPopUp(item, currencyIcon), item.SellerID, (int)item.Price, currencyIcon);
            }

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            StartCoroutine(SetLoadMoreButton());
        }

        private void ShowConfirmationPopUp(MarketplaceActiveOffer offer, Sprite currencyIcon)
        {
            var skinItem = UserDataService.GetCachedSkinItem(offer.ItemID);
            if (skinItem == null)
            {
                skinItem = UserDataService.GetAvatarSkinItem(offer.ItemID);
            }

            _popUpConfirmation.FullSet(() => BuyOffer(offer), skinItem.DisplayName, ((int)offer.Price).ToString(), currencyIcon);
            _popUpConfirmation.gameObject.SetActive(true);
        }

        private async void BuyOffer(MarketplaceActiveOffer offer)
        {
            Enum.TryParse(offer.CurrencyID, out VirtualCurrencyID virtualCurrencyID);

            int balance = UserInventory.GetVirtualCurrencyAmount(virtualCurrencyID.ToString());

            if (balance < offer.Price)
            {
                Message.Show(MessageCode.NOT_ENOUGH_FUNDS);

                if (virtualCurrencyID == VirtualCurrencyID.IG)
                {
                    ShopSystem.PopUpSystem.ShowTokenPopUp();
                }
                else if (virtualCurrencyID == VirtualCurrencyID.CO)
                {
                    ShopSystem.PopUpSystem.ShowCoinPopUp();
                }

                return;
            }

            var result = await RequestBuyOffer(offer.ID);

            if (result == null)
            {
                Message.Show(MessageCode.FAILED_TO_BUY_OFFER);
                return;
            }

            var jObjectResult = JsonConvert.DeserializeObject<JObject>(result);

            if (jObjectResult.ContainsKey("Message"))
            {
                Message.Show(jObjectResult["Message"].ToString());
            }

            OfferBuyed?.Invoke();

            UserDataService.RequestUserAllData();
            Refresh();
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
