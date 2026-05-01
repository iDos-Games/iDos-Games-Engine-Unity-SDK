using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
    [RequireComponent(typeof(Button))]
    public class LeaderboardButton : MonoBehaviour
    {
        [SerializeField] private LeaderboardWindow _window;
        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            ResetListener();
        }

        private void OnEnable()
        {
            IDosGamesData.Config.OnTitlePublicConfigurationUpdated += SetEnable;
        }

        private void OnDisable()
        {
            IDosGamesData.Config.OnTitlePublicConfigurationUpdated -= SetEnable;
        }

        private void ResetListener()
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OpenLeaderboard);
        }

        private void OpenLeaderboard()
        {
            _window.gameObject.SetActive(true);
        }

        private void SetEnable()
        {
            gameObject.SetActive(GetEnableState());
        }

        private bool GetEnableState()
        {
            var systemState = IDosGamesData.Config.TitlePublicConfiguration?.SystemState;
            if (systemState == null) return true;
            return systemState.Leaderboards;
        }
    }
}
