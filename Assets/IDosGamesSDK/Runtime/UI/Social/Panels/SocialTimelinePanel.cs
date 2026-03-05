// File: Assets/IDosGamesSDK/Runtime/UI/Social/Panels/SocialTimelinePanel.cs
using System.Collections.Generic;
using IDosGames.ClientModels;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames.UI.Social
{
    public class SocialTimelinePanel : MonoBehaviour
    {
        [Header("List")]
        [SerializeField] private Transform _listContent;
        [SerializeField] private TimelineItemView _itemPrefab;
        [SerializeField] private ScrollRect _scrollRect;

        [Header("States")]
        [SerializeField] private LoadingView _loadingView;
        [SerializeField] private EmptyStateView _emptyView;

        private SimpleListPool<TimelineItemView> _pool;
        private bool _isActive;

        private void Awake()
        {
            _pool = new SimpleListPool<TimelineItemView>(_listContent, _itemPrefab);
            if (_itemPrefab != null) _itemPrefab.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            _isActive = true;
            SocialService.OnTimelineUpdated += OnDataReceived;
        }

        private void OnDisable()
        {
            _isActive = false;
            SocialService.OnTimelineUpdated -= OnDataReceived;
        }

        public async void Refresh()
        {
            ShowLoading(true);
            var result = await SocialService.GetTimeline();

            if (!_isActive) return;

            ShowLoading(false);

            if (!result.Success)
            {
                Message.Show(result.Error ?? "Failed to load timeline");
                return;
            }
        }

        private void OnDataReceived(List<SocialTimelineEventDocument> events)
        {
            if (!_isActive) return;

            if (events == null || events.Count == 0)
            {
                _pool.Clear();
                _emptyView.Show("No activity yet");
                return;
            }

            _emptyView.Hide();

            var items = _pool.Resize(events.Count);
            for (int i = 0; i < events.Count; i++)
            {
                items[i].Setup(events[i]);
            }

            if (_scrollRect != null) _scrollRect.verticalNormalizedPosition = 1f;
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
