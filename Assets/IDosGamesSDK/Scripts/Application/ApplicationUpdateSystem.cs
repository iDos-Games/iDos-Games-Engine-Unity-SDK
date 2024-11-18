using UnityEngine;

namespace IDosGames
{
	public class ApplicationUpdateSystem : MonoBehaviour
	{
		[SerializeField] private PopUpApplicationUpdate _popUp;
        private string _version;
        private UpdateUrgency _urgency;
        private string _linkToUpdate;

        private bool _alreadyShowed = false;

		private void OnEnable()
		{
			UserDataService.DataUpdated += CheckForUpdates;
		}

		private void OnDisable()
		{
			UserDataService.DataUpdated -= CheckForUpdates;
		}

		private void CheckForUpdates()
		{
			if (_alreadyShowed)
			{
				return;
			}
			
			var platformSettings = IGSUserData.PlatformSettings;
			
            if (IDosGamesSDKSettings.Instance.BuildForPlatform == Platforms.GooglePlay)
            {
				_version = platformSettings.GooglePlay.ApplicationUpdate.Version;
                _urgency = platformSettings.GooglePlay.ApplicationUpdate.Urgency;
				_linkToUpdate = platformSettings.GooglePlay.ApplicationUpdate.Link;
            }
			else if (IDosGamesSDKSettings.Instance.BuildForPlatform == Platforms.AppleAppStore)
			{
                _version = platformSettings.AppleAppStore.ApplicationUpdate.Version;
                _urgency = platformSettings.AppleAppStore.ApplicationUpdate.Urgency;
                _linkToUpdate = platformSettings.AppleAppStore.ApplicationUpdate.Link;
            }
			
			if ($"{_version}" == $"{Application.version}")
			{
				return;
			}

			if (_urgency == UpdateUrgency.NoUpdates)
			{
				return;
			}

			_popUp.Set(_urgency, $"{_version}", $"{_linkToUpdate}");
			_popUp.gameObject.SetActive(true);

			_alreadyShowed = true;
		}
	}
}