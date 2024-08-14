using IDosGames.Friends;
using UnityEngine;
using UnityEngine.UI;
namespace IDosGames.UserProfile
{

    [RequireComponent(typeof(Button))]
    public class FriendButton : MonoBehaviour
    {
        [SerializeField] private FriendWindow _friendWindow;
        [SerializeField] private UserProfileRoom _room;
        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
        }
        private void OnEnable()
        {
            _button.onClick.AddListener(OpenFriendWindow);
        }
        private void OnDisable()
        {
            _button.onClick.RemoveAllListeners();
        }

        private void OpenFriendWindow()
        {
            _friendWindow.gameObject.SetActive(true);
            _friendWindow.OpenUserFriendPanel(OpenUserProfile);
        }
        private void OpenUserProfile(string id)
        {
            _room.OpenRoom(id);
        }
    }
}