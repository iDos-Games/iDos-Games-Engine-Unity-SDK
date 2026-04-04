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

        [SerializeField] private BoardHexRingRenderer boardHexRingRenderer;

        // === Прямой доступ через единый центр данных ===
        public BoardLoopState BoardState => IDosGamesData.User.Board;
        public BoardLoopDefinition BoardDefinition => IDosGamesData.Config.TitlePublicConfiguration?.GameLoops?.Board;

        // === Данные текущего уровня ===
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
            // GameLoopService уже вызывает IDosGamesData.User.ApplyBoard() внутри

            if (!stateResult.Success)
            {
                Debug.LogError($"[GameLoopData] Failed to load board state: {stateResult.Error}");
                return;
            }

            var defResult = await GameLoopService.GetBoardDefinitionForLevel(CurrentStageLevel);
            // GameLoopService уже записывает определение в IDosGamesData.Config

            if (!defResult.Success)
            {
                Debug.LogError($"[GameLoopData] Failed to load board definition for level {CurrentStageLevel}: {defResult.Error}");
                return;
            }

            ResolveCurrentStage();

            boardHexRingRenderer?.RenderFromCurrentTemplate();

            OnBoardReady?.Invoke();
            CheckPendingInteraction();
        }

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
            // Сбрасываем только локальный производный стейт
            // Основные данные сбрасываются через IDosGamesData.User.Clear()
            CurrentStage = null;
            CurrentTemplate = null;
            LastRollResponse = null;
            LastAttackResponse = null;
            LastRaidResponse = null;
            LastBuildResponse = null;
        }

        private void Subscribe()
        {
            // Подписка на единый центр данных вместо дублирования через GameLoopService
            IDosGamesData.User.OnBoardUpdated += OnBoardStateUpdated;
            IDosGamesData.Config.OnTitlePublicConfigurationUpdated += OnConfigUpdated;

            // Только игровые ответы по-прежнему через GameLoopService
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
            // BoardDefinition мог обновиться в составе TitlePublicConfiguration
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
    }
}
