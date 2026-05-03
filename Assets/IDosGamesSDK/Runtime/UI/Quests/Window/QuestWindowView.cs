using System;
using System.Linq;
using System.Threading.Tasks;
using IDosGames.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace IDosGames.UI.Quest
{
    public enum QuestTab { Cycle, Permanent }

    public class QuestWindowView : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private TextMeshProUGUI _coinText;
        [SerializeField] private TextMeshProUGUI _gemText;

        [Header("Tabs")]
        [SerializeField] private Button _cycleTabButton;
        [SerializeField] private Button _permanentTabButton;
        [SerializeField] private Image _cycleTabImage;
        [SerializeField] private Image _permanentTabImage;

        [Header("Cycle Progress")]
        [SerializeField] private Slider _progressSlider;
        [SerializeField] private TextMeshProUGUI _progressText;
        [SerializeField] private TextMeshProUGUI _milestoneRewardText;

        [Header("Quest List")]
        [SerializeField] private QuestItemView _questItemTemplate;
        [SerializeField] private RectTransform _questListContainer;

        [Header("Milestone")]
        [SerializeField] private QuestMilestoneItemView _milestoneTemplate;
        [SerializeField] private Button _claimMilestoneButton;

        [Header("Premium")]
        [SerializeField] private Button _activatePremiumButton;
        [SerializeField] private GameObject _premiumLockIcon;

        [Header("Navigation")]
        [SerializeField] private Button _backButton;

        public Button BackButton => _backButton;
        public Button ActivatePremiumButton => _activatePremiumButton;
        public QuestTab ActiveTab { get; private set; } = QuestTab.Cycle;

        private QuestWindowModel _cachedModel;
        private Func<string, string, UnityAction> _cachedClaimQuestFactory;
        private Func<string, Func<Task>> _cachedClaimMilestoneFactory;

        public void Render(
            QuestWindowModel model,
            Func<string, string, UnityAction> claimQuestFactory,
            Func<string, Func<Task>> claimMilestoneFactory)
        {
            _cachedModel = model;
            _cachedClaimQuestFactory = claimQuestFactory;
            _cachedClaimMilestoneFactory = claimMilestoneFactory;

            SetupTabButtons();
            RenderUI(model);
            if (model.IsLoaded)
            {
                RenderQuestList(model, claimQuestFactory);
                RenderMilestones(model, claimMilestoneFactory);
            }
        }

        public void RenderUI(QuestWindowModel model)
        {
            if (_coinText != null) _coinText.text = model.CoinBalance.ToString("N0");
            if (_gemText != null) _gemText.text = model.GemBalance.ToString("N0");

            if (_milestoneRewardText != null)
            {
                var ms = NextMilestone(model);
                long amount = ms?.Rewards?.FirstOrDefault()?.Amount ?? 0;
                _milestoneRewardText.text = amount > 0 ? $"+{amount}" : "";
            }

            if (_activatePremiumButton != null) _activatePremiumButton.gameObject.SetActive(true);
            if (_premiumLockIcon != null) _premiumLockIcon.SetActive(false);

            RefreshTabVisuals();
        }

        public void SwitchTab(QuestTab tab)
        {
            if (_cachedModel == null) return;
            ActiveTab = tab;
            RefreshTabVisuals();
            RenderQuestList(_cachedModel, _cachedClaimQuestFactory);
            RenderMilestones(_cachedModel, _cachedClaimMilestoneFactory);
        }

        private void SetupTabButtons()
        {
            if (_cycleTabButton != null)
            {
                _cycleTabButton.onClick.RemoveAllListeners();
                _cycleTabButton.onClick.AddListener(() => SwitchTab(QuestTab.Cycle));
            }

            if (_permanentTabButton != null)
            {
                _permanentTabButton.onClick.RemoveAllListeners();
                _permanentTabButton.onClick.AddListener(() => SwitchTab(QuestTab.Permanent));
            }
        }

        private static readonly Color TabActiveColor   = new Color(0x31 / 255f, 0x81 / 255f, 1f);
        private static readonly Color TabInactiveColor = new Color(0x36 / 255f, 0x36 / 255f, 0x4E / 255f);

        private void RefreshTabVisuals()
        {
            bool isCycle = ActiveTab == QuestTab.Cycle;

            if (_cycleTabImage != null) _cycleTabImage.color = isCycle ? TabActiveColor : TabInactiveColor;
            if (_permanentTabImage != null) _permanentTabImage.color = isCycle ? TabInactiveColor : TabActiveColor;

            if (_cachedModel == null) return;

            float sliderValue;
            string progressLabel;

            if (isCycle)
            {
                sliderValue   = _cachedModel.SegmentProgress;
                progressLabel = _cachedModel.ProgressText;
            }
            else
            {
                int total = _cachedModel.PermanentQuests.Count;
                int done  = _cachedModel.PermanentQuests.Count(q => q.Status != QuestStatus.Active);
                sliderValue   = total > 0 ? (float)done / total : 0f;
                progressLabel = $"{done}/{total}";
            }

            if (_progressSlider != null) _progressSlider.value = sliderValue;
            if (_progressText != null)   _progressText.text    = progressLabel;
        }

        private void RenderQuestList(QuestWindowModel model, Func<string, string, UnityAction> claimFactory)
        {
            if (_questItemTemplate == null || _questListContainer == null) return;

            ClearContainer(_questListContainer);

            // Milestone — всегда первым, только на вкладке цикличных
            if (ActiveTab == QuestTab.Cycle && _milestoneTemplate != null)
            {
                var ms = NextMilestone(model);
                if (ms != null)
                {
                    var msItem = Instantiate(_milestoneTemplate, _questListContainer);
                    long rewardAmount = ms.Rewards?.FirstOrDefault()?.Amount ?? 0;
                    var state = ms.IsReached ? QuestMilestoneState.Reached : QuestMilestoneState.Locked;
                    msItem.Setup(
                        displayName: ms.DisplayName,
                        requiredCount: ms.RequiredCount,
                        currentCount: model.AllCycles.FirstOrDefault()?.CompletedCount ?? 0,
                        rewardAmount: rewardAmount,
                        state: state,
                        iconPath: ms.IconPath,
                        onClaim: _cachedClaimMilestoneFactory?.Invoke(ms.MilestoneID)
                    );
                    msItem.gameObject.SetActive(true);
                }
            }

            // Квесты — после milestone
            var source = ActiveTab == QuestTab.Permanent
                ? model.PermanentQuests
                : model.AllCycles.Count > 0
                    ? model.AllCycles.SelectMany(c => c.Quests).ToList()
                    : model.PermanentQuests;

            var quests = source
                .OrderBy(q => SortOrder(q.Status))
                .ThenByDescending(q => q.ProgressPercent);

            foreach (var quest in quests)
            {
                var item = Instantiate(_questItemTemplate, _questListContainer);
                long rewardAmount = quest.Rewards?.FirstOrDefault()?.Amount ?? 0;
                item.Setup(
                    title: quest.Title,
                    description: "",
                    iconPath: "",
                    current: quest.CurrentProgress,
                    target: quest.TargetProgress,
                    status: quest.Status,
                    canClaim: quest.CanClaim,
                    rewardAmount: rewardAmount,
                    onClaimClicked: quest.CanClaim ? claimFactory?.Invoke(quest.QuestID, quest.CycleID) : null
                );
                item.gameObject.SetActive(true);
            }
        }

        private void RenderMilestones(QuestWindowModel model, Func<string, Func<Task>> claimFactory)
        {
            if (_claimMilestoneButton == null) return;
            var ms = NextMilestone(model);
            bool canClaim = ms?.IsReached == true;
            _claimMilestoneButton.interactable = canClaim;
            _claimMilestoneButton.onClick.RemoveAllListeners();
            if (canClaim && claimFactory != null)
            {
                var claimFunc = claimFactory.Invoke(ms.MilestoneID);
                _claimMilestoneButton.onClick.AddListener(() => _ = WrapAsync(claimFunc));
            }
        }

        // Следующий незаклеймленный milestone: сначала тот что можно забрать, иначе ближайший locked
        private static MilestoneUIItem NextMilestone(QuestWindowModel model)
        {
            var unclaimed = model.CycleMilestones.Where(m => !m.IsFreeClaimed).ToList();
            return unclaimed.FirstOrDefault(m => m.IsReached) ?? unclaimed.FirstOrDefault();
        }

        private static int SortOrder(QuestStatus status) => status switch
        {
            QuestStatus.Completed => 0,
            QuestStatus.Active    => 1,
            QuestStatus.Expired   => 2,
            QuestStatus.Claimed   => 3,
            _                     => 4,
        };

        private static async Task WrapAsync(Func<Task> asyncAction)
        {
            try { await asyncAction(); }
            catch (Exception ex) { Debug.LogError($"[QuestWindow] Error: {ex.Message}"); }
        }

        private static void ClearContainer(RectTransform container)
        {
            for (int i = container.childCount - 1; i >= 0; i--)
                DestroyImmediate(container.GetChild(i).gameObject);
        }
    }
}