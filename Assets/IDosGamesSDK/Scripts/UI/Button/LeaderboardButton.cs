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
			UserDataService.TitleDataUpdated += SetEnable;
		}

		private void OnDisable()
		{
			UserDataService.TitleDataUpdated -= SetEnable;
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

			var titleData = UserDataService.GetTitleData(TitleDataKey.system_state);

			if (titleData == string.Empty)
			{
				return enabled;
			}

			var systemStateData = JsonConvert.DeserializeObject<JObject>(titleData);

			string leaderboardState = $"{systemStateData[JsonProperty.LEADERBOARD]}";

			if (leaderboardState == string.Empty)
			{
				return enabled;
			}

			enabled = leaderboardState == JsonProperty.ENABLED_VALUE;

			return enabled;
		}
	}
}