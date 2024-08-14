using UnityEngine;

namespace IDosGames
{
	public class WeeklyEventWindow : MonoBehaviour
	{
		[SerializeField] private WeeklyEventWindowView _view;
		[SerializeField] private WeeklyEventItem _rewardsPrefab;
		[SerializeField] private Transform _parent;

		private void OnEnable()
		{
			UpdateRewards();
		}

		public void UpdateRewards()
		{
			try
			{
				foreach (Transform child in _parent)
				{
					Destroy(child.gameObject);
				}

				foreach (var rewardData in WeeklyEventSystem.Rewards)
				{
					var reward = Instantiate(_rewardsPrefab, _parent);
					reward.Set(rewardData);
				}

				_view.ScrollToTargetReward();
			}
			catch
			{
			}
		}
	}
}
