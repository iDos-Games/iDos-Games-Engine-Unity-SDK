using IDosGames.ServerModels;
using IDosGames.TitlePublicConfiguration;
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
            IDosGamesData.User.OnAnyUpdated += SetData;
            RewardService.OnClaimSuccess += OnRewardClaimSuccess;
        }

        private void OnDisable()
        {
            IDosGamesData.User.OnAnyUpdated -= SetData;
            RewardService.OnClaimSuccess -= OnRewardClaimSuccess;
        }

        private void OnRewardClaimSuccess(RewardResponse resp)
        {
            if (resp == null || resp.PointsAdded <= 0) return;

            ApplyWeeklyPointsLocal(resp.PointsAdded);
            TryPatchWeeklyPointsInCache(PlayerPoints);
        }

        private void ApplyWeeklyPointsLocal(int pointsAdded)
        {
            PlayerPoints += pointsAdded;

            if (string.IsNullOrEmpty(EventType) || Rewards == null)
            {
                SetData();
                if (string.IsNullOrEmpty(EventType) || Rewards == null) return;
            }

            SetRewards(EventType);
            SetSliderData();

            DataUpdated?.Invoke();
        }

        private void TryPatchWeeklyPointsInCache(int newPoints)
        {
            var cud = IDosGamesData.User.CustomUserData;
            if (cud?.Data == null) return;

            var key = CustomUserDataKey.event_weekly.ToString();

            if (!cud.Data.TryGetValue(key, out var record) || record == null) return;
            if (string.IsNullOrEmpty(record.Value)) return;

            JToken token;
            try
            {
                token = JsonConvert.DeserializeObject<JToken>(record.Value);
                if (token == null) return;
            }
            catch { return; }

            token[JsonProperty.POINTS] = newPoints;

            record.Value = token.ToString(Formatting.None);
            record.LastUpdated = DateTime.UtcNow;
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
                OnErrorUpdateEventForPlayer();
            }
            else
            {
                if (IDosGamesSDKSettings.Instance.DebugLogging)
                {
                    Debug.Log("UpdateEventForPlayer: " + result);
                }

                JObject resultData = JsonConvert.DeserializeObject<JObject>(result);
                if (resultData.ContainsKey(JsonProperty.MESSAGE_KEY))
                {
                    var message = resultData[JsonProperty.MESSAGE_KEY].ToString();
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
            if (points <= 0) return;

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
            var cud = IDosGamesData.User.CustomUserData;

            if (cud?.Data == null ||
                !cud.Data.TryGetValue(CustomUserDataKey.event_weekly.ToString(), out var record) ||
                record == null ||
                string.IsNullOrEmpty(record.Value))
            {
                UpdateEventForPlayer();
                return;
            }

            var playerData = JsonConvert.DeserializeObject<JToken>(record.Value);

            var weeklyEventData = IDosGamesData.Config.TitlePublicConfiguration.EventWeekly;

            if (weeklyEventData == null)
            {
                Debug.LogError("WeeklyEventSystem: EventWeekly config is null.");
                return;
            }

            EndDate = weeklyEventData.EndDate ?? default;
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

            EventType = weeklyEventData.Type.ToString();
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
            var allRewards = IDosGamesData.Config.TitlePublicConfiguration.EventWeeklyRewards;

            if (allRewards == null)
            {
                Debug.LogError("WeeklyEventSystem: EventWeeklyRewards config is null.");
                return;
            }

            Enum.TryParse(eventType, out TitlePublicConfiguration.EventType parsedType);
            var currentEvent = allRewards.FirstOrDefault(x => x.Type == parsedType);

            if (currentEvent == null)
            {
                Debug.LogError($"WeeklyEventSystem: No rewards found for event type '{eventType}'.");
                return;
            }

            // Конвертируем List<Reward> в List<JToken> чтобы не ломать внешние скрипты
            Rewards = currentEvent.Rewards
                .Select(r => JToken.FromObject(r))
                .ToList();

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
            if (Rewards == null) return;

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

        private bool IsNeedUpdateEvent(JToken playerData)
        {
            string endDateString = $"{playerData[JsonProperty.END_DATE]}";
            var playerEndDate = DateTime.Parse(endDateString, null,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);

            return EndDate > playerEndDate;
        }
    }
}
