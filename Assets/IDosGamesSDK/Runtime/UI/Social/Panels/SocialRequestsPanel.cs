// File: Assets/IDosGamesSDK/Runtime/UI/Social/Panels/SocialRequestsPanel.cs
using System.Collections.Generic;
using IDosGames.ClientModels;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames.UI.Social
{
    public class SocialRequestsPanel : MonoBehaviour
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
            SocialService.OnIncomingRequestsUpdated += OnDataReceived;
            SocialService.OnAcceptFriendRequestSuccess += OnActionSuccess;
            SocialService.OnDeclineFriendRequestSuccess += OnActionSuccess;
        }

        private void OnDisable()
        {
            _isActive = false;
            SocialService.OnIncomingRequestsUpdated -= OnDataReceived;
            SocialService.OnAcceptFriendRequestSuccess -= OnActionSuccess;
            SocialService.OnDeclineFriendRequestSuccess -= OnActionSuccess;
        }

        public async void Refresh()
        {
            ShowLoading(true);
            var result = await SocialService.GetIncomingRequests();

            if (!_isActive) return;

            ShowLoading(false);

            if (!result.Success)
            {
                Message.Show(result.Error ?? "Failed to load requests");
                return;
            }
        }

        private void OnDataReceived(List<FriendPublicProfile> requests)
        {
            if (!_isActive) return;

            if (requests == null || requests.Count == 0)
            {
                _pool.Clear();
                _emptyView.Show("No pending requests");
                return;
            }

            _emptyView.Hide();

            var items = _pool.Resize(requests.Count);
            for (int i = 0; i < requests.Count; i++)
            {
                items[i].Setup(
                    requests[i],
                    FriendItemMode.IncomingRequest,
                    OnAccept,
                    OnDecline
                );
            }

            if (_scrollRect != null) _scrollRect.verticalNormalizedPosition = 1f;
        }

        private async void OnAccept(string userId)
        {
            SetItemBusy(userId, true);

            var result = await SocialService.AcceptFriendRequest(userId);
            if (!_isActive) return;

            if (!result.Success)
            {
                Message.Show(result.Error ?? "Failed to accept request");
                SetItemBusy(userId, false);
            }
        }

        private async void OnDecline(string userId)
        {
            SetItemBusy(userId, true);

            var result = await SocialService.DeclineFriendRequest(userId);
            if (!_isActive) return;

            if (!result.Success)
            {
                Message.Show(result.Error ?? "Failed to decline request");
                SetItemBusy(userId, false);
            }
        }

        private void OnActionSuccess(FriendActionResponse response)
        {
            if (!_isActive) return;
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
