namespace IDosGames
{
	public class SpecialOfferTimer : Timer
	{
		protected override void OnEnable()
		{
			base.OnEnable();
			TimerStopped += UpdateShop;
		}

		private void OnDisable()
		{
			TimerStopped -= UpdateShop;
		}

		private void UpdateShop()
		{
			UserDataService.RequestUserAllData();
		}
	}
}