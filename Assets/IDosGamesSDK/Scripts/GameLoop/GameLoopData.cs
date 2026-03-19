using System;
using System.Threading.Tasks;
using IDosGames.ClientModels;
using IDosGames.TitlePublicConfiguration;
using UnityEngine;

namespace IDosGames
{
    public class GameLoopData : MonoBehaviour
    {
        public static GameLoopData Instance { get; private set; }

        public event Action OnDataUpdated;
        public event Action OnBoardReady;

        // Сырые данные
        public GameLoopsDefinition GameLoops { get; private set; }
        public BoardLoopDefinition BoardDefinition { get; private set; }
        public BoardLoopState BoardState { get; private set; }

        // === Данные текущего уровня (быстрый доступ) ===
        public int CurrentStageLevel => BoardState?.StageLevel ?? 0;
        public BoardStageDefinition CurrentStage { get; private set; }
        public BoardTemplateDefinition CurrentTemplate { get; private set; }

        // Последние ответы
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
            //DontDestroyOnLoad(gameObject);
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

            int level = BoardState.StageLevel;
            //Debug.Log($"[GameLoopData] Board state loaded. StageLevel: {level}");

            var defResult = await GameLoopService.GetBoardDefinitionForLevel(level);

            if (!defResult.Success)
            {
                Debug.LogError($"[GameLoopData] Failed to load board definition for level {level}: {defResult.Error}");
                return;
            }

            ResolveCurrentStage();
            //Debug.Log($"[GameLoopData] Board ready. Stage: {CurrentStage?.Name}, Template: {CurrentTemplate != null}");
            OnBoardReady?.Invoke();
        }

        /// <summary>
        /// Из BoardDefinition вытаскивает текущий Stage и Template по StageLevel.
        /// </summary>
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
            GameLoops = null;
            BoardDefinition = null;
            BoardState = null;
            CurrentStage = null;
            CurrentTemplate = null;
            LastRollResponse = null;
            LastAttackResponse = null;
            LastRaidResponse = null;
            LastBuildResponse = null;
        }

        private void Subscribe()
        {
            GameLoopService.OnGameLoopsUpdated += SetGameLoops;
            GameLoopService.OnBoardDefinitionUpdated += SetBoardDefinition;
            GameLoopService.OnBoardStateUpdated += SetBoardState;
            GameLoopService.OnBoardRollSuccess += SetRollResponse;
            GameLoopService.OnBoardAttackSuccess += SetAttackResponse;
            GameLoopService.OnBoardRaidSuccess += SetRaidResponse;
            GameLoopService.OnBoardBuildSuccess += SetBuildResponse;
        }

        private void Unsubscribe()
        {
            GameLoopService.OnGameLoopsUpdated -= SetGameLoops;
            GameLoopService.OnBoardDefinitionUpdated -= SetBoardDefinition;
            GameLoopService.OnBoardStateUpdated -= SetBoardState;
            GameLoopService.OnBoardRollSuccess -= SetRollResponse;
            GameLoopService.OnBoardAttackSuccess -= SetAttackResponse;
            GameLoopService.OnBoardRaidSuccess -= SetRaidResponse;
            GameLoopService.OnBoardBuildSuccess -= SetBuildResponse;
        }

        private void SetGameLoops(GameLoopsDefinition data)
        {
            GameLoops = data;
            OnDataUpdated?.Invoke();
        }

        private void SetBoardDefinition(BoardLoopDefinition data)
        {
            BoardDefinition = data;
            ResolveCurrentStage();
            OnDataUpdated?.Invoke();
        }

        private void SetBoardState(BoardLoopState data)
        {
            BoardState = data;
            ResolveCurrentStage();
            OnDataUpdated?.Invoke();
        }

        private void SetRollResponse(BoardRollResponse data)
        {
            LastRollResponse = data;

            if (BoardState != null && data != null)
            {
                BoardState.Position = data.NewPosition;
                BoardState.CyclesCompleted += data.CyclesCompletedDelta;
                BoardState.LastRollAtUtc = DateTime.UtcNow;
            }

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

            if (BoardState != null && data != null && BoardState.BuildingStates != null)
            {
                var state = BoardState.BuildingStates.Find(x => x != null && x.SlotIndex == data.BuiltIndex);
                if (state != null)
                {
                    state.Level = data.NewLevel;
                    state.IsDamaged = false;

                    if (data.MaxLevelRewardClaimed)
                        state.MaxLevelRewardClaimed = true;
                }
            }

            OnDataUpdated?.Invoke();
        }
    }
}
