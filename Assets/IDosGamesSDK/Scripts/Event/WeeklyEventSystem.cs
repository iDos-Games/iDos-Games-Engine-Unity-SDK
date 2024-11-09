using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace IDosGames
{
    public class WeeklyEventSystem : MonoBehaviour
    {
        private static WeeklyEventSystem _instance;

        [SerializeField] private PopUpWeeklyEventRewards _popUpRewards;

        public static event Action DataUpdated;
        public static string EventType { get; private set; }
        public static DateTime EndDate { get; private set; }
        public static int PlayerPoints { get; private set; }
        public static IReadOnlyList<JToken> Rewards { get; private set; }
        public static JToken FollowingReward { get; private set; }
        public static JToken PreviousReward { get; private set; }
        public static float SliderValue { get; private set; }
        public static string SliderText { get; private set; }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
        }

        private void OnEnable()
        {
            UserDataService.CustomUserDataUpdated += SetData;
        }

        private void OnDisable()
        {
            UserDataService.CustomUserDataUpdated -= SetData;
        }

        public static void UpdateEventForPlayer()
        {
            _ = IGSClientAPI.ExecuteFunction(
             functionName: ServerFunctionHandlers.StartNewWeeklyEventForPlayer,
             resultCallback: (result) => _instance.OnResultUpdateEventForPlayer(result),
             notConnectionErrorCallback: (error) => _instance.OnErrorUpdateEventForPlayer(),
             connectionErrorCallback: UpdateEventForPlayer
             );
        }

        private void OnResultUpdateEventForPlayer(string result)
        {
            if (result == null)
            {
                _instance.OnErrorUpdateEventForPlayer();
            }
            else
            {
                if (IDosGamesSDKSettings.Instance.DebugLogging)
                {
                    Debug.Log("UpdateEventForPlayer: " + result);
                }
                
                JObject resultData = JsonConvert.DeserializeObject<JObject>(result.ToString());
                if (resultData.ContainsKey(JsonProperty.MESSAGE_KEY))
                {
                    var message = resultData[JsonProperty.MESSAGE_KEY].ToString();
                    //Message.Show(message);
                    if (message == "MESSAGE_CODE_SUCCESS" || message == "SUCCESS")
                    {
                        UserDataService.RequestUserAllData();
                    }

                }
            }

        }

        private void OnErrorUpdateEventForPlayer()
        {
            Message.Show(MessageCode.FAILED_TO_UPDATE_EVENT);
        }

        public static void AddEventPoints(int points)
        {
            if (points <= 0)
            {
                return;
            }

            FunctionParameters parameter = new()
            {
                Points = points
            };

            _ = IGSClientAPI.ExecuteFunction(
                 functionName: ServerFunctionHandlers.AddWeeklyEventPoints,
                resultCallback: (result) => _instance.OnSuccessAddEventPoints(points),
                notConnectionErrorCallback: (error) => _instance.OnErrorAddEventPoints(),
                connectionErrorCallback: () => AddEventPoints(points),
                functionParameter: parameter
                );
        }

        private void OnErrorAddEventPoints()
        {
            Message.Show(MessageCode.FAILED_TO_ADD_EVENT_POINTS);
        }

        private void OnSuccessAddEventPoints(int points)
        {
            PlayerPoints += points;

            SetRewards(EventType);
            SetSliderData();

            DataUpdated?.Invoke();
        }

        private void SetData()
        {
            var playerDataRaw = UserDataService.GetCachedCustomUserData(CustomUserDataKey.event_weekly);

            if (playerDataRaw == string.Empty)
            {
                UpdateEventForPlayer();
                return;
            }

            var playerData = JsonConvert.DeserializeObject<JToken>(playerDataRaw);

            var weeklyEventDataRaw = UserDataService.GetCachedTitlePublicConfig(TitleDataKey.EventWeekly);
            var weeklyEventData = JsonConvert.DeserializeObject<JToken>(weeklyEventDataRaw);

            EndDate = GetEndDateTime(weeklyEventData);
            SetPlayerPoints(playerData);

            if (IsNeedUpdateEvent(playerData))
            {
                if (PlayerPoints > 0)
                {
                    SetRewards($"{playerData[JsonProperty.TYPE]}");
                    _popUpRewards.ShowRewards(new(Rewards));
                }
                else
                {
                    UpdateEventForPlayer();
                }

                return;
            }

            EventType = $"{weeklyEventData[JsonProperty.TYPE]}";
            SetRewards(EventType);

            SetSliderData();

            DataUpdated?.Invoke();
        }

        private void SetPlayerPoints(JToken playerData)
        {
            int.TryParse($"{playerData[JsonProperty.POINTS]}", out int points);
            PlayerPoints = points;
        }

        private void SetRewards(string eventType)
        {
            var eventRewardsDataRaw = UserDataService.GetCachedTitlePublicConfig(TitleDataKey.EventWeeklyRewards);
            var weeklyEventRewardsData = JsonConvert.DeserializeObject<JToken>(eventRewardsDataRaw);

            var currentEvent = weeklyEventRewardsData.FirstOrDefault(x => $"{x[JsonProperty.TYPE]}" == eventType);

            Rewards = currentEvent[JsonProperty.REWARDS].ToList();

            var lastReward = Rewards.Last();
            var firstReward = Rewards.First();

            int maxPoints = int.Parse($"{lastReward[JsonProperty.POINTS]}");
            int minPoints = int.Parse($"{firstReward[JsonProperty.POINTS]}");

            if (PlayerPoints >= maxPoints)
            {
                FollowingReward = lastReward;
            }
            else
            {
                FollowingReward = Rewards.First(x => int.Parse($"{x[JsonProperty.POINTS]}") > PlayerPoints);
            }

            if (PlayerPoints < minPoints)
            {
                PreviousReward = firstReward;
            }
            else
            {
                PreviousReward = Rewards.Last(x => int.Parse($"{x[JsonProperty.POINTS]}") <= PlayerPoints);
            }
        }

        private void SetSliderData()
        {
            if (Rewards == null)
            {
                return;
            }

            int.TryParse($"{PreviousReward[JsonProperty.POINTS]}", out int previousPoints);
            int.TryParse($"{FollowingReward[JsonProperty.POINTS]}", out int followingPoints);

            SliderValue = 1;
            SliderText = "completed!";

            if (PlayerPoints <= 0)
            {
                SliderValue = 0;
                SliderText = $"{PlayerPoints}/{followingPoints}";
            }
            else if (PlayerPoints < followingPoints)
            {
                float deltaPlayerPoint = PlayerPoints - previousPoints;
                float deltaRewardsPoint = followingPoints - previousPoints;

                SliderValue = deltaPlayerPoint / deltaRewardsPoint;
                SliderText = $"{deltaPlayerPoint}/{deltaRewardsPoint}";
            }
        }

        private DateTime GetEndDateTime(JToken jToken)
        {
            string endDateString = $"{jToken[JsonProperty.END_DATE]}";

            return DateTime.Parse(endDateString, null, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
        }

        private bool IsNeedUpdateEvent(JToken playerData)
        {
            var playerEndDate = GetEndDateTime(playerData);

            return EndDate > playerEndDate;
        }
    }
}
