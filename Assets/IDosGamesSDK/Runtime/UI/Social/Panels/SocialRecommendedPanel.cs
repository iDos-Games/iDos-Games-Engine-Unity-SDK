// File: Assets/IDosGamesSDK/Runtime/UI/Social/Panels/SocialRecommendedPanel.cs
using System.Collections.Generic;
using IDosGames.ClientModels;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames.UI.Social
{
    public class SocialRecommendedPanel : MonoBehaviour
    {
        [Header("List")]
        [SerializeField] private Transform _listContent;
        [SerializeField] private FriendItemView _itemPrefab;
        [SerializeField] private ScrollRect _scrollRect;

        [Header("States")]
        [SerializeField] private LoadingView _loadingView;
        [SerializeField] private EmptyStateView _emptyView;

        [Header("Settings")]
        [SerializeField] private int _recommendLimit = 10;

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
            SocialService.OnRecommendedFriendsUpdated += OnDataReceived;
            SocialService.OnSendFriendRequestSuccess += OnRequestSent;
        }

        private void OnDisable()
        {
            _isActive = false;
            SocialService.OnRecommendedFriendsUpdated -= OnDataReceived;
            SocialService.OnSendFriendRequestSuccess -= OnRequestSent;
        }

        public async void Refresh()
        {
            ShowLoading(true);
            var result = await SocialService.GetRecommendedFriends(_recommendLimit);

            if (!_isActive) return;

            ShowLoading(false);

            if (!result.Success)
            {
                Message.Show(result.Error ?? "Failed to load recommendations");
                return;
            }
        }

        private void OnDataReceived(List<FriendPublicProfile> recommended)
        {
            if (!_isActive) return;

            if (recommended == null || recommended.Count == 0)
            {
                _pool.Clear();
                _emptyView.Show("No recommendations right now");
                return;
            }

            _emptyView.Hide();

            var items = _pool.Resize(recommended.Count);
            for (int i = 0; i < recommended.Count; i++)
            {
                items[i].Setup(recommended[i], FriendItemMode.Recommended, OnSendRequest);
            }

            if (_scrollRect != null) _scrollRect.verticalNormalizedPosition = 1f;
        }

        private async void OnSendRequest(string userId)
        {
            SetItemBusy(userId, true);

            var result = await SocialService.SendFriendRequest(userId);
            if (!_isActive) return;

            if (!result.Success)
            {
                Message.Show(result.Error ?? "Failed to send request");
                SetItemBusy(userId, false);
            }
        }

        private void OnRequestSent(FriendActionResponse response)
        {
            if (!_isActive) return;
            // Refresh to remove the sent user from recommendations
            Refresh();
        }

        private void SetItemBusy(string userId, bool busy)
        {
            foreach (var item in _pool.ActiveItems)
            {
                if (item.gameObject.activeSelf && item.UserId == userId)
                {
                    item.SetBusy(busy);
                    break;
                }
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
