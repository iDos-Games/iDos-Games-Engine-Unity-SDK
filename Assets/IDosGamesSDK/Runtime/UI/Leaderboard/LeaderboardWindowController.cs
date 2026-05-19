using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace IDosGames.UI.Leaderboard
{
    [RequireComponent(typeof(LeaderboardView))]
    public class LeaderboardWindowController : MonoBehaviour
    {
        // ── Static cache — survives window close/reopen ────────────────────────
        // Gives an instant render the next time the window is opened.
        private static LeaderboardDefinitions                               s_defs;
        private static readonly Dictionary<string, GetLeaderboardResponse>  s_topLists    = new();
        private static readonly Dictionary<string, GetMyProgressResponse>   s_progressMap = new();

        // ── Inspector ──────────────────────────────────────────────────────────
        [Header("References")]
        [SerializeField] private LeaderboardView _view;

        [Header("Currency Keys")]
        [SerializeField] private string _coinCurrencyKey = "CO";
        [SerializeField] private string _gemCurrencyKey  = "GE";

        // ── Instance state ─────────────────────────────────────────────────────
        private LeaderboardWindowModel _model;
        private bool                   _isActive;

        // ── Unity lifecycle ────────────────────────────────────────────────────

        private void Awake()
        {
            _model = new LeaderboardWindowModel();
            if (_view == null) _view = GetComponent<LeaderboardView>();
        }

        private void OnEnable()
        {
            _isActive = true;

            LeaderboardService.OnLeaderboardDefinitionsLoaded += HandleDefinitionsLoaded;
            LeaderboardService.OnLeaderboardLoaded            += HandleLeaderboardLoaded;
            LeaderboardService.OnMyProgressLoaded             += HandleMyProgressLoaded;
            LeaderboardService.OnMilestoneClaimed             += HandleMilestoneClaimed;
            LeaderboardService.OnCycleRewardClaimed           += HandleCycleRewardClaimed;
            LeaderboardService.OnScoreSubmitted               += HandleScoreSubmitted;
            IDosGamesData.User.OnVirtualCurrencyUpdated       += HandleCurrencyUpdated;

            _view.BackButton?.onClick.AddListener(OnBackClicked);

            // Instant render from cache — eliminates blank-screen flicker on reopen
            if (s_defs?.Definitions?.Count > 0)
                RebuildAndRender();
            else
                _view.ShowLoading();

            _ = LoadAllAsync();
        }

        private void OnDisable()
        {
            _isActive = false;

            LeaderboardService.OnLeaderboardDefinitionsLoaded -= HandleDefinitionsLoaded;
            LeaderboardService.OnLeaderboardLoaded            -= HandleLeaderboardLoaded;
            LeaderboardService.OnMyProgressLoaded             -= HandleMyProgressLoaded;
            LeaderboardService.OnMilestoneClaimed             -= HandleMilestoneClaimed;
            LeaderboardService.OnCycleRewardClaimed           -= HandleCycleRewardClaimed;
            LeaderboardService.OnScoreSubmitted               -= HandleScoreSubmitted;
            IDosGamesData.User.OnVirtualCurrencyUpdated       -= HandleCurrencyUpdated;

            _view.BackButton?.onClick.RemoveListener(OnBackClicked);
        }

        // ── Data loading ───────────────────────────────────────────────────────

        private async Task LoadAllAsync()
        {
            var defsResult = await LeaderboardService.GetDefinitions();
            if (!_isActive) return;

            if (!defsResult.Success || defsResult.Data?.Definitions == null)
            {
                Message.Show(defsResult.Error ?? MessageCode.SOMETHING_WENT_WRONG.ToString());
                _view.ShowEmpty();
                return;
            }

            // Load top list and personal progress per leaderboard in parallel
            var tasks = new List<Task>();
            foreach (var id in defsResult.Data.Definitions.Keys)
            {
                tasks.Add(LeaderboardService.GetLeaderboard(id));
                tasks.Add(LeaderboardService.GetMyProgress(id));
            }
            await Task.WhenAll(tasks);
        }

        // ── Service callbacks ──────────────────────────────────────────────────

        private void HandleDefinitionsLoaded(LeaderboardDefinitions defs)
        {
            if (!_isActive) return;
            s_defs = defs;
            RebuildAndRender();
        }

        private void HandleLeaderboardLoaded(GetLeaderboardResponse r)
        {
            if (!_isActive || r == null) return;
            s_topLists[r.LeaderboardID] = r;
            RebuildAndRender();
        }

        private void HandleMyProgressLoaded(GetMyProgressResponse r)
        {
            if (!_isActive || r == null) return;
            s_progressMap[r.LeaderboardID] = r;
            RebuildAndRender();
        }

        private void HandleMilestoneClaimed(ClaimLeaderboardMilestoneResponse r)
        {
            if (!_isActive || r == null) return;
            // Refresh progress to get updated NextMilestone (determines claim state of all milestones)
            _ = LeaderboardService.GetMyProgress(r.LeaderboardID);
        }

        private void HandleCycleRewardClaimed(ClaimCycleRewardResponse r)
        {
            if (!_isActive || r == null) return;
            _ = LeaderboardService.GetMyProgress(r.LeaderboardID);
        }

        private void HandleScoreSubmitted(SubmitScoreResponse r)
        {
            if (!_isActive || r == null) return;
            // Service already applied optimistic patch; refresh for authoritative rank
            _ = LeaderboardService.GetMyProgress(r.LeaderboardID);
        }

        private void HandleCurrencyUpdated()
        {
            if (!_isActive) return;
            var (coins, gems) = ReadCurrencies();
            _model.CoinBalance = coins;
            _model.GemBalance  = gems;
            _view.Render(_model, BuildClaimFactory());
        }

        // ── Render ─────────────────────────────────────────────────────────────

        private void RebuildAndRender()
        {
            RebuildModel();
            _view.Render(_model, BuildClaimFactory());
        }

        private void RebuildModel()
        {
            var (coins, gems) = ReadCurrencies();

            _model.Clear();
            _model.CoinBalance = coins;
            _model.GemBalance  = gems;

            if (s_defs?.Definitions == null) return;

            var userID = GetUserID();
            foreach (var def in s_defs.Definitions.Values)
            {
                s_topLists.TryGetValue(def.LeaderboardID,    out var topList);
                s_progressMap.TryGetValue(def.LeaderboardID, out var progress);
                _model.AddLeaderboard(def, topList, progress, userID);
            }
        }

        // (leaderboardID, milestoneID) → async claim action
        private Func<string, string, Func<Task>> BuildClaimFactory() =>
            (leaderboardID, milestoneID) => async () =>
            {
                var result = await LeaderboardService.ClaimMilestone(leaderboardID, milestoneID);
                if (!result.Success)
                    Message.Show(result.Error ?? MessageCode.SOMETHING_WENT_WRONG.ToString());
            };

        // ── Helpers ────────────────────────────────────────────────────────────

        private (long coins, long gems) ReadCurrencies()
        {
            var vc     = IDosGamesData.User.State?.InventoryV2?.VirtualCurrencies;
            long coins = vc != null && vc.TryGetValue(_coinCurrencyKey, out var c) ? c.Amount : 0;
            long gems  = vc != null && vc.TryGetValue(_gemCurrencyKey,  out var g) ? g.Amount : 0;
            return (coins, gems);
        }

        private static string GetUserID() =>
            AuthenticationService.GetAuthContext()?.UserID ?? "";

        private void OnBackClicked() => gameObject.SetActive(false);
    }
}
