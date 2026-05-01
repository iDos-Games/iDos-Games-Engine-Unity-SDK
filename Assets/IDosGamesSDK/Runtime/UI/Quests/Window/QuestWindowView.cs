using System;
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
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private TextMeshProUGUI _coinText;
        [SerializeField] private TextMeshProUGUI _gemText;

        [Header("Cycle Progress")]
        [SerializeField] private Slider _progressSlider;
        [SerializeField] private TextMeshProUGUI _progressText;
        [SerializeField] private TextMeshProUGUI _milestoneRewardText;

        [Header("Quest List")]
        [SerializeField] private QuestItemView _questItemTemplate;
        [SerializeField] private RectTransform _questListContainer;

        [Header("Milestone")]
        [SerializeField] private QuestMilestoneItemView _milestoneTemplate;
        [SerializeField] private RectTransform _milestoneContainer;
        [SerializeField] private Button _claimMilestoneButton;

        [Header("Premium")]
        [SerializeField] private Button _activatePremiumButton;
        [SerializeField] private GameObject _premiumLockIcon;

        [Header("Navigation")]
        [SerializeField] private Button _backButton;

        [Header("States")]
        [SerializeField] private GameObject _loadingView;
        [SerializeField] private GameObject _contentView;
        [SerializeField] private GameObject _emptyView;

        public Button BackButton => _backButton;
        public Button ActivatePremiumButton => _activatePremiumButton;

        public void ShowLoading() => SetState(true, false, false);
        public void ShowEmpty()   => SetState(false, false, true);
        public void ShowContent() => SetState(false, true, false);

        public void Render(
            QuestWindowModel model,
            Func<string, string, UnityAction> claimQuestFactory,
            Func<string, Func<Task>> claimMilestoneFactory)
        {
            RenderUI(model);
            if (model.IsLoaded)
            {
                RenderQuestList(model, claimQuestFactory);
                RenderMilestones(model, claimMilestoneFactory);
            }
        }

        public void RenderUI(QuestWindowModel model)
        {
            if (!model.IsLoaded) { ShowEmpty(); return; }
            ShowContent();

            if (_titleText != null) _titleText.text = model.ActiveCycle?.CycleID ?? "Квесты";
            if (_coinText != null) _coinText.text = model.CoinBalance.ToString("N0");
            if (_gemText != null) _gemText.text = model.GemBalance.ToString("N0");

            if (_progressSlider != null) _progressSlider.value = model.SegmentProgress;
            if (_progressText != null) _progressText.text = model.ProgressText;
            
            if (_milestoneRewardText != null)
            {
                var ms = model.CycleMilestones.FirstOrDefault();
                long amount = ms?.Rewards?.FirstOrDefault()?.Amount ?? 0;
                _milestoneRewardText.text = amount > 0 ? $"+{amount}" : "";
            }

            if (_activatePremiumButton != null) _activatePremiumButton.gameObject.SetActive(true);
            if (_premiumLockIcon != null) _premiumLockIcon.SetActive(false);
        }

        public void UpdateTimer(string timerText)
        {
            if (_timerText != null) _timerText.text = timerText;
        }

        private void RenderQuestList(QuestWindowModel model, Func<string, string, UnityAction> claimFactory)
        {
            if (_questItemTemplate == null || _questListContainer == null) return;
            ClearContainer(_questListContainer, _questItemTemplate.gameObject);

            var quests = model.ActiveCycle?.Quests ?? model.PermanentQuests;
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
            if (_milestoneTemplate == null || _milestoneContainer == null) return;
            ClearContainer(_milestoneContainer, _milestoneTemplate.gameObject);

            foreach (var ms in model.CycleMilestones)
            {
                var item = Instantiate(_milestoneTemplate, _milestoneContainer);
                long rewardAmount = ms.Rewards?.FirstOrDefault()?.Amount ?? 0;
                var state = ms.IsFreeClaimed ? QuestMilestoneState.Claimed 
                          : ms.IsReached ? QuestMilestoneState.Reached 
                          : QuestMilestoneState.Locked;

                item.Setup(
                    displayName: ms.DisplayName,
                    requiredCount: ms.RequiredCount,
                    currentCount: model.ActiveCycle?.CompletedCount ?? 0,
                    rewardAmount: rewardAmount,
                    state: state,
                    iconPath: ms.IconPath,
                    onClaim: claimFactory?.Invoke(ms.MilestoneID)
                );
                item.gameObject.SetActive(true);
            }

            if (_claimMilestoneButton != null)
            {
                var mainMs = model.CycleMilestones.FirstOrDefault();
                bool canClaim = mainMs?.IsReached == true && mainMs.IsFreeClaimed == false;
                _claimMilestoneButton.interactable = canClaim;
                _claimMilestoneButton.onClick.RemoveAllListeners();
                if (canClaim && claimFactory != null)
                {
                    var claimFunc = claimFactory.Invoke(mainMs.MilestoneID);
                    _claimMilestoneButton.onClick.AddListener(() => _ = WrapAsync(claimFunc));
                }
            }
        }

        private static async Task WrapAsync(Func<Task> asyncAction)
        {
            try { await asyncAction(); }
            catch (Exception ex) { Debug.LogError($"[QuestWindow] Error: {ex.Message}"); }
        }

        private static void ClearContainer(RectTransform container, GameObject template)
        {
            for (int i = container.childCount - 1; i >= 0; i--)
            {
                var child = container.GetChild(i).gameObject;
                if (child == template) continue;
                DestroyImmediate(child);
            }
        }

        private void SetState(bool loading, bool content, bool empty)
        {
            if (_loadingView != null) _loadingView.SetActive(loading);
            if (_contentView != null) _contentView.SetActive(content);
            if (_emptyView != null) _emptyView.SetActive(empty);
        }
    }
}