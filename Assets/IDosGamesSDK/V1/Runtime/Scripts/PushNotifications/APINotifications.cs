using UnityEngine.Events;

namespace IDosGames.PushNotifications
{
    public static class API
    {
        public static void Initialize()
        {
            PushNotificationsManager.Instance.Initialize();
        }

        public static void SendNotification(string title, string text, System.TimeSpan timeDelayFromNow, string smallIcon = null, string largeIcon = null, string customData = "")
        {
            PushNotificationsManager.Instance.SendNotification(title, text, timeDelayFromNow, smallIcon, largeIcon, customData, null, null, default, default);
        }

        public static void SendRepeatNotification(string title, string text, System.TimeSpan timeDelayFromNow, System.TimeSpan? repeatInterval, string smallIcon = null, string largeIcon = null, string customData = "")
        {
            PushNotificationsManager.Instance.SendNotification(title, text, timeDelayFromNow, smallIcon, largeIcon, customData, repeatInterval, null, default, default);
        }

        public static void SendBigPictureNotification(string title, string text, string summaryText, System.TimeSpan timeDelayFromNow, string bigPicturePath, bool showWhenColapsed, string smallIcon = null, string largeIcon = null, string customData = "")
        {
            PushNotificationsManager.Instance.SendNotification(title, text, timeDelayFromNow, smallIcon, largeIcon, customData, null, bigPicturePath, showWhenColapsed, summaryText);
        }

        public static void SendRepeatBigPictureNotification(string title, string text, string summaryText, System.TimeSpan timeDelayFromNow, System.TimeSpan? repeatInterval, string bigPicturePath, bool showWhenColapsed, string smallIcon = null, string largeIcon = null, string customData = "")
        {
            PushNotificationsManager.Instance.SendNotification(title, text, timeDelayFromNow, smallIcon, largeIcon, customData, repeatInterval, bigPicturePath, showWhenColapsed, summaryText);
        }

        public static string AppWasOpenFromNotification()
        {
            return PushNotificationsManager.Instance.AppWasOpenFromNotification();
        }

        public static void RequestPermision(UnityAction<bool,string> completeMethod)
        {
            PushNotificationsManager.Instance.RequestNotificationPermision(completeMethod);
        }

        public static bool IsPermissionGranted()
        {
            return PushNotificationsManager.Instance.IsPermissionGranted();
        }

        public static void CopyBigPictureToDevice(string imageName)
        {
            PushNotificationsManager.Instance.CopyImage(imageName);
        }

        public static void CancelAllNotifications()
        {
            PushNotificationsManager.Instance.CancelAllNotifications();
        }
    }
}
