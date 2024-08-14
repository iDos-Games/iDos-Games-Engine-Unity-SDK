using System;

namespace IDosGames
{
	[Serializable]
	public enum AlarmType
	{
		AvailableFreeItemOnShop,
		AvailableUpdateDailyFreeProducts,
		AvailableStandardSpin,
		AvailablePremiumSpin,
		AvailableChest,
		OpenedInviteFriendsPopUp,
		OpenedAuthorizationPopUp,
		OpenedLeaderboardWindow,
        AvailableNewFriend
    }
}