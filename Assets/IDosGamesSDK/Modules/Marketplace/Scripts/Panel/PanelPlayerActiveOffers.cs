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
    public class PanelPlayerActiveOffers : MonoBehaviour
    {
        [Range(1, 100)][SerializeField] private int _itemsInOnePage = 20;
        public int ItemsInOnePage => _itemsInOnePage;

        [SerializeField] private MarketplacePlayerActiveOffer _offerItem;
        [SerializeField] private Transform _parent;
        [SerializeField] private LoadMoreButton _loadMoreButtonPrefab;
        [SerializeField] private TMP_Text _voidText;
        [SerializeField] private ActiveOfferMenu _activeOfferMenu;
        [SerializeField] private PopUpConfirmation _deleteConfirmationPopUp;
        [SerializeField] private MarketplaceEditOfferPopUp _editOfferPopUp;
        private LoadMoreButton _loadMoreButton;

        public bool IsNeedUpdate { get; set; } = true;

        private string _continuationToken;
        private bool _destoyChildrenOnInstantiate = true;

        public static event Action OfferChanged;

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
            if (string.IsNullOrEmpty(IGSUserData.MarketplaceActiveOffers))
            {
                Refresh();
            }
            else
            {
                string activeOffers = IGSUserData.MarketplaceActiveOffers;
                _continuationToken = null;
                _destoyChildrenOnInstantiate = true;
                ProcessRequestDataResult(activeOffers);
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
                Panel = MarketplacePanel.PlayerActiveOffers,
                ItemsInOnePage = _itemsInOnePage,
                ContinuationToken = _continuationToken?.ToString()
            };

            var result = await IGSService.GetDataFromMarketplace(requestBody);

            Loading.HideAllPanels();

            IGSUserData.MarketplaceActiveOffers = result;

            return result;
        }

        private async Task<string> RequestDeleteOffer(string id)
        {
            Loading.ShowTransparentPanel();

            var requestBody = new MarketplaceActionRequest
            {
                Action = MarketplaceAction.DeleteOffer,
                OfferID = id
            };

            var result = await IGSService.TryDoMarketplaceAction(requestBody);

            Loading.HideAllPanels();

            return result;
        }

        private async Task<string> RequestUpdateOffer(string id, VirtualCurrencyID currencyID, int price)
        {
            Loading.ShowTransparentPanel();

            var request = new MarketplaceActionRequest
            {
                Action = MarketplaceAction.UpdateOffer,
                OfferID = id,
                CurrencyID = currencyID,
                Price = price
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

                offer.Fill(() => ShowOfferMenuPopUp(offer.transform, item, currencyIcon, virtualCurrencyID), item, currencyIcon);
            }

            StartCoroutine(SetLoadMoreButton());
        }

        private void ShowOfferMenuPopUp(Transform transform, MarketplaceActiveOffer offer, Sprite currencyIcon, VirtualCurrencyID currencyID)
        {
            _activeOfferMenu.ShowPopUp
            (
                transform,
                offer?.ItemID,
                () => ShowEditOfferPopUp(offer?.ID, currencyID, (int)offer?.Price),
                () => ShowDeleteConfirmationPopUp(offer, currencyIcon)
            );
        }

        private void ShowEditOfferPopUp(string id, VirtualCurrencyID currencyID, int price)
        {
            _editOfferPopUp.Show((curencyID, price) => EditOffer(id, curencyID, price), currencyID, price);
        }

        private async void EditOffer(string id, VirtualCurrencyID currencyID, int price)
        {
            var result = await RequestUpdateOffer(id, currencyID, price);

            if (result == null)
            {
                Message.Show(MessageCode.FAILED_TO_UPDATE_OFFER);
                return;
            }

            var jObjectResult = JsonConvert.DeserializeObject<JObject>(result);

            if (jObjectResult.ContainsKey("Message"))
            {
                Message.Show(jObjectResult["Message"].ToString());
            }

            UserDataService.RequestUserAllData();
            Refresh();

            OfferChanged?.Invoke();
        }

        private void ShowDeleteConfirmationPopUp(MarketplaceActiveOffer offer, Sprite currencyIcon)
        {
            var item = UserDataService.GetCachedSkinItem(offer?.ItemID);
            if (item == null)
            {
                item = UserDataService.GetAvatarSkinItem(offer?.ItemID);
            }
            _deleteConfirmationPopUp.FullSet(() => DeleteOffer(offer), item.DisplayName, ((int)offer.Price).ToString(), currencyIcon);
            _deleteConfirmationPopUp.gameObject.SetActive(true);
        }

        private async void DeleteOffer(MarketplaceActiveOffer offer)
        {
            var result = await RequestDeleteOffer(offer.ID);

            if (result == null)
            {
                Message.Show(MessageCode.FAILED_TO_DELETE_OFFER);
                return;
            }

            var jObjectResult = JsonConvert.DeserializeObject<JObject>(result);

            if (jObjectResult.ContainsKey("Message"))
            {
                Message.Show(jObjectResult["Message"].ToString());
            }

            UserDataService.RequestUserAllData();
            Refresh();

            OfferChanged?.Invoke();
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
