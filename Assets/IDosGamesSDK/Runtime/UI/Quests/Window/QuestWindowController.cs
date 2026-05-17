using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace IDosGames.UI.Quest
{
    [RequireComponent(typeof(QuestWindowView))]
    public class QuestWindowController : MonoBehaviour
    {
        [SerializeField] private QuestWindowView _view;

        private QuestWindowModel _model;
        private bool _isLoading;

        // ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            _model = new QuestWindowModel();
            if (_view == null) _view = GetComponent<QuestWindowView>();
        }

        private void OnEnable()
        {
            IDosGamesData.User.OnQuestUpdated              += OnDataChanged;
            IDosGamesData.User.OnVirtualCurrencyUpdated    += OnDataChanged;
            IDosGamesData.Config.OnQuestDefinitionsUpdated += OnDataChanged;

            _view.BackButton?.onClick.AddListener(OnBackClicked);

            Render();
            LoadData();
        }

        private void OnDisable()
        {
            IDosGamesData.User.OnQuestUpdated              -= OnDataChanged;
            IDosGamesData.User.OnVirtualCurrencyUpdated    -= OnDataChanged;
            IDosGamesData.Config.OnQuestDefinitionsUpdated -= OnDataChanged;

            if (_view.BackButton != null)
                _view.BackButton.onClick.RemoveListener(OnBackClicked);
        }

        // ─────────────────────────────────────────────────────────────

        private async void LoadData()
        {
            if (_isLoading) return;
            _isLoading = true;

            var defsTask  = QuestService.GetQuestDefinitions();
            var stateTask = QuestService.GetUserQuestState(autoRefreshCycles: true);
            await Task.WhenAll(defsTask, stateTask);

            _isLoading = false;
            Render();
        }

        private void OnDataChanged() => Render();

        private void OnBackClicked() => gameObject.SetActive(false);

        // ─────────────────────────────────────────────────────────────

        private void Render()
        {
            UpdateModel();
            _view?.Render(
                _model,
                claimQuestFactory:     (questId, cycleId) => () => _ = ClaimQuest(questId, cycleId),
                claimMilestoneFactory: (cycleId, msId)    => ()     => ClaimMilestone(cycleId, msId)
            );
        }

        private void UpdateModel()
        {
            var currencies = IDosGamesData.User.State?.InventoryV2?.VirtualCurrencies;
            _model.CoinBalance = currencies?.GetValueOrDefault("Coins")?.Amount ?? 0;
            _model.GemBalance  = currencies?.GetValueOrDefault("Gems")?.Amount  ?? 0;

            var state  = IDosGamesData.User.State?.Quest ?? new UserQuestState();
            var config = IDosGamesData.Config.TitlePublicConfiguration?.Quest ?? new QuestDefinitions();
            _model.Apply(state, config);
        }

        // ─────────────────────────────────────────────────────────────

        private async Task ClaimQuest(string questId, string cycleId)
        {
            // Optimistic: mark claimed immediately, revert on failure
            IDosGamesData.User.PatchQuestStatus(questId, cycleId, QuestStatus.Claimed);

            var result = await QuestService.ClaimQuestReward(questId, cycleId);
            if (!result.Success)
            {
                IDosGamesData.User.PatchQuestStatus(questId, cycleId, QuestStatus.Completed);
                Debug.LogWarning($"[QuestWindowController] ClaimQuest failed: {result.Error}");
            }
        }

        private async Task ClaimMilestone(string cycleId, string milestoneId)
        {
            // Optimistic: mark milestone claimed immediately, revert on failure
            var currentCount = IDosGamesData.User.State?.Quest?.Cycles
                ?.GetValueOrDefault(cycleId)?.CompletedQuestsCount ?? 0;
            IDosGamesData.User.PatchMilestoneClaimed(cycleId, milestoneId, currentCount);

            var result = await QuestService.ClaimMilestoneReward(cycleId, milestoneId);
            if (!result.Success)
            {
                var cycle = IDosGamesData.User.State?.Quest?.Cycles?.GetValueOrDefault(cycleId);
                cycle?.ClaimedMilestoneIDs?.Remove(milestoneId);
                Render();
                Debug.LogWarning($"[QuestWindowController] ClaimMilestone failed: {result.Error}");
            }
        }
    }
}
