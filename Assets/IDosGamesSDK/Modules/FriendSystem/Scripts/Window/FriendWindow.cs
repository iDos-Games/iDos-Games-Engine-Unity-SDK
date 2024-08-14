using System;
using UnityEngine;

namespace IDosGames.Friends
{
    public class FriendWindow : MonoBehaviour
    {
        [SerializeField] private FriendSettingsPanel _frienSettingsPanel;
        [SerializeField] private UserFriendsPanel _userFriendsPanel;

        private void OnEnable()
        {
            FriendRequestPanel.FriendAdded += OnFriendAdded;
        }
        private void OnDisable()
        {
            FriendRequestPanel.FriendAdded -= OnFriendAdded;
        }

        public void OpenUserFriendPanel(Action<string> action)
        {
            _userFriendsPanel.Initialize(action);
        }


        private void OnFriendAdded()
        {
            _userFriendsPanel.IsNeedUpdate = true;
            if (_userFriendsPanel.gameObject.activeSelf)
            {
                _userFriendsPanel.Refresh();
            }

        }

        public void CloseWindow()
        {
            if (_frienSettingsPanel.gameObject.activeSelf)
            {
                _frienSettingsPanel.gameObject.SetActive(false);
            }
            gameObject.SetActive(false);
        }
    }
}