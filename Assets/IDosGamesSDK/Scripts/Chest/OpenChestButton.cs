using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	[RequireComponent(typeof(Button))]
	public class OpenChestButton : MonoBehaviour
	{
		[SerializeField] private ChestRarityType _rarityType;
		[SerializeField] private ChestWindow _chestWindow;
		[SerializeField] private GameObject _alarm;

		public Button Button => _button;
		private Button _button;

		private bool _canOpen => HasAllKeyFragments();

		private void Awake()
		{
			_button = GetComponent<Button>();
			ResetListener();
		}

		private void OnEnable()
		{
			UpdateUI();
			UserInventory.InventoryUpdated += UpdateUI;
		}

		private void OnDisable()
		{
			UserInventory.InventoryUpdated -= UpdateUI;
		}

		private void ResetListener()
		{
			_button.onClick.RemoveAllListeners();
			_button.onClick.AddListener(OnClickOpenButton);
		}

		private void OnClickOpenButton()
		{
			if (_canOpen)
			{
				_chestWindow.TryToOpenChest(_rarityType);
			}
			else
			{
				_chestWindow.OpenSpinWindow();
				Message.Show(MessageCode.NOT_ENOUGH_KEY_FRAGMENTS);
			}

			UpdateUI();
		}

		protected virtual void UpdateUI()
		{
			SetButtonInteractable(true);
			_alarm.SetActive(_canOpen);
		}

		private bool HasAllKeyFragments()
		{
			switch (_rarityType)
			{
				case ChestRarityType.Common:
					{

						if (HasKeyFragment(ChestKeyFragmentType.Common_1) &&
							HasKeyFragment(ChestKeyFragmentType.Common_2) &&
							HasKeyFragment(ChestKeyFragmentType.Common_3))
						{
							return true;
						}
						break;
					}
				case ChestRarityType.Rare:
					{

						if (HasKeyFragment(ChestKeyFragmentType.Rare_1) &&
							HasKeyFragment(ChestKeyFragmentType.Rare_2) &&
							HasKeyFragment(ChestKeyFragmentType.Rare_3))
						{
							return true;
						}
						break;
					}
				case ChestRarityType.Legendary:
					{

						if (HasKeyFragment(ChestKeyFragmentType.Legendary_1) &&
							HasKeyFragment(ChestKeyFragmentType.Legendary_2) &&
							HasKeyFragment(ChestKeyFragmentType.Legendary_3))
						{
							return true;
						}
						break;
					}
			}

			return false;
		}

		private bool HasKeyFragment(ChestKeyFragmentType type)
		{
			if (UserInventory.GetChestKeyFragmentAmount(type) > 0)
			{
				return true;
			}
			return false;
		}

		private void SetButtonInteractable(bool interactable)
		{
			_button.interactable = interactable;
		}
	}
}