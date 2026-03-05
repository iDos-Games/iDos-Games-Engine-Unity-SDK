// File: Assets/IDosGamesSDK/Runtime/UI/Social/SocialScreen.cs
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames.UI.Social
{
    /// <summary>
    /// Main controller for the Social screen.
    /// Manages tab switching and panel lifecycle.
    /// </summary>
    public class SocialScreen : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _refreshButton;

        [Header("Tabs")]
        [SerializeField] private SocialTabsView _tabsView;

        [Header("Panels")]
        [SerializeField] private SocialFriendsPanel _friendsPanel;
        [SerializeField] private SocialRequestsPanel _requestsPanel;
        [SerializeField] private SocialRecommendedPanel _recommendedPanel;
        [SerializeField] private SocialTimelinePanel _timelinePanel;

        private SocialTab _currentTab = SocialTab.Friends;
        private bool _isActive;
        private bool _isRefreshing;

        private void OnEnable()
        {
            _isActive = true;

            _tabsView.OnTabSelected += OnTabSelected;
            if (_closeButton != null) _closeButton.onClick.AddListener(Close);
            if (_refreshButton != null) _refreshButton.onClick.AddListener(RefreshCurrentTab);

            // Open default tab
            _tabsView.SelectTab(SocialTab.Friends);
        }

        private void OnDisable()
        {
            _isActive = false;

            _tabsView.OnTabSelected -= OnTabSelected;
            if (_closeButton != null) _closeButton.onClick.RemoveAllListeners();
            if (_refreshButton != null) _refreshButton.onClick.RemoveAllListeners();
        }

        private void OnTabSelected(SocialTab tab)
        {
            _currentTab = tab;

            // Show/hide panels
            _friendsPanel.gameObject.SetActive(tab == SocialTab.Friends);
            _requestsPanel.gameObject.SetActive(tab == SocialTab.Requests);
            _recommendedPanel.gameObject.SetActive(tab == SocialTab.Recommended);
            _timelinePanel.gameObject.SetActive(tab == SocialTab.Timeline);

            // Auto-refresh on tab switch
            RefreshCurrentTab();
        }

        public void RefreshCurrentTab()
        {
            if (_isRefreshing) return;

            switch (_currentTab)
            {
                case SocialTab.Friends:
                    _friendsPanel.Refresh();
                    break;
                case SocialTab.Requests:
                    _requestsPanel.Refresh();
                    break;
                case SocialTab.Recommended:
                    _recommendedPanel.Refresh();
                    break;
                case SocialTab.Timeline:
                    _timelinePanel.Refresh();
                    break;
            }
        }

        /// <summary>
        /// Call this to open the Social screen.
        /// </summary>
        public void Open()
        {
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}
