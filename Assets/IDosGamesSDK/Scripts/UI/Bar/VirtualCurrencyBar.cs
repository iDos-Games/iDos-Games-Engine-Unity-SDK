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
            UserInventory.InventoryUpdated += UpdateAmount;
            UserDataService.VirtualCurrencyUpdated += UpdateAmount;
        }

        private void OnDisable()
        {
            UserInventory.InventoryUpdated -= UpdateAmount;
            UserDataService.VirtualCurrencyUpdated -= UpdateAmount;
        }

        public override void UpdateAmount()
        {
            Amount = IGSUserData.UserInventory.VirtualCurrency.GetValueOrDefault(_virtualCurrencyID, 0);
        }
    }
}