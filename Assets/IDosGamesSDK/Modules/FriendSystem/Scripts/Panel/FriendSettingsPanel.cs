using TMPro;
using UnityEngine;

namespace IDosGames.Friends
{
    public class FriendSettingsPanel : MonoBehaviour
    {
        [SerializeField] private RecommendedFriendPanel _recommendedPanel;
        public RecommendedFriendPanel RecommendedPanel => _recommendedPanel;
        [SerializeField] private TMP_Text _recommendedTitle;


        [SerializeField] private FriendRequestPanel _friendRequestPanel;
        public FriendRequestPanel FriendRequestPanel => _friendRequestPanel;
        [SerializeField] protected TMP_Text _friendRequestTitle;

        [SerializeField] private TabItem _recommendedTab;
        [SerializeField] private TabItem _friendRequestTab;

        private void OnEnable()
        {
            OpenFriendRequestPanel();
        }

        public void OpenFriendRequestPanel()
        {
            _recommendedPanel.gameObject.SetActive(false);
            _friendRequestPanel.gameObject.SetActive(true);
            _friendRequestTitle.gameObject.SetActive(true);
            _recommendedTitle.gameObject.SetActive(false);

            _recommendedTab.Deselect();
            _friendRequestTab.Select();
        }

        public void OpenRecommendedPanel()
        {

            _friendRequestPanel.gameObject.SetActive(false);
            _recommendedPanel.gameObject.SetActive(true);
            _friendRequestTitle.gameObject.SetActive(false);
            _recommendedTitle.gameObject.SetActive(true);

            _recommendedTab.Select();
            _friendRequestTab.Deselect();
        }

        public void Refresh()
        {
            if (_friendRequestPanel.gameObject.activeSelf)
            {
                _friendRequestPanel.Refresh();
            }
            else if (_recommendedPanel.gameObject.activeSelf)
            {
                _recommendedPanel.Refresh();
            }
        }
    }
}