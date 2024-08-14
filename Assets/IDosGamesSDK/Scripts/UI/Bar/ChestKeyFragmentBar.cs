using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IDosGames
{
	public class ChestKeyFragmentBar : CurrencyBar
	{
		[SerializeField] private ChestKeyFragmentType _fragmentType;

		private void OnEnable()
		{
			UpdateAmount();
			UserInventory.InventoryUpdated += UpdateAmount;
		}

		private void OnDisable()
		{
			UserInventory.InventoryUpdated -= UpdateAmount;
		}

		public override void UpdateAmount()
		{
			Amount = UserInventory.GetChestKeyFragmentAmount(_fragmentType);
		}
	}
}