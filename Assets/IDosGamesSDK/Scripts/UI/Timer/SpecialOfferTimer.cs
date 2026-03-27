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

		private async void UpdateShop()
		{
            await UserService.GetClientState();
        }
	}
}