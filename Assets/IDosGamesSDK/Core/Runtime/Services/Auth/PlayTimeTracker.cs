using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace IDosGames
{
    public class PlayTimeTracker : MonoBehaviour
    {
        private const float FLUSH_INTERVAL_SECONDS = 60f;
        private const string PENDING_KEY_PREFIX = "IDG_PendingUsageSeconds_";

        private static PlayTimeTracker _instance;

        private string _pendingKey;
        private float _pendingSeconds;
        private bool _isActive;
        private bool _isFlushing;
        private bool _isFocused = true;
        private bool _isPaused;
        private Coroutine _flushLoop;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null) return;

            var go = new GameObject("[IDG] PlayTimeTracker");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<PlayTimeTracker>();
        }

        private void OnEnable()
        {
            _pendingKey = PENDING_KEY_PREFIX + IDosGamesSDKSettings.Instance.TitleID;

            AuthenticationService.OnLoggedIn += HandleLoggedIn;
            AuthenticationService.OnLoggedOut += HandleLoggedOut;

            if (AuthenticationService.IsLoggedIn) HandleLoggedIn();
        }

        private void OnDisable()
        {
            AuthenticationService.OnLoggedIn -= HandleLoggedIn;
            AuthenticationService.OnLoggedOut -= HandleLoggedOut;

            StopFlushLoop();
            _isActive = false;
            _ = FlushAsync();
        }

        private void Update()
        {
            if (!_isActive) return;

            _pendingSeconds += Time.unscaledDeltaTime;
            PersistPending();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (_isFocused == hasFocus) return;
            _isFocused = hasFocus;
            RecomputeActive(flushOnSuspend: !hasFocus);
        }

        private void OnApplicationPause(bool isPaused)
        {
            if (_isPaused == isPaused) return;
            _isPaused = isPaused;
            RecomputeActive(flushOnSuspend: isPaused);
        }

        private void OnApplicationQuit()
        {
            _isActive = false;
            PersistPending();
            PlayerPrefs.Save();
            _ = FlushAsync();
        }

        private void HandleLoggedIn()
        {
            int leftover = PlayerPrefs.GetInt(_pendingKey, 0);
            if (leftover > 0 && _pendingSeconds < leftover) _pendingSeconds = leftover;

            _isActive = _isFocused && !_isPaused;

            StopFlushLoop();
            _flushLoop = StartCoroutine(FlushLoopCoroutine());

            if (_pendingSeconds >= 1f) _ = FlushAsync();
        }

        private void HandleLoggedOut()
        {
            _isActive = false;
            StopFlushLoop();
            _ = FlushAsync();
        }

        private void RecomputeActive(bool flushOnSuspend)
        {
            bool shouldBeActive = AuthenticationService.IsLoggedIn && _isFocused && !_isPaused;
            _isActive = shouldBeActive;

            if (flushOnSuspend && AuthenticationService.IsLoggedIn) _ = FlushAsync();
        }

        private IEnumerator FlushLoopCoroutine()
        {
            var wait = new WaitForSecondsRealtime(FLUSH_INTERVAL_SECONDS);
            while (true)
            {
                yield return wait;
                _ = FlushAsync();
            }
        }

        private void StopFlushLoop()
        {
            if (_flushLoop != null)
            {
                StopCoroutine(_flushLoop);
                _flushLoop = null;
            }
        }

        private async Task FlushAsync()
        {
            if (_isFlushing) return;
            if (!AuthenticationService.IsLoggedIn) return;

            int whole = Mathf.FloorToInt(_pendingSeconds);
            if (whole <= 0) return;

            _isFlushing = true;
            try
            {
                _pendingSeconds -= whole;
                PersistPending();
                PlayerPrefs.Save();

                var result = await UserService.AddUsageTime(whole);
                if (!result.Success)
                {
                    _pendingSeconds += whole;
                    PersistPending();
                    PlayerPrefs.Save();
                }
            }
            finally
            {
                _isFlushing = false;
            }
        }

        private void PersistPending()
        {
            PlayerPrefs.SetInt(_pendingKey, Mathf.FloorToInt(_pendingSeconds));
        }
    }
}
