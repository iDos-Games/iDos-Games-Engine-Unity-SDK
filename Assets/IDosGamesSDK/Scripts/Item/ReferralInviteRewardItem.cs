
using UnityEngine;

namespace IDosGames
{
	public class ReferralInviteRewardItem : RewardItem
	{
		[SerializeField] private int _followersAmount;
		public int FollowersAmount => _followersAmount;

		[SerializeField] private GameObject _checkMark;

		public void SetActiveCheckMark(bool active)
		{
			_checkMark.SetActive(active);
		}
	}
}