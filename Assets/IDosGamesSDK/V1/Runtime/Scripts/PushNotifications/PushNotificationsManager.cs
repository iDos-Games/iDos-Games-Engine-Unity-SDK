using UnityEngine;
using System;
using System.IO;
using System.Collections;

#if IDOSGAMES_NOTIFICATIONS_IOS && UNITY_IOS
using Unity.Notifications.iOS;
#endif

#if IDOSGAMES_NOTIFICATIONS_ANDROID && UNITY_ANDROID
using Unity.Notifications.Android;
#endif

namespace IDosGames
{
    public class PushNotificationsManager : MonoBehaviour
    {
        private static PushNotificationsManager instance;

#if IDOSGAMES_NOTIFICATIONS_IOS && UNITY_IOS
        AuthorizationRequest request;
#endif
#if IDOSGAMES_NOTIFICATIONS_ANDROID && UNITY_ANDROID
        const string channelID = "channel_id";
        private bool initialized;
        PermissionRequest request;
#endif

        internal static PushNotificationsManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject();
                    go.name = "PushNotificationsManager";
                    DontDestroyOnLoad(go);
                    instance = go.AddComponent<PushNotificationsManager>();
                }
                return instance;
            }
        }

        internal void Initialize()
        {
#if IDOSGAMES_NOTIFICATIONS_ANDROID && UNITY_ANDROID
            if (initialized == false)
            {
                initialized = true;
                var c = new AndroidNotificationChannel()
                {
                    Id = channelID,
                    Name = "Default Channel",
                    Importance = Importance.High,
                    Description = "Generic notifications",

                };
                AndroidNotificationCenter.RegisterNotificationChannel(c);
                RequestNotificationPermision(null);
            }
#endif
        }

        internal void CancelAllNotifications()
        {
#if IDOSGAMES_NOTIFICATIONS_ANDROID && UNITY_ANDROID
            AndroidNotificationCenter.CancelAllNotifications();
#endif
#if IDOSGAMES_NOTIFICATIONS_IOS && UNITY_IOS
            iOSNotificationCenter.RemoveAllScheduledNotifications();
            iOSNotificationCenter.RemoveAllDeliveredNotifications();
#endif
        }

        internal void SendNotification(string title, string text, TimeSpan timeDelayFromNow, string smallIcon, string largeIcon, string customData, TimeSpan? repeatInterval, string bigPicturePath, bool showWhenColapsed, string summaryText)
        {
#if IDOSGAMES_NOTIFICATIONS_ANDROID && UNITY_ANDROID
            var notification = new AndroidNotification();
            notification.Title = title;
            notification.Text = text;
            if (repeatInterval != null)
            {
                notification.RepeatInterval = repeatInterval;
            }
            if (smallIcon != null)
            {
                notification.SmallIcon = smallIcon;
            }
            if (largeIcon != null)
            {
                notification.LargeIcon = largeIcon;
            }
            if (customData != null)
            {
                notification.IntentData = customData;
            }

            if (bigPicturePath != null)
            {
                var style = new BigPictureStyle();
                style.Picture = bigPicturePath;
                if (largeIcon != null)
                {
                    style.LargeIcon = largeIcon;
                }
                style.ShowWhenCollapsed = showWhenColapsed;
                style.SummaryText = summaryText;
                notification.BigPicture = style;
            }

            notification.FireTime = DateTime.Now.Add(timeDelayFromNow);

            AndroidNotificationCenter.SendNotification(notification, channelID);
#endif

#if IDOSGAMES_NOTIFICATIONS_IOS && UNITY_IOS
            iOSNotificationTimeIntervalTrigger timeTrigger = new iOSNotificationTimeIntervalTrigger();

            if (repeatInterval == null)
            {
                timeTrigger.TimeInterval = timeDelayFromNow;
                timeTrigger.Repeats = false;
            }
            else
            {
                timeTrigger.TimeInterval = (TimeSpan)repeatInterval;
                timeTrigger.Repeats = true;
            }

            iOSNotification notification = new iOSNotification()
            {
                Title = title,
                Subtitle = "",
                Body = text,
                Data = customData,
                ShowInForeground = true,
                ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
                CategoryIdentifier = "category_a",
                ThreadIdentifier = "thread1",
                Trigger = timeTrigger,
            };

            iOSNotificationCenter.ScheduleNotification(notification);
#endif
        }

        internal string AppWasOpenFromNotification()
        {
#if IDOSGAMES_NOTIFICATIONS_ANDROID && UNITY_ANDROID
            var notificationIntentData = AndroidNotificationCenter.GetLastNotificationIntent();

            if (notificationIntentData != null)
            {
                return notificationIntentData.Notification.IntentData;
            }
            else
            {
                return null;
            }
#elif IDOSGAMES_NOTIFICATIONS_IOS && UNITY_IOS
            iOSNotification notificationIntentData = iOSNotificationCenter.GetLastRespondedNotification();

            if (notificationIntentData != null)
            {
                return notificationIntentData.Data;
            }
            else
            {
                return null;
            }
#else
            return null;
#endif
        }

        internal void RequestNotificationPermision(UnityEngine.Events.UnityAction<bool, string> completeMethod)
        {
            StartCoroutine(RequestNotificationPermission(completeMethod));
        }

        private IEnumerator RequestNotificationPermission(UnityEngine.Events.UnityAction<bool, string> completeMethod)
        {
#if IDOSGAMES_NOTIFICATIONS_ANDROID && UNITY_ANDROID
            request = new PermissionRequest();
            while (request.Status == PermissionStatus.RequestPending)
            {
                yield return null;
            }
            if (completeMethod != null)
            {
                completeMethod(request.Status == PermissionStatus.Allowed, request.Status.ToString());
            }
            
#elif IDOSGAMES_NOTIFICATIONS_IOS && UNITY_IOS
            var authorizationOption = AuthorizationOption.Alert | AuthorizationOption.Badge;
            using (var req = new AuthorizationRequest(authorizationOption, true))
            {
                while (!req.IsFinished)
                {
                    yield return null;
                };
                request = req;
                completeMethod(req.Granted,req.ToString());
            }
#else
            yield return null;
#endif
        }


        internal bool IsPermissionGranted()
        {
#if IDOSGAMES_NOTIFICATIONS_ANDROID && UNITY_ANDROID
            return (request.Status == PermissionStatus.Allowed);
#elif IDOSGAMES_NOTIFICATIONS_IOS && UNITY_IOS
            return request.Granted;
#else
            return false;
#endif
        }

        internal void CopyImage(string streamingAssetsPath)
        {
            StartCoroutine(ReadFromStreamingAssets(streamingAssetsPath));
        }

        private IEnumerator ReadFromStreamingAssets(string streamingAssetsPath)
        {
            string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, streamingAssetsPath);
            byte[] result;
            if (filePath.Contains("://") || filePath.Contains(":///"))
            {
                UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(filePath);
                yield return www.SendWebRequest();
                result = www.downloadHandler.data;
            }
            else
            {
                result = System.IO.File.ReadAllBytes(filePath);
            }
            File.WriteAllBytes(Path.Combine(Application.persistentDataPath, streamingAssetsPath), result);
        }
    }
}
