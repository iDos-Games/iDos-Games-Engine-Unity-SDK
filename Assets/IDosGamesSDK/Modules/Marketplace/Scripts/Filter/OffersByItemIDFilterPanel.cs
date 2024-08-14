using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	public class OffersByItemIDFilterPanel : MonoBehaviour
	{
		[SerializeField] private TMP_Dropdown _orderByDropdown;
		[SerializeField] private TMP_Dropdown _sortOrderDropdown;
		[SerializeField] private TMP_Dropdown _currencyDropdown;
		[SerializeField] private Button _applyButton;

		public int CurrencyIDValue { get; private set; }
		public MarketplaceOrderBy OrderBy { get; private set; }
		public MarketplaceSortOrder SortOrder { get; private set; }

		private void OnDisable()
		{
			ResetAll();
		}

		private void Start()
		{
			_orderByDropdown.onValueChanged.AddListener(OnOrderByValueChanged);
			_sortOrderDropdown.onValueChanged.AddListener(OnSortOrderValueChanged);
			_currencyDropdown.onValueChanged.AddListener(OnCurrencyOrderValueChanged);
		}

		public void ResetAll()
		{
			ResetDropdowns();
			ResetProperties();
			SetActiveteApplyButton(false);
		}

		private void OnOrderByValueChanged(int value)
		{
			OrderBy = (MarketplaceOrderBy)value;
			SetActiveteApplyButton(true);
		}

		private void OnSortOrderValueChanged(int value)
		{
			SortOrder = (MarketplaceSortOrder)value;
			SetActiveteApplyButton(true);
		}

		private void OnCurrencyOrderValueChanged(int value)
		{
			CurrencyIDValue = value;
			SetActiveteApplyButton(true);
		}

		private void ResetDropdowns()
		{
			_orderByDropdown.value = 0;
			_sortOrderDropdown.value = 0;
			_currencyDropdown.value = 0;
		}
		private void ResetProperties()
		{
			CurrencyIDValue = 0;
			SortOrder = 0;
			OrderBy = 0;
		}

		private void SetActiveteApplyButton(bool active)
		{
			_applyButton.gameObject.SetActive(active);
		}
	}
}
