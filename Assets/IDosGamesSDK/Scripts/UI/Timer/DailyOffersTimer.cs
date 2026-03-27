namespace IDosGames
{
	public class DailyOffersTimer : Timer
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

		private async void UpdateShop()
		{
            await UserService.GetClientState();
        }
	}
}