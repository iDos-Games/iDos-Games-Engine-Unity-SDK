using System;
using UnityEngine;

namespace IDosGames
{
    public class ShopPopUpSystem : MonoBehaviour
    {
        [SerializeField] private GameObject _shopWindow;
        [SerializeField] private PopUp _vipPopUp;
        [SerializeField] private PopUp _tokenPopUp;
        [SerializeField] private PopUp _coinPopUp;
        [SerializeField] private PopUp _wkPopUp;
        [SerializeField] private PopUp _spinTicketPopUp;
        [SerializeField] private PopUpConfirmation _confirmationPopUp;

        private void Awake()
        {
            HideAllPopUps();
        }

        public void HideAllPopUps()
        {
            SetActivatePopUp(_vipPopUp, false);
            SetActivatePopUp(_spinTicketPopUp, false);
            SetActivatePopUp(_wkPopUp, false);
            SetActivatePopUp(_tokenPopUp, false);
            SetActivatePopUp(_coinPopUp, false);
            SetActivatePopUp(_confirmationPopUp, false);
        }

        private void SetActivatePopUp(PopUp popUp, bool active)
        {
            if (popUp == null)
            {
                return;
            }

            popUp.gameObject.SetActive(active);
        }

        public void ShowShopWindow()
        {
            _shopWindow.SetActive(true);
        }

        public void ShowTokenPopUp()
        {
            SetActivatePopUp(_tokenPopUp, true);
        }

        public void ShowCoinPopUp()
        {
            SetActivatePopUp(_coinPopUp, true);
        }

        public void ShowWKPopUp()
        {
            SetActivatePopUp(_wkPopUp, true);
        }

        public void ShowSpinTicketPopUp()
        {
            SetActivatePopUp(_spinTicketPopUp, true);
        }

        public void ShowVIPPopUp()
        {
            SetActivatePopUp(_vipPopUp, true);
        }

        public void ShowConfirmationPopUp(Action confirmAction, string productName, string price, Sprite currencyIcon)
        {
            _confirmationPopUp.FullSet(confirmAction, productName, price, currencyIcon);
            SetActivatePopUp(_confirmationPopUp, true);
        }
    }
}