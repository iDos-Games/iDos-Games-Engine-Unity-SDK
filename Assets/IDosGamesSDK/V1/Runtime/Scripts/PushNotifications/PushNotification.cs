using UnityEngine;

namespace IDosGames
{
    public class PushNotification : MonoBehaviour
    {

        void Start()
        {
            IDosGames.PushNotifications.API.Initialize();
        }

        private void OnApplicationFocus(bool focus)
        {
            if (focus == false)
            {
                //IDosGames.PushNotifications.API.SendNotification(LocalizationSystem.GetTranslation("PUSH_NOTIFICATION_1_1"), LocalizationSystem.GetTranslation("PUSH_NOTIFICATION_1_2"), new System.TimeSpan(0, 5, 0));
                IDosGames.PushNotifications.API.SendRepeatNotification("Winner, the Event will be over soon!", "Be sure to pick up all the rewards!", new System.TimeSpan(1, 0, 0), new System.TimeSpan(24, 0, 0), "icon_0", "icon_1", "");
            }
            else
            {
                IDosGames.PushNotifications.API.CancelAllNotifications();
            }
        }
    }
}