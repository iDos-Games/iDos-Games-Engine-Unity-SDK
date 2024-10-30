using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using UnityEngine;

namespace IDosGames
{
	public class SubscriptionTrialObjects : MonoBehaviour
	{
		[SerializeField] private GameObject[] _onFreeTrial;
		[SerializeField] private GameObject[] _onNotFreeTrial;

		private void Start()
		{
			bool freeEnabled = GetFreeTrialState();

			_onFreeTrial.ToList().ForEach(x => x.SetActive(freeEnabled));
			_onNotFreeTrial.ToList().ForEach(x => x.SetActive(!freeEnabled));
		}

		private bool GetFreeTrialState()
		{
			bool enabled = false;

			var titleData = UserDataService.GetCachedTitlePublicConfig(TitleDataKey.SystemState);

			if (titleData == string.Empty)
			{
				return enabled;
			}

			var systemStateData = JsonConvert.DeserializeObject<JObject>(titleData);

			var freeTrialState = systemStateData[JsonProperty.VIP_FREE_TRIAL];

			if (freeTrialState == null)
			{
				return enabled;
			}

#if UNITY_IOS
			freeTrialState = freeTrialState[JsonProperty.IOS];
#elif UNITY_ANDROID
			freeTrialState = freeTrialState[JsonProperty.ANDROID];
#endif

			enabled = $"{freeTrialState}" == JsonProperty.ENABLED_VALUE;

			return enabled;
		}
	}
}