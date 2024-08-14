using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
    public class SecondarySpinButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _adIcon;
        [SerializeField] private TMP_Text _usesLeft;

        public void Set(Action tryToSpinAction, int ticketsAmount, bool showAd)
        {
            bool block = ticketsAmount <= 0;

            ResetButtonListener(tryToSpinAction);

            if (block)
            {
                Block();
            }
            else
            {
                SetUsesLeftText(ticketsAmount);
                Unblock(showAd);
            }
        }

        private void ResetButtonListener(Action action)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => action());
        }

        private void Block()
        {
            SetActiveAdIcon(false);
            SetActiveUsesLeftText(false);

            _button.interactable = false;

        }

        private void Unblock(bool showAd)
        {
            SetActiveAdIcon(showAd);
            SetActiveUsesLeftText(true);

            _button.interactable = true;
        }

        private void SetActiveAdIcon(bool active)
        {
            _adIcon.gameObject.SetActive(active);
        }

        private void SetUsesLeftText(int ticketsAmount)
        {
            _usesLeft.text = $"{ticketsAmount}/{UserInventory.SecondarySpinTicketRechargeMax}";
        }

        private void SetActiveUsesLeftText(bool active)
        {
            _usesLeft.gameObject.SetActive(active);
        }
    }
}
