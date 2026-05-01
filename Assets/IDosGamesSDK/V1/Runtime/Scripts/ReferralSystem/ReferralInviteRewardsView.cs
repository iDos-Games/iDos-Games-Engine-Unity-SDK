using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	public class ReferralInviteRewardsView : MonoBehaviour
	{
		[SerializeField] private Slider _slider;

		[SerializeField] private List<ReferralInviteRewardItem> _rewards;
		public List<ReferralInviteRewardItem> Rewards => _rewards;

		public void SetSliderValue(int value)
		{
			_slider.value = value;
		}
	}
}