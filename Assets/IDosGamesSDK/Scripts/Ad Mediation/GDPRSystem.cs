using System.Globalization;
using System.Linq;
using UnityEngine;

namespace IDosGames
{
	public class GDPRSystem : MonoBehaviour
	{
		[SerializeField] private PopUpGDPR _popUp;

		private readonly string[] EU_COUNTRIES = { "AT", "BE", "BG", "HR", "CY", "CZ", "DK", "EE", "FI", "FR",
												   "DE", "GR", "HU", "IE", "IT", "LV", "LT", "LU", "MLT", "NL",
												   "PL", "PT", "RO", "SK", "SI", "ES", "SE", "CA" };

		private const string PLAYER_PREFS_PLAYER_COUNTRY = "PLAYER_COUNTRY";
		public string PlayerCountry => PlayerPrefs.GetString(PLAYER_PREFS_PLAYER_COUNTRY, string.Empty);

		public const int DELAY_REQUEST_PLAYER_PROFILE = 2;

#if !UNITY_IOS
		private void Start()
		{
			Invoke(nameof(RequestPlayerProfile), DELAY_REQUEST_PLAYER_PROFILE);
		}
#endif

		private void RequestPlayerProfile()
		{
			if (PlayerCountry != string.Empty)
			{
				return;
			}

            RegionInfo regionInfo = RegionInfo.CurrentRegion;
            string playerCountry = regionInfo.TwoLetterISORegionName;

            if (EU_COUNTRIES.Contains(playerCountry))
            {
                ShowPopUp();
            }
            else
            {
                OnConsentSelected(true);
            }

            SavePlayerCountry($"{playerCountry}");
		}

		private void SavePlayerCountry(string country)
		{
			PlayerPrefs.SetString(PLAYER_PREFS_PLAYER_COUNTRY, country);
			PlayerPrefs.Save();
		}

		private void ShowPopUp()
		{
			var popUp = Instantiate(_popUp);
			popUp.ResetButtons(() => OnConsentSelected(true), () => OnConsentSelected(false));
		}

		public void OnConsentSelected(bool isAccepted)
		{
			AdMediation.Instance.SetConsent(isAccepted);
		}
	}
}