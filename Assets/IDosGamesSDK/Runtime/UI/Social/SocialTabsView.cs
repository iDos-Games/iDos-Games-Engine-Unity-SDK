// File: Assets/IDosGamesSDK/Runtime/UI/Social/SocialTabsView.cs
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IDosGames.UI.Social
{
    public class SocialTabsView : MonoBehaviour
    {
        [Header("Tab Buttons")]
        [SerializeField] private Button _friendsTabButton;
        [SerializeField] private Button _requestsTabButton;
        [SerializeField] private Button _recommendedTabButton;
        [SerializeField] private Button _timelineTabButton;

        [Header("Tab Button Texts")]
        [SerializeField] private TMP_Text _friendsTabText;
        [SerializeField] private TMP_Text _requestsTabText;
        [SerializeField] private TMP_Text _recommendedTabText;
        [SerializeField] private TMP_Text _timelineTabText;

        [Header("Visual")]
        [SerializeField] private Color _activeTabColor = new Color(1f, 1f, 1f, 1f);
        [SerializeField] private Color _inactiveTabColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        [SerializeField] private Color _activeTextColor = Color.white;
        [SerializeField] private Color _inactiveTextColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        public event Action<SocialTab> OnTabSelected;

        private Button[] _allButtons;
        private TMP_Text[] _allTexts;
        private Image[] _allImages;

        private void Awake()
        {
            _allButtons = new[] { _friendsTabButton, _requestsTabButton, _recommendedTabButton, _timelineTabButton };
            _allTexts = new[] { _friendsTabText, _requestsTabText, _recommendedTabText, _timelineTabText };
            _allImages = new Image[_allButtons.Length];
            for (int i = 0; i < _allButtons.Length; i++)
            {
                _allImages[i] = _allButtons[i]?.GetComponent<Image>();
            }
        }

        private void OnEnable()
        {
            _friendsTabButton.onClick.AddListener(() => SelectTab(SocialTab.Friends));
            _requestsTabButton.onClick.AddListener(() => SelectTab(SocialTab.Requests));
            _recommendedTabButton.onClick.AddListener(() => SelectTab(SocialTab.Recommended));
            _timelineTabButton.onClick.AddListener(() => SelectTab(SocialTab.Timeline));
        }

        private void OnDisable()
        {
            _friendsTabButton.onClick.RemoveAllListeners();
            _requestsTabButton.onClick.RemoveAllListeners();
            _recommendedTabButton.onClick.RemoveAllListeners();
            _timelineTabButton.onClick.RemoveAllListeners();
        }

        public void SelectTab(SocialTab tab)
        {
            int activeIndex = (int)tab;

            for (int i = 0; i < _allButtons.Length; i++)
            {
                bool isActive = i == activeIndex;

                if (_allImages[i] != null)
                    _allImages[i].color = isActive ? _activeTabColor : _inactiveTabColor;

                if (_allTexts[i] != null)
                    _allTexts[i].color = isActive ? _activeTextColor : _inactiveTextColor;
            }

            OnTabSelected?.Invoke(tab);
        }
    }

    public enum SocialTab
    {
        Friends = 0,
        Requests = 1,
        Recommended = 2,
        Timeline = 3
    }
}
