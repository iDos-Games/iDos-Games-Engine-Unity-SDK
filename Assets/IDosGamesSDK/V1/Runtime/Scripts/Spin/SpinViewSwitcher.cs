using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	public class SpinViewSwitcher : MonoBehaviour
	{
		[SerializeField] private Button _buttonHandler;
		[SerializeField] private Slider _sliderHandler;
		[SerializeField] private SpinWindowView _spinWindowView;
		[SerializeField] private SpinButtonsSwitcher _spinButtonsSwitcher;

		private const float SLIDER_STANDARD_VALUE = 0.5f;
		private const float SLIDER_PREMIUM_VALUE = 1.0f;

		private SpinTicketType _currentSpinView = SpinTicketType.Standard;
		public SpinTicketType CurrentSpinView => _currentSpinView;

		private void OnEnable()
		{
			SwitchToStandard();
		}

		private void OnDisable()
		{
			SwitchToStandard();
		}

		private void Start()
		{
			ResetSwitchButton();
		}

		private void ResetSwitchButton()
		{
			_buttonHandler.onClick.RemoveAllListeners();
			_buttonHandler.onClick.AddListener(() => Switch());
		}

		public void Switch()
		{
			if (_currentSpinView == SpinTicketType.Standard)
			{
				SwitchToPremium();
			}
			else
			{
				SwitchToStandard();
			}
		}

		public void SwitchToStandard()
		{
			if (_currentSpinView == SpinTicketType.Standard)
			{
				return;
			}

			ChangeSliderValue(SpinTicketType.Standard);
			ChangeCurrentSpinView(SpinTicketType.Standard);
			_spinButtonsSwitcher.Switch(SpinTicketType.Standard);
			_spinWindowView.SwitchRewards(SpinTicketType.Standard);
		}

		public void SwitchToPremium()
		{
			if (_currentSpinView == SpinTicketType.Premium)
			{
				return;
			}

			ChangeSliderValue(SpinTicketType.Premium);
			ChangeCurrentSpinView(SpinTicketType.Premium);
			_spinButtonsSwitcher.Switch(SpinTicketType.Premium);
			_spinWindowView.SwitchRewards(SpinTicketType.Premium);
		}

		private void ChangeSliderValue(SpinTicketType type)
		{
			_sliderHandler.value = type == SpinTicketType.Standard ? SLIDER_STANDARD_VALUE : SLIDER_PREMIUM_VALUE;
		}

		private void ChangeCurrentSpinView(SpinTicketType type)
		{
			_currentSpinView = type == SpinTicketType.Standard ? SpinTicketType.Standard : SpinTicketType.Premium;
		}
	}
}