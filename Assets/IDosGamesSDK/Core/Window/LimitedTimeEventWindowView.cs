using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


namespace IDosGames.UI.LimitedTimeEvent
{
    public class LimitedTimeEventWindowView : MonoBehaviour
    {
        // ── Resource Bar ─────────────────────────────────────────────────────
        [Header("Resource Bar")]
        [SerializeField] private TextMeshProUGUI _coinText;
        [SerializeField] private TextMeshProUGUI _gemText;

        // ── Top Bar ───────────────────────────────────────────────────────────
        [Header("Top Bar")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _timerText;

        // ── Progress Slider ───────────────────────────────────────────────────
        [Header("Progress Slider")]
        [SerializeField] private Slider           _progressFill;
        [SerializeField] private Slider           _totalProgressFill;
        [SerializeField] private TextMeshProUGUI _progressText;
        [FormerlySerializedAs("_balanceText")] [SerializeField] private TextMeshProUGUI _milestoneCountText;
        [SerializeField] private Image           _tokenIcon;

        // ── Activate Button ───────────────────────────────────────────────────
        [Header("Activate Button")]
        [SerializeField] private Button     _activateButton;
        [SerializeField] private GameObject _activateLockIcon;

        // ── Free Milestones — Left column ─────────────────────────────────────
        [Header("Free Milestones — Left column")]
        [Tooltip("Template item to instantiate — must NOT be a scene active child")]
        [SerializeField] private MilestoneItemView _freeMilestoneTemplate;
        [Tooltip("Parent RectTransform that contains the free milestone items")]
        [SerializeField] private RectTransform     _freeMilestoneContainer;

        // ── Premium Milestones — Right column ─────────────────────────────────
        [Header("Premium Milestones — Right column (BubbleFrame05 x N)")]
        [SerializeField] private MilestoneItemView _premiumMilestone;
        [SerializeField] private RectTransform     _premiumMilestoneContainer;

        // ── Stage Progress ────────────────────────────────────────────────────
        [Header("Stage Progress")]
        [SerializeField] private StageProgressView _stageProgress;

        // ── Scroll fitter ─────────────────────────────────────────────────────
        [Header("Scroll")]
        [SerializeField] private LTEScrollContentFitter _scrollFitter;

        // ── Event Tabs ────────────────────────────────────────────────────────
        [Header("Event Tabs")]
        [SerializeField] private EventTabButtonView _tabTemplate;
        [SerializeField] private RectTransform      _tabContainer;

        // ── State Views ───────────────────────────────────────────────────────
        [Header("State Views")]
        [SerializeField] private GameObject _loadingView;
        [SerializeField] private GameObject _contentView;
        [SerializeField] private GameObject _emptyView;

        // ── Navigation ────────────────────────────────────────────────────────
        [Header("Navigation")]
        [SerializeField] private Button _backButton;

        public Button ActivateButton => _activateButton;
        public Button BackButton     => _backButton;

        private readonly List<EventTabButtonView> _spawnedTabs = new();

        // ─────────────────────────────────────────────────────────────────────

        public void ShowLoading() => SetState(loading: true,  content: false, empty: false);
        public void ShowEmpty()   => SetState(loading: false, content: false, empty: true);

        // Полный рендер — спаунит/удаляет префабы milestone. Вызывать только явно.
        // claimFactory: (milestoneId, isPremium) → Func<Task>, строится контроллером.
        public void Render(LimitedTimeEventWindowModel model, Func<string, bool, Func<Task>> claimFactory)
        {
            RenderUI(model);
            if (model.IsLoaded) RenderMilestones(model, claimFactory);
        }

        // Лёгкий рендер — только тексты и слайдер, без трогания префабов.
        public void RenderUI(LimitedTimeEventWindowModel model)
        {
            if (!model.IsLoaded)
            {
                ShowEmpty();
                return;
            }

            SetState(loading: false, content: true, empty: false);

            RenderResourceBar(model);
            RenderHeader(model.Event);
            RenderProgressSlider(model);
            RenderActivateButton(model.HasPremium);
        }

        public void UpdateTimer(string timerText)
        {
            if (_timerText != null) _timerText.text = timerText;
        }

        // ── Tabs ──────────────────────────────────────────────────────────────

        public void RenderTabs(IReadOnlyList<string> names, int selectedIndex, Action<int> onSelect)
        {
            bool multiEvent = names != null && names.Count > 1;

            if (_tabContainer != null) _tabContainer.gameObject.SetActive(multiEvent);
            if (!multiEvent || _tabTemplate == null || _tabContainer == null) return;

            if (_spawnedTabs.Count != names.Count)
            {
                for (int i = _tabContainer.childCount - 1; i >= 0; i--)
                {
                    var child = _tabContainer.GetChild(i).gameObject;
                    if (child == _tabTemplate.gameObject) continue;
                    DestroyImmediate(child);
                }
                _spawnedTabs.Clear();

                for (int i = 0; i < names.Count; i++)
                {
                    int index = i;
                    var tab = Instantiate(_tabTemplate, _tabContainer);
                    tab.gameObject.SetActive(true);
                    tab.Setup(names[i], () => onSelect?.Invoke(index));
                    _spawnedTabs.Add(tab);
                }
            }

            SetActiveTab(selectedIndex);
        }

        public void SetActiveTab(int index)
        {
            for (int i = 0; i < _spawnedTabs.Count; i++)
                _spawnedTabs[i].SetSelected(i == index);
        }

        // ── Private ───────────────────────────────────────────────────────────

        private void RenderResourceBar(LimitedTimeEventWindowModel model)
        {
            if (_coinText != null) _coinText.text = model.CoinBalance.ToString("N0");
            if (_gemText  != null) _gemText.text  = model.GemBalance.ToString("N0");
        }

        private void RenderHeader(ActiveEventInfo evt)
        {
            if (_titleText != null)
                _titleText.text = evt.Content?.DisplayName ?? string.Empty;

            var tokenPaths = evt.Content?.Token?.AssetPaths;
            if (_tokenIcon != null && tokenPaths?.Count > 0)
                LoadTokenIconAsync(tokenPaths.Values.FirstOrDefault());
        }

        private void RenderProgressSlider(LimitedTimeEventWindowModel model)
        {
            if (_progressFill      != null) _progressFill.value      = model.SegmentProgress;
            if (_totalProgressFill != null) _totalProgressFill.value = model.TokenProgress;
            if (_progressText      != null) _progressText.text       = model.ProgressText;
            if (_milestoneCountText != null) _milestoneCountText.text = model.CompletedMilestones + "";
        }

        private void RenderActivateButton(bool hasPremium)
        {
            if (_activateButton   != null) _activateButton.gameObject.SetActive(!hasPremium);
            if (_activateLockIcon != null) _activateLockIcon.SetActive(!hasPremium);
        }

        private void RenderMilestones(LimitedTimeEventWindowModel model, Func<string, bool, Func<Task>> claimFactory)
        {
            var milestonesDict = model.Event.Content?.Milestones;
            if (milestonesDict == null) return;

            var milestones = milestonesDict.Values.OrderBy(m => m.SortOrder).ToList();

            // ── Free column ────────────────────────────────────────────────
            if (_freeMilestoneTemplate != null && _freeMilestoneContainer != null)
            {
                ClearContainer(_freeMilestoneContainer, _freeMilestoneTemplate.gameObject);

                for (int i = 0; i < milestones.Count; i++)
                {
                    var def   = milestones[i];
                    var item  = Instantiate(_freeMilestoneTemplate, _freeMilestoneContainer);
                    var state = GetMilestoneState(def, model, isPremiumColumn: false);
                    item.Setup(def, state, isPremiumColumn: false, model.HasPremium, claimFactory?.Invoke(def.MilestoneID, false));
                    item.gameObject.SetActive(true);
                }
            }

            // ── Premium column ──────────────────────────────────────────────
            if (_premiumMilestone != null && _premiumMilestoneContainer != null)
            {
                ClearContainer(_premiumMilestoneContainer, _premiumMilestone.gameObject);

                for (int i = 0; i < milestones.Count; i++)
                {
                    var def   = milestones[i];
                    var item  = Instantiate(_premiumMilestone, _premiumMilestoneContainer);
                    var state = GetMilestoneState(def, model, isPremiumColumn: true);
                    item.Setup(def, state, isPremiumColumn: true, model.HasPremium, claimFactory?.Invoke(def.MilestoneID, true));
                    item.gameObject.SetActive(true);
                }
            }

            // ── Stage progress ──────────────────────────────────────────────
            int reached = 0;
            for (int i = 0; i < milestones.Count; i++)
            {
                if (model.IsMilestoneReached(milestones[i].RequiredTokensEarned))
                    reached = i + 1;
            }
            _stageProgress?.Render(reached, milestones.Count);

            // ── Resize scroll content ───────────────────────────────────────
            _scrollFitter?.Refresh();
        }

        private static void ClearContainer(RectTransform container, GameObject template)
        {
            for (int i = container.childCount - 1; i >= 0; i--)
            {
                var child = container.GetChild(i).gameObject;
                if (child == template) continue;
                DestroyImmediate(child); // работает и в Editor, и в Play Mode
            }
        }

        private static MilestoneItemState GetMilestoneState(
            EventMilestoneDefinition def, LimitedTimeEventWindowModel model, bool isPremiumColumn)
        {
            if (def == null) return MilestoneItemState.Locked;
            bool claimed = isPremiumColumn
                ? model.IsMilestonePremiumClaimed(def.MilestoneID)
                : model.IsMilestoneFreeClaimed(def.MilestoneID);
            if (claimed) return MilestoneItemState.Claimed;
            if (model.IsMilestonePendingEventEnd(def.RequiredTokensEarned, def.IsFeatured)) return MilestoneItemState.Pending;
            if (model.IsMilestoneReached(def.RequiredTokensEarned)) return MilestoneItemState.Reached;
            return MilestoneItemState.Locked;
        }

        private void SetState(bool loading, bool content, bool empty)
        {
            if (_loadingView != null) _loadingView.SetActive(loading);
            if (_contentView != null) _contentView.SetActive(content);
            if (_emptyView   != null) _emptyView.SetActive(empty);
        }

        private async void LoadTokenIconAsync(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            var sprite = await ImageLoader.GetSpriteAsync(path);
            if (this != null && _tokenIcon != null && sprite != null)
                _tokenIcon.sprite = sprite;
        }
    }
}
