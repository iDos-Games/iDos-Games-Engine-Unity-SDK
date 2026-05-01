using System.Collections.Generic;
using UnityEngine;

namespace IDosGames
{
    public class VirtualCurrencyBar : CurrencyBar
    {
        [SerializeField] private string _virtualCurrencyID;

        private void OnEnable()
        {
            UpdateAmount();
            IDosGamesData.User.OnInventoryUpdated += UpdateAmount;
            IDosGamesData.User.OnVirtualCurrencyUpdated += UpdateAmount;
            IDosGamesData.User.OnAnyUpdated += UpdateAmount;
        }

        private void OnDisable()
        {
            IDosGamesData.User.OnInventoryUpdated -= UpdateAmount;
            IDosGamesData.User.OnVirtualCurrencyUpdated -= UpdateAmount;
            IDosGamesData.User.OnAnyUpdated -= UpdateAmount;
        }

        public override void UpdateAmount()
        {
            if (IDosGamesData.User == null || IDosGamesData.User.VirtualCurrency == null)
            {
                Amount = 0;
                return;
            }

            Amount = IDosGamesData.User.VirtualCurrency.GetValueOrDefault(_virtualCurrencyID, 0);
        }
    }
}