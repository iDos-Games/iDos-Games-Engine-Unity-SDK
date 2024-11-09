using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	[RequireComponent(typeof(Button))]
	public class LeaderboardButton : MonoBehaviour
	{
		[SerializeField] private LeaderboardWindow _window;

		private Button _button;

		private void Awake()
		{
			_button = GetComponent<Button>();
			ResetListener();
		}

		private void OnEnable()
		{
			UserDataService.TitlePublicConfigurationUpdated += SetEnable;
		}

		private void OnDisable()
		{
			UserDataService.TitlePublicConfigurationUpdated -= SetEnable;
		}

		private void ResetListener()
		{
			_button.onClick.RemoveAllListeners();
			_button.onClick.AddListener(OpenLeaderboard);
		}

		private void OpenLeaderboard()
		{
			_window.gameObject.SetActive(true);
		}

		private void SetEnable()
		{
			gameObject.SetActive(GetEnableState());
		}

        private bool GetEnableState()
        {
            bool enabled = true;

            var titleData = UserDataService.GetCachedTitlePublicConfig(TitleDataKey.SystemState);

            if (string.IsNullOrEmpty(titleData))
            {
                return enabled;
            }

            JObject systemStateData = JsonConvert.DeserializeObject<JObject>(titleData);

            if (systemStateData.TryGetValue(JsonProperty.LEADERBOARDS, out JToken leaderboardStateToken) &&
                leaderboardStateToken.Type == JTokenType.Boolean)
            {
                enabled = leaderboardStateToken.ToObject<bool>();
            }

            return enabled;
        }
    }
}