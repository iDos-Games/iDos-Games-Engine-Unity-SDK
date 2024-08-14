using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	public class WeeklyEventWindowView : MonoBehaviour
	{
		[SerializeField] private Timer _timer;

		[Header("Slider")]
		[SerializeField] private Slider _slider;
		[SerializeField] private TMP_Text _sliderText;

		[SerializeField] private TMP_Text _folowingLevel;

		[SerializeField] private ScrollRect _rewardsScrollRect;

		private void OnEnable()
		{
			UpdateView();
			WeeklyEventSystem.DataUpdated += UpdateView;
		}

		private void OnDisable()
		{
			WeeklyEventSystem.DataUpdated -= UpdateView;
		}

		public void ScrollToTargetReward()
		{
			if (WeeklyEventSystem.FollowingReward == null)
			{
				return;
			}

			int followingRewardID = int.Parse($"{WeeklyEventSystem.FollowingReward[JsonProperty.ID]}");

			float normalizedPosition = 1.0f;

			if (followingRewardID < WeeklyEventSystem.Rewards.Count)
			{
				normalizedPosition = (float)(followingRewardID - 1) / WeeklyEventSystem.Rewards.Count;
			}

			StartCoroutine(ScrollToTargetPosition(normalizedPosition));
		}

		private IEnumerator ScrollToTargetPosition(float normalizedPosition)
		{
			yield return null;

			_rewardsScrollRect.verticalNormalizedPosition = 1 - normalizedPosition;
		}

		private void UpdateView()
		{
			UpdateSlider();
			UpdateTimer();
			UpdateFollowingLevel();
		}

		private void UpdateSlider()
		{
			if (WeeklyEventSystem.SliderText != null)
			{
				_sliderText.text = WeeklyEventSystem.SliderText;
			}

			_slider.value = WeeklyEventSystem.SliderValue;
		}

		private void UpdateFollowingLevel()
		{
			if (WeeklyEventSystem.FollowingReward == null)
			{
				return;
			}

			_folowingLevel.text = $"{WeeklyEventSystem.FollowingReward[JsonProperty.ID]}";
		}

		private void UpdateTimer()
		{
			_timer.Set(WeeklyEventSystem.EndDate);
		}
	}
}
