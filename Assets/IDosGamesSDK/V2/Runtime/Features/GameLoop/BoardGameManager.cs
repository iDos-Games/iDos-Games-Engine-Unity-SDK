using System;
using System.Collections;
using System.Threading.Tasks;
using IDosGames.ClientModels;
using IDosGames.TitlePublicConfiguration;
using UnityEngine;

namespace IDosGames
{
    public class BoardGameManager : MonoBehaviour
    {
        public static BoardGameManager Instance { get; private set; }

        public event Action OnDataUpdated;
        public event Action OnBoardReady;

        [Header("Board Mode")]
        [SerializeField] private bool use3DBoard = false;

        [Header("2D")]
        [SerializeField] private BoardHexRingRenderer boardHexRingRenderer;

        [Header("3D")]
        [SerializeField] private Transform playerToken;
        [SerializeField] private Transform tokenMesh;
        [SerializeField] private Transform[] tiles;
        [SerializeField] private float stepDuration = 0.3f;
        [SerializeField] private float hopHeight = 0.3f;
        [SerializeField] private int rewardsPopupDelayMs = 500;
        [SerializeField] private float tileSquashDownDuration = 0.06f;
        [SerializeField] private float tileSquashUpDuration = 0.25f;
        [SerializeField] private float tileSquashDepth = 0.025f;

        private bool _isRolling;
        private Coroutine _moveRoutine;

        public BoardLoopState BoardState => IDosGamesData.User.Board;
        public BoardLoopDefinition BoardDefinition => IDosGamesData.Config.TitlePublicConfiguration?.GameLoop?.Board;

        public int CurrentStageLevel => BoardState?.StageLevel ?? 0;
        public BoardStageDefinition CurrentStage { get; private set; }
        public BoardTemplateDefinition CurrentTemplate { get; private set; }

        public BoardRollResponse LastRollResponse { get; private set; }
        public AttackResponse LastAttackResponse { get; private set; }
        public RaidResponse LastRaidResponse { get; private set; }
        public BuildResponse LastBuildResponse { get; private set; }

        public bool IsBoardReady => BoardState != null && BoardDefinition != null && CurrentStage != null;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Subscribe();
        }

        private async void Start()
        {
            await LoadBoard();
        }

        private void OnDestroy()
        {
            Unsubscribe();
            if (Instance == this)
                Instance = null;
        }

        public async Task LoadBoard()
        {
            var stateResult = await GameLoopService.GetUserBoardState();

            if (!stateResult.Success)
            {
                Debug.LogError($"[GameLoopData] Failed to load board state: {stateResult.Error}");
                return;
            }

            var defResult = await GameLoopService.GetBoardDefinitionForLevel(CurrentStageLevel);

            if (!defResult.Success)
            {
                Debug.LogError($"[GameLoopData] Failed to load board definition for level {CurrentStageLevel}: {defResult.Error}");
                return;
            }

            ResolveCurrentStage();

            if (use3DBoard)
                SnapTokenToCurrentPosition();
            else
                boardHexRingRenderer?.RenderFromCurrentTemplate();

            OnBoardReady?.Invoke();
            CheckPendingInteraction();
        }

        // ─── 3D Token ────────────────────────────────────────────────

        private void SnapTokenToCurrentPosition()
        {
            if (BoardState == null || playerToken == null || tiles == null || tiles.Length == 0) return;
            playerToken.position = tiles[NormalizeIndex(BoardState.Position)].position;
        }

        public async void RollWithMultiplierFromUI(int multiplicator)
        {
            if (!use3DBoard) return;
            if (_isRolling) return;
            if (multiplicator <= 0) multiplicator = 1;
            if (playerToken == null || tiles == null || tiles.Length == 0) return;

            _isRolling = true;

            try
            {
                await PlayDiceHideAsync();

                var result = await GameLoopService.BoardLoopRoll(multiplicator);

                if (result?.Data == null)
                {
                    Debug.LogWarning("[GameLoopData] Roll result is null");
                    return;
                }

                await PlayDiceAnimationAsync(result.Data.Steps);

                await PlayRollAnimationAsync(result.Data);

                var response = result.Data;

                if (response.ActionRequired == "ATTACK")
                    AttackPanel.Instance.Show(response.ActionData);
                else if (response.ActionRequired == "RAID")
                    RaidPanel.Instance.Show(response.ActionData);
                else
                {
                    if (rewardsPopupDelayMs > 0)
                        await Task.Delay(rewardsPopupDelayMs);

                    if (response.GrantedRewards?.Count > 0)
                        Message.ShowRewards(response.GrantedRewards);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameLoopData] Roll failed: {ex}");
            }
            finally
            {
                _isRolling = false;
            }
        }

        private Task PlayRollAnimationAsync(BoardRollResponse response)
        {
            var tcs = new TaskCompletionSource<bool>();

            if (_moveRoutine != null)
                StopCoroutine(_moveRoutine);

            _moveRoutine = StartCoroutine(AnimateRoll(response, () => tcs.TrySetResult(true)));
            return tcs.Task;
        }

        private IEnumerator AnimateRoll(BoardRollResponse response, Action onComplete)
        {
            int totalSteps = Mathf.Max(0, response.Steps);
            int currentIndex = NormalizeIndex(response.OldPosition);

            playerToken.position = tiles[currentIndex].position;

            for (int i = 0; i < totalSteps; i++)
            {
                int nextIndex = NormalizeIndex(currentIndex + 1);
                bool isLastStep = (i == totalSteps - 1);
                yield return MoveToken(tiles[currentIndex].position, tiles[nextIndex].position, stepDuration, isLastStep);
                currentIndex = nextIndex;
            }

            int finalIndex = NormalizeIndex(response.NewPosition);
            playerToken.position = tiles[finalIndex].position;

            _moveRoutine = null;
            onComplete?.Invoke();
        }

        private IEnumerator MoveToken(Vector3 from, Vector3 to, float duration, bool isLastStep = false)
        {
            Transform toTile = GetTileAtPosition(to);
            Vector3 tileOrigin = to;

            float time = 0f;
            while (time < duration)
            {
                time += Time.deltaTime;
                float t = Mathf.Clamp01(time / duration);

                Vector3 pos = Vector3.Lerp(from, to, t);
                playerToken.position = pos;

                float jumpY = Mathf.Sin(t * Mathf.PI) * hopHeight;
                if (tokenMesh != null)
                {
                    tokenMesh.localPosition = new Vector3(0, jumpY, 0);
                }

                yield return null;
            }

            playerToken.position = to;
            if (tokenMesh != null)
            {
                tokenMesh.localPosition = Vector3.zero;
            }

            if (toTile != null)
            {
                if (isLastStep)
                    yield return SquashTile(toTile, tileOrigin, liftTokenWithTile: true);
                else
                    StartCoroutine(SquashTile(toTile, tileOrigin, liftTokenWithTile: false));
            }
        }

        private Transform GetTileAtPosition(Vector3 pos)
        {
            if (tiles == null) return null;
            foreach (var tile in tiles)
                if (tile != null && Vector3.Distance(tile.position, pos) < 0.01f)
                    return tile;
            return null;
        }

        private IEnumerator SquashTile(Transform tile, Vector3 origin, bool liftTokenWithTile = false)
        {
            Vector3 pressed = origin + Vector3.down * tileSquashDepth;

            // Быстро вниз
            float elapsed = 0f;
            while (elapsed < tileSquashDownDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / tileSquashDownDuration);
                tile.position = Vector3.Lerp(origin, pressed, t);
                if (liftTokenWithTile)
                    playerToken.position = tile.position;
                yield return null;
            }
            tile.position = pressed;
            if (liftTokenWithTile)
                playerToken.position = pressed;

            // Медленно вверх
            elapsed = 0f;
            while (elapsed < tileSquashUpDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / tileSquashUpDuration);
                tile.position = Vector3.Lerp(pressed, origin, t);
                if (liftTokenWithTile)
                    playerToken.position = tile.position;
                yield return null;
            }
            tile.position = origin;
            if (liftTokenWithTile)
                playerToken.position = origin;
        }

        private int NormalizeIndex(int index)
        {
            if (tiles == null || tiles.Length == 0) return 0;
            return ((index % tiles.Length) + tiles.Length) % tiles.Length;
        }

        // ─── Common ──────────────────────────────────────────────────

        private void CheckPendingInteraction()
        {
            var pending = BoardState?.Pending;
            if (pending == null) return;

            if (pending.ExpiresAtUtc < DateTime.UtcNow)
            {
                Debug.Log("[GameLoopData] Pending interaction expired, skipping.");
                return;
            }

            var actionData = new RollActionData
            {
                TargetUserID = pending.TargetUserID,
                PublicData = pending.TargetPublicData
            };

            if (pending.Type == "ATTACK")
            {
                Debug.Log("[GameLoopData] Restoring pending ATTACK");
                AttackPanel.Instance?.Show(actionData);
            }
            else if (pending.Type == "RAID" || pending.Type == "RAID_FINISHING")
            {
                Debug.Log("[GameLoopData] Restoring pending RAID");
                RaidPanel.Instance?.Show(actionData);
            }
        }

        private void ResolveCurrentStage()
        {
            CurrentStage = null;
            CurrentTemplate = null;

            if (BoardDefinition == null || BoardState == null)
                return;

            string levelKey = BoardState.StageLevel.ToString();

            if (BoardDefinition.StagesByLevel != null &&
                BoardDefinition.StagesByLevel.TryGetValue(levelKey, out var stage))
            {
                CurrentStage = stage;

                if (!string.IsNullOrEmpty(stage.BoardTemplateID) &&
                    BoardDefinition.BoardTemplatesByID != null &&
                    BoardDefinition.BoardTemplatesByID.TryGetValue(stage.BoardTemplateID, out var template))
                {
                    CurrentTemplate = template;
                }
            }
            else
            {
                Debug.LogWarning($"[GameLoopData] Stage not found for level: {levelKey}");
            }
        }

        public void Clear()
        {
            CurrentStage = null;
            CurrentTemplate = null;
            LastRollResponse = null;
            LastAttackResponse = null;
            LastRaidResponse = null;
            LastBuildResponse = null;
        }

        private void Subscribe()
        {
            IDosGamesData.User.OnBoardUpdated += OnBoardStateUpdated;
            IDosGamesData.Config.OnTitlePublicConfigurationUpdated += OnConfigUpdated;

            GameLoopService.OnBoardRollSuccess += SetRollResponse;
            GameLoopService.OnBoardAttackSuccess += SetAttackResponse;
            GameLoopService.OnBoardRaidSuccess += SetRaidResponse;
            GameLoopService.OnBoardBuildSuccess += SetBuildResponse;
        }

        private void Unsubscribe()
        {
            IDosGamesData.User.OnBoardUpdated -= OnBoardStateUpdated;
            IDosGamesData.Config.OnTitlePublicConfigurationUpdated -= OnConfigUpdated;

            GameLoopService.OnBoardRollSuccess -= SetRollResponse;
            GameLoopService.OnBoardAttackSuccess -= SetAttackResponse;
            GameLoopService.OnBoardRaidSuccess -= SetRaidResponse;
            GameLoopService.OnBoardBuildSuccess -= SetBuildResponse;
        }

        private void OnBoardStateUpdated()
        {
            ResolveCurrentStage();
            OnDataUpdated?.Invoke();
        }

        private void OnConfigUpdated()
        {
            ResolveCurrentStage();
            OnDataUpdated?.Invoke();
        }

        private void SetRollResponse(BoardRollResponse data)
        {
            LastRollResponse = data;
            OnDataUpdated?.Invoke();
        }

        private void SetAttackResponse(AttackResponse data)
        {
            LastAttackResponse = data;
            OnDataUpdated?.Invoke();
        }

        private void SetRaidResponse(RaidResponse data)
        {
            LastRaidResponse = data;
            OnDataUpdated?.Invoke();
        }

        private void SetBuildResponse(BuildResponse data)
        {
            LastBuildResponse = data;

            if (data != null)
            {
                if (data.StageComplete)
                {
                    if (data.CompletionReward != null && data.CompletionReward.Count > 0)
                        Message.ShowRewards(data.CompletionReward);

                    _ = LoadBoard();
                }
                else if (data.MaxLevelReward != null && data.MaxLevelReward.Count > 0)
                {
                    Message.ShowRewards(data.MaxLevelReward);
                }
            }

            OnDataUpdated?.Invoke();
        }

        private Task PlayDiceHideAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            if (DiceAnimator.Instance == null) { tcs.TrySetResult(true); return tcs.Task; }
            DiceAnimator.Instance.PlayHideBeforeRoll(() => tcs.TrySetResult(true));
            return tcs.Task;
        }

        private Task PlayDiceAnimationAsync(int steps)
        {
            var tcs = new TaskCompletionSource<bool>();
            if (DiceAnimator.Instance == null) { tcs.TrySetResult(true); return tcs.Task; }
            DiceAnimator.Instance.PlayDiceRoll(steps, () => tcs.TrySetResult(true));
            return tcs.Task;
        }
    }
}
