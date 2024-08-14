using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	[RequireComponent(typeof(Button))]
	public class ButtonWeeklyEvent : MonoBehaviour
	{
		[SerializeField] private Timer _timer;

		[Header("Slider")]
		[SerializeField] private Slider _slider;
		[SerializeField] private TMP_Text _sliderText;

		[Header("Following Reward")]
		[SerializeField] private Image _rewardIcon;
		[SerializeField] private TMP_Text _rewardAmount;

		private void OnEnable()
		{
			UpdateView();
			WeeklyEventSystem.DataUpdated += UpdateView;
		}

		private void OnDisable()
		{
			WeeklyEventSystem.DataUpdated -= UpdateView;
		}

		private void UpdateView()
		{
			UpdateSlider();
			UpdateTimer();
			UpdateFollowingReward();
		}

		private void UpdateSlider()
		{
			if (WeeklyEventSystem.SliderText != null)
			{
				_sliderText.text = WeeklyEventSystem.SliderText;
			}

			_slider.value = WeeklyEventSystem.SliderValue;
		}

		private void UpdateFollowingReward()
		{
			if (WeeklyEventSystem.FollowingReward == null)
			{
				return;
			}

			var reward = UserInventory.HasVIPStatus ?
				WeeklyEventSystem.FollowingReward[JsonProperty.PREMIUM] :
				WeeklyEventSystem.FollowingReward[JsonProperty.STANDARD];

			_rewardAmount.text = $"{reward[JsonProperty.AMOUNT]}";
			_rewardIcon.sprite = Resources.Load<Sprite>($"{reward[JsonProperty.IMAGE_PATH]}");
		}

		private void UpdateTimer()
		{
			_timer.Set(WeeklyEventSystem.EndDate);
		}
	}
}
