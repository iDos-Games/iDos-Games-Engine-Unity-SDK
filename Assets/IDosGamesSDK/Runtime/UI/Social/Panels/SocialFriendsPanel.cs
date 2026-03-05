// File: Assets/IDosGamesSDK/Runtime/UI/Social/Panels/SocialFriendsPanel.cs
using System.Collections.Generic;
using IDosGames.ClientModels;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames.UI.Social
{
    public class SocialFriendsPanel : MonoBehaviour
    {
        [Header("List")]
        [SerializeField] private Transform _listContent;
        [SerializeField] private FriendItemView _itemPrefab;
        [SerializeField] private ScrollRect _scrollRect;

        [Header("States")]
        [SerializeField] private LoadingView _loadingView;
        [SerializeField] private EmptyStateView _emptyView;

        private SimpleListPool<FriendItemView> _pool;
        private bool _isActive;

        private void Awake()
        {
            _pool = new SimpleListPool<FriendItemView>(_listContent, _itemPrefab);
            if (_itemPrefab != null) _itemPrefab.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            _isActive = true;
            SocialService.OnFriendsListUpdated += OnDataReceived;
            SocialService.OnRemoveFriendSuccess += OnFriendRemoved;
        }

        private void OnDisable()
        {
            _isActive = false;
            SocialService.OnFriendsListUpdated -= OnDataReceived;
            SocialService.OnRemoveFriendSuccess -= OnFriendRemoved;
        }

        public async void Refresh()
        {
            ShowLoading(true);
            var result = await SocialService.GetFriendsList();

            if (!_isActive) return;

            ShowLoading(false);

            if (!result.Success)
            {
                Message.Show(result.Error ?? "Failed to load friends");
                return;
            }

            // Data will arrive via OnFriendsListUpdated event
        }

        private void OnDataReceived(List<FriendPublicProfile> friends)
        {
            if (!_isActive) return;

            if (friends == null || friends.Count == 0)
            {
                _pool.Clear();
                _emptyView.Show("No friends yet. Add some!");
                return;
            }

            _emptyView.Hide();

            var items = _pool.Resize(friends.Count);
            for (int i = 0; i < friends.Count; i++)
            {
                items[i].Setup(friends[i], FriendItemMode.Friend, OnRemoveFriend);
            }

            if (_scrollRect != null) _scrollRect.verticalNormalizedPosition = 1f;
        }

        private async void OnRemoveFriend(string friendUserId)
        {
            var result = await SocialService.RemoveFriend(friendUserId);

            if (!_isActive) return;

            if (!result.Success)
            {
                Message.Show(result.Error ?? "Failed to remove friend");
                ResetAllBusy();
            }
            // Success handled by OnFriendRemoved event → triggers refresh
        }

        private void OnFriendRemoved(FriendActionResponse response)
        {
            if (!_isActive) return;
            // Refresh list after removal
            Refresh();
        }

        private void ResetAllBusy()
        {
            foreach (var item in _pool.ActiveItems)
            {
                if (item.gameObject.activeSelf) item.SetBusy(false);
            }
        }

        private void ShowLoading(bool show)
        {
            if (show)
            {
                _loadingView.Show();
                _emptyView.Hide();
            }
            else
            {
                _loadingView.Hide();
            }
        }
    }
}
