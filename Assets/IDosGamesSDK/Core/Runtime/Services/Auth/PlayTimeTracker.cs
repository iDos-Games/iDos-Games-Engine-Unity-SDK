using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace IDosGames
{
    public class PlayTimeTracker : MonoBehaviour
    {
        private const float FLUSH_INTERVAL_SECONDS = 60f;

        // Idle time threshold for session boundaries. Matches the recommendation from
        // DailyUsageRecord.Sessions on the server: 5 minutes of inactivity =
        // the next session is considered a new session.
        private const int SESSION_IDLE_THRESHOLD_SECONDS = 300;

        private const string PENDING_KEY_PREFIX = "IDG_PendingUsageSeconds_";
        private const string CURRENT_SESSION_KEY_PREFIX = "IDG_CurrentSessionSeconds_";
        private const string CLOSED_SESSION_KEY_PREFIX = "IDG_PendingClosedSession_";
        private const string NEW_SESSION_FLAG_KEY_PREFIX = "IDG_NewSessionPending_";
        private const string LAST_ACTIVITY_KEY_PREFIX = "IDG_LastActivityAt_";

        private static PlayTimeTracker _instance;

        private string _pendingKey;
        private string _currentSessionKey;
        private string _closedSessionKey;
        private string _newSessionFlagKey;
        private string _lastActivityKey;

        // An unsynchronized active time buffer waiting to be sent.
        private float _pendingSeconds;

        // The current open session (accumulated in parallel with _pendingSeconds —
        // Needed separately because it is reset at the session boundary, not on the flush).
        private float _currentSessionSeconds;

        // Closed session waiting to be sent in the Session Duration Seconds field.
        private int _pendingClosedSessionSeconds;

        // The "next flush - start of a new session" flag (after returning from idle time of ≥5 minutes).
        // This is NOT set on the login itself - the server's already
        // increments TotalSessions upon login.
        private bool _isNewSessionPending;

        private bool _isActive;
        private bool _isFlushing;
        private bool _isFocused = true;
        private bool _isPaused;
        private DateTime? _suspendedAt;
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
            var titleID = IDosGamesSDKSettings.Instance.TitleID;
            _pendingKey = PENDING_KEY_PREFIX + titleID;
            _currentSessionKey = CURRENT_SESSION_KEY_PREFIX + titleID;
            _closedSessionKey = CLOSED_SESSION_KEY_PREFIX + titleID;
            _newSessionFlagKey = NEW_SESSION_FLAG_KEY_PREFIX + titleID;
            _lastActivityKey = LAST_ACTIVITY_KEY_PREFIX + titleID;

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
            CloseCurrentSession(markNewSessionPending: false);
            _ = FlushAsync();
        }

        private void Update()
        {
            if (!_isActive) return;

            float dt = Time.unscaledDeltaTime;
            _pendingSeconds += dt;
            _currentSessionSeconds += dt;
            PersistState();
            PlayerPrefs.SetString(_lastActivityKey, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (_isFocused == hasFocus) return;
            _isFocused = hasFocus;
            HandleLifecycleChange(suspended: !hasFocus);
        }

        private void OnApplicationPause(bool isPaused)
        {
            if (_isPaused == isPaused) return;
            _isPaused = isPaused;
            HandleLifecycleChange(suspended: isPaused);
        }

        private void OnApplicationQuit()
        {
            _isActive = false;
            CloseCurrentSession(markNewSessionPending: false);
            PersistState();
            PlayerPrefs.Save();
            _ = FlushAsync();
        }

        private void HandleLoggedIn()
        {
            // Restore persisted state.
            _pendingSeconds = Mathf.Max(_pendingSeconds, PlayerPrefs.GetInt(_pendingKey, 0));
            _currentSessionSeconds = PlayerPrefs.GetInt(_currentSessionKey, 0);
            _pendingClosedSessionSeconds = PlayerPrefs.GetInt(_closedSessionKey, 0);
            _isNewSessionPending = PlayerPrefs.GetInt(_newSessionFlagKey, 0) != 0;

            // Cross-launch session boundary: if the previous launch left an
            // open session and ≥5 minutes have passed since the last activity,
            // close it retroactively. The duration will be stored in SessionDurationSeconds
            // on the next flash. Do NOT set IsNewSession=true: the server login
            // is already incrementing TotalSessions.
            if (_currentSessionSeconds > 0 && long.TryParse(PlayerPrefs.GetString(_lastActivityKey, "0"), out long lastActivityUnix) && lastActivityUnix > 0)
            {
                long nowUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                if (nowUnix - lastActivityUnix >= SESSION_IDLE_THRESHOLD_SECONDS)
                {
                    _pendingClosedSessionSeconds = Mathf.Max(_pendingClosedSessionSeconds, Mathf.FloorToInt(_currentSessionSeconds));
                    _currentSessionSeconds = 0;
                }
            }

            _isActive = _isFocused && !_isPaused;
            _suspendedAt = null;

            StopFlushLoop();
            _flushLoop = StartCoroutine(FlushLoopCoroutine());

            if (HasPendingPayload()) _ = FlushAsync();
        }

        private void HandleLoggedOut()
        {
            _isActive = false;
            StopFlushLoop();
            CloseCurrentSession(markNewSessionPending: false);
            _ = FlushAsync();
        }

        private void HandleLifecycleChange(bool suspended)
        {
            if (suspended)
            {
                _suspendedAt = DateTime.UtcNow;
                _isActive = false;
                if (AuthenticationService.IsLoggedIn) _ = FlushAsync();
                return;
            }

            // Return from background.
            if (_suspendedAt.HasValue)
            {
                var idleSeconds = (DateTime.UtcNow - _suspendedAt.Value).TotalSeconds;
                if (idleSeconds >= SESSION_IDLE_THRESHOLD_SECONDS)
                {
                    // The old session is closed due to idle time—flash its duration,
                    // mark the start of a new one.
                    CloseCurrentSession(markNewSessionPending: true);
                }
                _suspendedAt = null;
            }

            _isActive = AuthenticationService.IsLoggedIn && _isFocused && !_isPaused;
        }

        private void CloseCurrentSession(bool markNewSessionPending)
        {
            int duration = Mathf.FloorToInt(_currentSessionSeconds);
            if (duration > 0)
            {
                // We take the max, not the sum: SessionDurationSeconds is semantically
                // the duration of ONE specific session. If the buffer already contains a record from
                // the previous close, we keep the larger one.
                _pendingClosedSessionSeconds = Mathf.Max(_pendingClosedSessionSeconds, duration);
            }
            _currentSessionSeconds = 0;

            if (markNewSessionPending) _isNewSessionPending = true;

            PersistState();
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
            if (!HasPendingPayload()) return;

            int whole = Mathf.FloorToInt(_pendingSeconds);

            // The server requires UsageTime > 0. If the buffer is empty, wait for the next tick —
            // the IsNewSession / SessionDurationSeconds flags will be activated along with
            // the next accumulated second.
            if (whole <= 0) return;

            int closedSession = _pendingClosedSessionSeconds;
            bool isNewSession = _isNewSessionPending;

            _isFlushing = true;
            try
            {
                _pendingSeconds -= whole;
                _pendingClosedSessionSeconds = 0;
                _isNewSessionPending = false;
                PersistState();
                PlayerPrefs.Save();

                var result = await UserService.AddUsageTime(whole, isNewSession, closedSession);
                if (!result.Success)
                {
                    // Roll back the entire payload so that the next flush will repeat it.
                    _pendingSeconds += whole;
                    _pendingClosedSessionSeconds = Mathf.Max(_pendingClosedSessionSeconds, closedSession);
                    _isNewSessionPending = _isNewSessionPending || isNewSession;
                    PersistState();
                    PlayerPrefs.Save();
                }
            }
            finally
            {
                _isFlushing = false;
            }
        }

        private bool HasPendingPayload()
        {
            return _pendingSeconds >= 1f || _pendingClosedSessionSeconds > 0 || _isNewSessionPending;
        }

        private void PersistState()
        {
            PlayerPrefs.SetInt(_pendingKey, Mathf.FloorToInt(_pendingSeconds));
            PlayerPrefs.SetInt(_currentSessionKey, Mathf.FloorToInt(_currentSessionSeconds));
            PlayerPrefs.SetInt(_closedSessionKey, _pendingClosedSessionSeconds);
            PlayerPrefs.SetInt(_newSessionFlagKey, _isNewSessionPending ? 1 : 0);
        }
    }
}
