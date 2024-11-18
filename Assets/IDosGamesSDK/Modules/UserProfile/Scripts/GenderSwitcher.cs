using IDosGames.TitlePublicConfiguration;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames.UserProfile
{

    public class GenderSwitcher : MonoBehaviour
    {
        [SerializeField] private Button _buttonHandler;
        [SerializeField] private Slider _sliderHandler;
        [SerializeField] private AvatarProfileSettings profileSettings;

        private const float SLIDER_STANDARD_VALUE = 0.5f;
        private const float SLIDER_PREMIUM_VALUE = 1.0f;

        private Gender _currentGender;
        public Gender CurrentSpinView => _currentGender;

        private void OnEnable()
        {
            _currentGender = profileSettings.GetAvatarGender();
            ChangeSliderValue(_currentGender);
            ChangeCurrentSpinView(_currentGender);
            ResetSwitchButton();
        }
        private void Start()
        {

        }

        private void ResetSwitchButton()
        {
            _buttonHandler.onClick.RemoveAllListeners();
            _buttonHandler.onClick.AddListener(() => Switch());
        }

        public void Switch()
        {
            if (_currentGender == Gender.Male)
            {
                SwitchToFemale();
            }
            else
            {
                SwitchToMale();
            }
        }

        public void SwitchToMale()
        {
            if (_currentGender == Gender.Male)
            {
                return;
            }

            ChangeSliderValue(Gender.Male);
            ChangeCurrentSpinView(Gender.Male);
            profileSettings.ChangeGender(Gender.Male);
        }

        public void SwitchToFemale()
        {
            if (_currentGender == Gender.Female)
            {
                return;
            }

            ChangeSliderValue(Gender.Female);
            ChangeCurrentSpinView(Gender.Female);
            profileSettings.ChangeGender(Gender.Female);
        }

        private void ChangeSliderValue(Gender type)
        {
            _sliderHandler.value = type == Gender.Male ? SLIDER_STANDARD_VALUE : SLIDER_PREMIUM_VALUE;
        }

        private void ChangeCurrentSpinView(Gender type)
        {
            _currentGender = type == Gender.Male ? Gender.Male : Gender.Female;
        }
    }
}
