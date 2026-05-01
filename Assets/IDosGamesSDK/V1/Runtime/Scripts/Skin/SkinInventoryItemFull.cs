using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	public class SkinInventoryItemFull : SkinInventoryItem
	{
		[SerializeField] private TMP_Text _profit;
		[SerializeField] private TMP_Text _name;
		[SerializeField] private Image _checkMark;

		public SkinCatalogItem SkinData { get; private set; }

		public new void Fill(Action action, SkinCatalogItem item)
		{
			base.Fill(action, item);

			_profit.text = $"+{item.Profit}";
			_name.text = item.DisplayName;

			SkinData = item;
		}

		public void SetActivateCheckMark(bool active)
		{
			_checkMark.gameObject.SetActive(active);
		}
	}
}
