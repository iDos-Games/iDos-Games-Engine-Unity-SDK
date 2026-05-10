using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace IDosGames.UI.Quest
{
    public class QuestWindowView : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private TextMeshProUGUI _coinText;
        [SerializeField] private TextMeshProUGUI _gemText;

        [Header("Tabs")]
        [SerializeField] private QuestTabView    _tabTemplate;
        [SerializeField] private RectTransform   _tabContainer;

        [Header("Cycle Progress")]
        [SerializeField] private TextMeshProUGUI _progressText;
        [SerializeField] private TextMeshProUGUI _timerText;

        [Header("Quest List")]
        [SerializeField] private QuestItemView   _questItemTemplate;
        [SerializeField] private RectTransform   _questListContainer;

        [Header("Milestone")]
        [SerializeField] private QuestMilestoneItemView _milestoneTemplate;

        [Header("Premium")]
        [SerializeField] private Button     _activatePremiumButton;
        [SerializeField] private GameObject _premiumLockIcon;

        [Header("Navigation")]
        [SerializeField] private Button _backButton;

        public Button BackButton            => _backButton;
        public Button ActivatePremiumButton => _activatePremiumButton;

        /// <summary>CycleID активного таба; "" = постоянные квесты.</summary>
        public string ActiveTabId { get; private set; } = "Ежедневные";

        private QuestWindowModel                  _cachedModel;
        private Func<string, string, UnityAction> _cachedClaimQuestFactory;
        private Func<string, string, Func<Task>>  _cachedClaimMilestoneFactory;
        private readonly List<QuestTabView>        _spawnedTabs = new();

        // ─────────────────────────────────────────────────────────────

        private void Update() => RefreshTimer();

        // ─────────────────────────────────────────────────────────────

        public void Render(
            QuestWindowModel                  model,
            Func<string, string, UnityAction> claimQuestFactory,
            Func<string, string, Func<Task>>  claimMilestoneFactory)
        {
            _cachedModel                 = model;
            _cachedClaimQuestFactory     = claimQuestFactory;
            _cachedClaimMilestoneFactory = claimMilestoneFactory;

            SetupTabs(model);
            RenderUI(model);

            if (model.IsLoaded)
            {
                RenderQuestList(model, claimQuestFactory);
            }
        }

        public void RenderUI(QuestWindowModel model)
        {
            if (_coinText != null) _coinText.text = model.CoinBalance.ToString("N0");
            if (_gemText  != null) _gemText.text  = model.GemBalance.ToString("N0");

            if (_activatePremiumButton != null) _activatePremiumButton.gameObject.SetActive(true);
            if (_premiumLockIcon       != null) _premiumLockIcon.SetActive(false);

            RefreshTabVisuals(model);
        }

        // ─────────────────────────────────────────────────────────────

        public void SwitchTab(string tabId)
        {
            if (_cachedModel == null) return;
            ActiveTabId = tabId;
            RefreshTabVisuals(_cachedModel);
            RenderQuestList(_cachedModel, _cachedClaimQuestFactory);
        }

        // ─────────────────────────────────────────────────────────────

        private void SetupTabs(QuestWindowModel model)
        {
            if (_tabTemplate == null || _tabContainer == null) return;

            for (int i = _tabContainer.childCount - 1; i >= 0; i--)
            {
                var child = _tabContainer.GetChild(i);
                if (child == _tabTemplate.transform) continue;
                DestroyImmediate(child.gameObject);
            }
            _spawnedTabs.Clear();

            // One tab per cycle
            foreach (var cycle in model.AllCycles)
            {
                var tab        = Instantiate(_tabTemplate, _tabContainer);
                var capturedId = cycle.CycleID;
                tab.Setup(capturedId, capturedId, () => SwitchTab(capturedId));
                tab.gameObject.SetActive(true);
                _spawnedTabs.Add(tab);
            }

            // Permanent tab — always last
            var permTab = Instantiate(_tabTemplate, _tabContainer);
            permTab.Setup("", "Постоянные", () => SwitchTab(""));
            permTab.gameObject.SetActive(true);
            _spawnedTabs.Add(permTab);

            // If current selection is no longer valid, pick first tab
            if (!_spawnedTabs.Any(t => t.TabId == ActiveTabId))
                ActiveTabId = _spawnedTabs.Count > 0 ? _spawnedTabs[0].TabId : "";
        }

        private void RefreshTabVisuals(QuestWindowModel model)
        {
            foreach (var tab in _spawnedTabs)
            {
                tab.SetSelected(tab.TabId == ActiveTabId);

                bool hasCompleted;
                if (string.IsNullOrEmpty(tab.TabId))
                {
                    hasCompleted = model.PermanentQuests.Any(q => q.Status == QuestStatus.Completed);
                }
                else
                {
                    var tabCycle = model.AllCycles.FirstOrDefault(c => c.CycleID == tab.TabId);
                    bool hasClaimableQuest     = tabCycle?.Quests.Any(q => q.Status == QuestStatus.Completed) == true;
                    bool hasUnclaimedMilestone = tabCycle?.Milestones.Any(m => m.IsReached && !m.IsFreeClaimed) == true;
                    hasCompleted = hasClaimableQuest || hasUnclaimedMilestone;
                }

                tab.SetHasCompleted(hasCompleted);
            }

            string progressLabel;

            if (string.IsNullOrEmpty(ActiveTabId))
            {
                int total = model.PermanentQuests.Count;
                int done  = model.PermanentQuests.Count(q => q.Status != QuestStatus.Active);
                progressLabel = $"{done}/{total}";
            }
            else
            {
                var cycle     = GetActiveCycle(model);
                progressLabel = cycle?.ProgressText    ?? "0/0";
            }

            if (_progressText   != null) _progressText.text    = progressLabel;

            RefreshTimer();
        }

        private void RefreshTimer()
        {
            if (_timerText == null || _cachedModel == null) return;

            var cycle = GetActiveCycle(_cachedModel);
            if (cycle == null)
            {
                // Permanent quests have no expiry
                _timerText.text = "Never";
                return;
            }

            _timerText.text = FormatCountdown(cycle.EndUtc - DateTime.UtcNow);
        }

        private static string FormatCountdown(TimeSpan diff)
        {
            if (diff.TotalSeconds <= 0) return "0м";

            if (diff.TotalDays >= 7)
            {
                int weeks = (int)(diff.TotalDays / 7);
                int days  = (int)diff.TotalDays % 7;
                return $"{weeks}н {days}д";
            }
            if (diff.TotalHours >= 24)
            {
                int days  = (int)diff.TotalDays;
                int hours = diff.Hours;
                return $"{days}д {hours}ч";
            }
            if (diff.TotalMinutes >= 60)
            {
                int hours   = (int)diff.TotalHours;
                int minutes = diff.Minutes;
                return $"{hours}ч {minutes}м";
            }
            {
                int minutes = (int)diff.TotalMinutes;
                int seconds = diff.Seconds;
                return $"{minutes}м {seconds}с";
            }
        }

        // ─────────────────────────────────────────────────────────────

        private void RenderQuestList(
            QuestWindowModel               model,
            Func<string, string, UnityAction> claimFactory)
        {
            if (_questItemTemplate == null || _questListContainer == null) return;

            ClearContainer(_questListContainer);

            var cycle = GetActiveCycle(model);

            // Milestone banner — only on cycle tabs, always at top
            if (cycle != null && _milestoneTemplate != null)
            {
                var ms = NextMilestone(cycle);
                if (ms != null)
                {
                    var msItem = Instantiate(_milestoneTemplate, _questListContainer);
                    var state  = ms.IsFreeClaimed
                                        ? QuestMilestoneState.Claimed
                                        : ms.IsReached
                                            ? QuestMilestoneState.Reached
                                            : QuestMilestoneState.Locked;

                    int  msIndex      = cycle.Milestones.IndexOf(ms);
                    long prevRequired = msIndex > 0 ? cycle.Milestones[msIndex - 1].RequiredCount : 0;

                    msItem.Setup(
                        displayName:   ms.DisplayName,
                        requiredCount: ms.RequiredCount,
                        currentCount:  cycle.CompletedCount,
                        previousCount: prevRequired,
                        rewards:       ms.Rewards,
                        state:         state,
                        iconPath:      ms.IconPath,
                        onClaim:       _cachedClaimMilestoneFactory?.Invoke(ms.CycleID, ms.MilestoneID)
                    );
                    msItem.gameObject.SetActive(true);
                }
            }

            // Quest items
            var source = cycle != null ? cycle.Quests : model.PermanentQuests;

            foreach (var quest in source.OrderBy(q => SortOrder(q.Status))
                                        .ThenByDescending(q => q.ProgressPercent))
            {
                var item = Instantiate(_questItemTemplate, _questListContainer);
                item.Setup(
                    title:          quest.Title,
                    description:    "",
                    iconPath:       "",
                    status:         quest.Status,
                    canClaim:       quest.CanClaim,
                    objectives:     quest.Objectives,
                    rewards:        quest.Rewards,
                    onClaimClicked: quest.CanClaim ? claimFactory?.Invoke(quest.QuestID, quest.CycleID) : null
                );
                item.gameObject.SetActive(true);
            }
        }

        // ─────────────────────────────────────────────────────────────

        private ActiveQuestCycle GetActiveCycle(QuestWindowModel model)
        {
            if (string.IsNullOrEmpty(ActiveTabId)) return null;
            return model.AllCycles.FirstOrDefault(c => c.CycleID == ActiveTabId);
        }

        private static MilestoneUIItem NextMilestone(ActiveQuestCycle cycle)
        {
            if (cycle == null || cycle.Milestones.Count == 0) return null;
            var unclaimed = cycle.Milestones.Where(m => !m.IsFreeClaimed).ToList();
            // Если все взяты — показываем последний в состоянии Claimed
            return unclaimed.FirstOrDefault(m => m.IsReached)
                ?? unclaimed.FirstOrDefault()
                ?? cycle.Milestones[^1];
        }

        private static int SortOrder(QuestStatus status) => status switch
        {
            QuestStatus.Completed => 0,
            QuestStatus.Active    => 1,
            QuestStatus.Expired   => 2,
            QuestStatus.Claimed   => 3,
            _                     => 4,
        };

        private static async Task WrapAsync(Func<Task> fn)
        {
            try   { await fn(); }
            catch (Exception ex) { Debug.LogError($"[QuestWindow] {ex.Message}"); }
        }

        private static void ClearContainer(RectTransform container)
        {
            for (int i = container.childCount - 1; i >= 0; i--)
                DestroyImmediate(container.GetChild(i).gameObject);
        }
    }
}
