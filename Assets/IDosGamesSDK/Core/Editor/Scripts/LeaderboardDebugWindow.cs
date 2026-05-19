#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace IDosGames
{
    public class LeaderboardDebugWindow : EditorWindow
    {
        [MenuItem("iDos Games/QA — Leaderboard Debug")]
        public static void ShowWindow() =>
            GetWindow<LeaderboardDebugWindow>("Leaderboard Debug");

        // ── State ────────────────────────────────────────────────────────────

        private Vector2 _scroll;
        private string  _statusMsg = "";
        private bool    _statusOk  = true;

        private LeaderboardDefinitions                              _cachedDefs;
        private readonly Dictionary<string, GetLeaderboardResponse> _topLists    = new();
        private readonly Dictionary<string, GetMyProgressResponse>  _progressMap = new();

        private readonly Dictionary<string, bool>   _foldouts      = new();
        private readonly Dictionary<string, string> _submitAmounts = new();

        // ── Styles ──────────────────────────────────────────────────────────

        private static class S
        {
            public static GUIStyle Box, SmallBtn, TagLabel, ScoreLabel;
            public static bool     Ready;

            public static void Init()
            {
                if (Ready) return;
                Ready      = true;
                Box        = new GUIStyle(GUI.skin.box)    { padding = new RectOffset(8, 8, 5, 5), margin = new RectOffset(0, 0, 3, 3) };
                SmallBtn   = new GUIStyle(GUI.skin.button) { fontSize = 11, padding = new RectOffset(4, 4, 2, 2) };
                TagLabel   = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = new Color(0.5f, 0.8f, 1f) } };
                ScoreLabel = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = new Color(1f, 0.85f, 0.4f) } };
            }
        }

        // ── Lifecycle ────────────────────────────────────────────────────────

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged           += OnPlayModeChanged;
            LeaderboardService.OnLeaderboardDefinitionsLoaded += OnDefinitionsLoaded;
            LeaderboardService.OnLeaderboardLoaded            += OnTopListLoaded;
            LeaderboardService.OnMyProgressLoaded             += OnProgressLoaded;
            LeaderboardService.OnMilestoneClaimed             += OnMilestoneClaimed;
            LeaderboardService.OnCycleRewardClaimed           += OnCycleRewardClaimed;
            LeaderboardService.OnScoreSubmitted               += OnScoreSubmitted;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged           -= OnPlayModeChanged;
            LeaderboardService.OnLeaderboardDefinitionsLoaded -= OnDefinitionsLoaded;
            LeaderboardService.OnLeaderboardLoaded            -= OnTopListLoaded;
            LeaderboardService.OnMyProgressLoaded             -= OnProgressLoaded;
            LeaderboardService.OnMilestoneClaimed             -= OnMilestoneClaimed;
            LeaderboardService.OnCycleRewardClaimed           -= OnCycleRewardClaimed;
            LeaderboardService.OnScoreSubmitted               -= OnScoreSubmitted;
        }

        private void OnPlayModeChanged(PlayModeStateChange _) => Repaint();

        private void OnDefinitionsLoaded(LeaderboardDefinitions d)
            { _cachedDefs = d; Repaint(); }
        private void OnTopListLoaded(GetLeaderboardResponse r)
            { if (r != null) { _topLists[r.LeaderboardID] = r; Repaint(); } }
        private void OnProgressLoaded(GetMyProgressResponse r)
            { if (r != null) { _progressMap[r.LeaderboardID] = r; Repaint(); } }
        private void OnMilestoneClaimed(ClaimLeaderboardMilestoneResponse r)
            => Fire(r != null ? RefreshProgress(r.LeaderboardID) : RefreshAllProgress());
        private void OnCycleRewardClaimed(ClaimCycleRewardResponse r)
            => Fire(r != null ? RefreshProgress(r.LeaderboardID) : RefreshAllProgress());
        private void OnScoreSubmitted(SubmitScoreResponse r)
            => Fire(r != null ? RefreshProgress(r.LeaderboardID) : RefreshAllProgress());

        // ── Root ─────────────────────────────────────────────────────────────

        private void OnGUI()
        {
            S.Init();

            if (!Application.isPlaying)
            {
                EditorGUILayout.Space(12);
                EditorGUILayout.HelpBox("Enter Play Mode to use Leaderboard Debug tools.", MessageType.Info);
                return;
            }

            DrawStatusBar();
            DrawToolbar();

            if (_cachedDefs?.Definitions == null || _cachedDefs.Definitions.Count == 0)
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.HelpBox("No definitions. Press ↺ Definitions to load.", MessageType.Warning);
                return;
            }

            DrawSummaryBar();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.Space(4);

            foreach (var def in _cachedDefs.Definitions.Values)
                DrawLeaderboardSection(def);

            EditorGUILayout.EndScrollView();
        }

        // ── Toolbar ──────────────────────────────────────────────────────────

        private void DrawStatusBar()
        {
            if (string.IsNullOrEmpty(_statusMsg)) return;
            var prev = GUI.backgroundColor;
            GUI.backgroundColor = _statusOk ? new Color(0.25f, 0.65f, 0.35f) : new Color(0.75f, 0.25f, 0.25f);
            EditorGUILayout.HelpBox(_statusMsg, _statusOk ? MessageType.Info : MessageType.Error);
            GUI.backgroundColor = prev;
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("↺ Definitions",   EditorStyles.toolbarButton, GUILayout.Width(100)))
                Fire(RefreshDefinitions());
            if (GUILayout.Button("↺ All Progress",  EditorStyles.toolbarButton, GUILayout.Width(100)))
                Fire(RefreshAllProgress());
            if (GUILayout.Button("↺ All Top Lists", EditorStyles.toolbarButton, GUILayout.Width(100)))
                Fire(RefreshAllTopLists());

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        // ── Summary bar ──────────────────────────────────────────────────────

        private void DrawSummaryBar()
        {
            var parts = new List<string>();
            foreach (var def in _cachedDefs.Definitions.Values)
            {
                _progressMap.TryGetValue(def.LeaderboardID, out var prog);
                long score  = prog?.ScoreEarnedThisCycle ?? 0;
                int  rank   = prog?.LastKnownRank ?? 0;
                string notif = (prog?.HasUnclaimedMilestone == true ? "🏅" : "")
                             + (prog?.HasUnclaimedReward    == true ? "🎁" : "");
                string name = def.DisplayName ?? def.LeaderboardID;
                parts.Add($"{name}: {FormatScore(score, def)}  Rank:{(rank > 0 ? rank.ToString() : "—")}{(notif.Length > 0 ? " " + notif : "")}");
            }

            var prevBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.15f, 0.15f, 0.2f);
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label(string.Join("   |   ", parts), EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
            GUI.backgroundColor = prevBg;
        }

        // ── Per-leaderboard section ───────────────────────────────────────────

        private void DrawLeaderboardSection(LeaderboardDefinition def)
        {
            string id = def.LeaderboardID;
            _progressMap.TryGetValue(id, out var prog);
            _topLists.TryGetValue(id, out var top);

            DateTime? cycleEnd = top?.CycleEndUtc;
            bool cycleEnded = def.CycleReset == LeaderboardCycleReset.Never
                           || (cycleEnd.HasValue && DateTime.UtcNow >= cycleEnd.Value);

            string timer = def.CycleReset == LeaderboardCycleReset.Never ? "∞"
                : cycleEnd.HasValue ? (cycleEnded ? "ENDED" : FormatTime(cycleEnd.Value - DateTime.UtcNow))
                : "—";

            string modeStr = def.MilestoneClaimMode switch
            {
                EventClaimMode.AfterEventEnd    => "AfterEnd",
                EventClaimMode.FeaturedAfterEnd => "FeaturedAfterEnd",
                _                               => "Instant",
            };

            string notif = (prog?.HasUnclaimedMilestone == true ? " 🏅" : "")
                         + (prog?.HasUnclaimedReward    == true ? " 🎁" : "");

            string header = $"  {def.DisplayName ?? id}  [{def.ScoreAggregation}/{def.CycleReset}]  [{modeStr}]  ⏱ {timer}{notif}";

            _foldouts.TryGetValue(id, out bool open);
            open = EditorGUILayout.Foldout(open, header, true, EditorStyles.foldoutHeader);
            _foldouts[id] = open;
            if (!open) return;

            EditorGUILayout.BeginVertical(S.Box);

            DrawDefinitionInfo(def, top);
            EditorGUILayout.Space(2);
            DrawMyProgress(def, prog);
            EditorGUILayout.Space(4);
            DrawSubmitRow(def);

            if (prog?.HasUnclaimedReward == true)
            {
                EditorGUILayout.Space(2);
                DrawClaimCycleRewardButton(id, prog);
            }

            EditorGUILayout.Space(4);
            DrawMilestones(def, prog, cycleEnded);
            EditorGUILayout.Space(4);
            DrawTopList(def, top);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(4);
        }

        // ── Definition info ───────────────────────────────────────────────────

        private void DrawDefinitionInfo(LeaderboardDefinition def, GetLeaderboardResponse top)
        {
            long participants = top?.TotalParticipants ?? 0;
            string bracket = string.IsNullOrEmpty(top?.BracketID)
                ? (def.BracketSize > 0 ? $"size {def.BracketSize}" : "off")
                : top.BracketID;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(
                $"Unit: {def.ScoreDisplayName}   Min score: {def.MinScoreToEnterTop}   " +
                $"Limit: {def.TopUsersLimit}   Bracket: {bracket}   " +
                $"Participants: {(participants > 0 ? participants.ToString("N0") : "—")}",
                S.TagLabel, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Copy ID", S.SmallBtn, GUILayout.Width(62), GUILayout.Height(18)))
            {
                EditorGUIUtility.systemCopyBuffer = def.LeaderboardID;
                Log($"Copied: {def.LeaderboardID}", true);
            }
            EditorGUILayout.EndHorizontal();
        }

        // ── My progress ───────────────────────────────────────────────────────

        private void DrawMyProgress(LeaderboardDefinition def, GetMyProgressResponse prog)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Progress:", EditorStyles.miniBoldLabel, GUILayout.Width(60));

            if (prog == null)
            {
                GUILayout.Label("not loaded", EditorStyles.miniLabel, GUILayout.ExpandWidth(true));
            }
            else
            {
                GUILayout.Label(
                    $"Score: {prog.CurrentScore:N0}   Earned: {prog.ScoreEarnedThisCycle:N0}   " +
                    $"Rank: {(prog.LastKnownRank > 0 ? prog.LastKnownRank.ToString() : "—")}   " +
                    $"Reward: {(prog.HasUnclaimedReward ? "⚠ YES" : "no")}   " +
                    $"Milestone: {(prog.HasUnclaimedMilestone ? "⚠ YES" : "no")}",
                    S.TagLabel, GUILayout.ExpandWidth(true));
            }

            if (GUILayout.Button("↺", S.SmallBtn, GUILayout.Width(20), GUILayout.Height(18)))
                Fire(RefreshProgress(def.LeaderboardID));
            EditorGUILayout.EndHorizontal();

            // Progress bar toward next unclaimed milestone
            if (prog != null && def.Milestones?.Count > 0)
            {
                var nextMs = def.Milestones.Values
                    .OrderBy(m => m.RequiredScore)
                    .FirstOrDefault(m => prog.ScoreEarnedThisCycle < m.RequiredScore);

                if (nextMs != null)
                {
                    float pct = nextMs.RequiredScore > 0
                        ? Mathf.Clamp01((float)prog.ScoreEarnedThisCycle / nextMs.RequiredScore) : 1f;

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("→ Next ms:", EditorStyles.miniLabel, GUILayout.Width(60));
                    Rect bar = GUILayoutUtility.GetRect(200, 14, GUILayout.Width(200));
                    DrawProgressBar(bar, pct, $"{prog.ScoreEarnedThisCycle:N0}/{nextMs.RequiredScore:N0}", false);
                    GUILayout.Label($"  {nextMs.DisplayName ?? nextMs.MilestoneID}", EditorStyles.miniLabel);
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        // ── Submit score ──────────────────────────────────────────────────────

        private void DrawSubmitRow(LeaderboardDefinition def)
        {
            string id = def.LeaderboardID;
            if (!_submitAmounts.ContainsKey(id)) _submitAmounts[id] = "100";

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Submit:", EditorStyles.miniBoldLabel, GUILayout.Width(50));
            GUILayout.Label($"{def.ScoreDisplayName}:", EditorStyles.miniLabel, GUILayout.Width(50));
            _submitAmounts[id] = EditorGUILayout.TextField(_submitAmounts[id], GUILayout.Width(80));

            var prevBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.3f, 0.6f, 0.9f);
            if (GUILayout.Button("Submit Score", S.SmallBtn, GUILayout.Width(90), GUILayout.Height(18)))
            {
                if (long.TryParse(_submitAmounts[id], out long score) && score > 0)
                    Fire(SubmitScore(id, score));
                else
                    Log("Invalid score — must be an integer > 0.", false);
            }
            GUI.backgroundColor = prevBg;
            EditorGUILayout.EndHorizontal();
        }

        // ── Claim cycle reward button ─────────────────────────────────────────

        private void DrawClaimCycleRewardButton(string leaderboardID, GetMyProgressResponse prog)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(
                $"🎁 Unclaimed cycle reward — rank {(prog.LastKnownRank > 0 ? prog.LastKnownRank.ToString() : "?")}",
                EditorStyles.miniBoldLabel, GUILayout.ExpandWidth(true));
            var prevBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.8f, 0.5f, 0.1f);
            if (GUILayout.Button("Claim Rank Reward", S.SmallBtn, GUILayout.Width(120), GUILayout.Height(18)))
                Fire(ClaimCycleReward(leaderboardID));
            GUI.backgroundColor = prevBg;
            EditorGUILayout.EndHorizontal();
        }

        // ── Milestones ────────────────────────────────────────────────────────

        private void DrawMilestones(
            LeaderboardDefinition def,
            GetMyProgressResponse prog,
            bool cycleEnded)
        {
            if (def.Milestones == null || def.Milestones.Count == 0) return;

            long   scoreEarned = prog?.ScoreEarnedThisCycle ?? 0;
            string nextMsId    = prog?.NextMilestone?.MilestoneID;
            var    claimMode   = def.MilestoneClaimMode;

            var sorted = def.Milestones.Values
                .OrderBy(m => m.SortOrder)
                .ThenBy(m => m.RequiredScore)
                .ToList();

            // ─ header row with "Claim All Reached" ─
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Milestones", EditorStyles.miniBoldLabel, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Claim All Reached", S.SmallBtn, GUILayout.Width(110), GUILayout.Height(18)))
                Fire(ClaimAllReachable(def.LeaderboardID, sorted, scoreEarned, nextMsId, claimMode, cycleEnded));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(2);

            // ─ per-milestone rows ─
            bool passedNext = false;
            foreach (var ms in sorted)
            {
                bool isReached = scoreEarned >= ms.RequiredScore;
                bool isClaimed;

                if (string.IsNullOrEmpty(nextMsId))
                    isClaimed = isReached;                        // all claimed
                else if (ms.MilestoneID == nextMsId)
                { isClaimed = false; passedNext = true; }        // this is the next one
                else
                    isClaimed = isReached && !passedNext;         // claimed only before next

                bool canClaimNow = CanClaimNow(claimMode, ms.IsFeatured, cycleEnded);
                bool isPending   = isReached && !isClaimed && !canClaimNow;
                bool canClaim    = isReached && !isClaimed &&  canClaimNow;

                string icon = isClaimed ? "✓" : isPending ? "⏳" : isReached ? "●" : "○";

                var prevCol = GUI.contentColor;
                GUI.contentColor = isClaimed ? Color.gray
                    : isPending ? new Color(1f, 0.75f, 0.2f)
                    : isReached ? new Color(0.3f, 0.95f, 0.5f)
                    : GUI.contentColor;

                EditorGUILayout.BeginHorizontal();

                GUILayout.Label($"{icon}  {ms.DisplayName ?? ms.MilestoneID}", GUILayout.Width(145));
                GUI.contentColor = prevCol;

                float pct = ms.RequiredScore > 0
                    ? Mathf.Clamp01((float)scoreEarned / ms.RequiredScore) : 1f;
                Rect bar = GUILayoutUtility.GetRect(110, 14, GUILayout.Width(110));
                DrawProgressBar(bar, pct,
                    $"{Math.Min(scoreEarned, ms.RequiredScore):N0}/{ms.RequiredScore:N0}",
                    isClaimed);

                GUILayout.Label(BuildRewardSummary(ms.Rewards), S.TagLabel, GUILayout.ExpandWidth(true));

                if (ms.IsFeatured)
                {
                    var prevBg2 = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(0.65f, 0.45f, 0.05f);
                    GUILayout.Label("★", S.SmallBtn, GUILayout.Width(20), GUILayout.Height(16));
                    GUI.backgroundColor = prevBg2;
                }

                if (GUILayout.Button("⎘", S.SmallBtn, GUILayout.Width(20), GUILayout.Height(16)))
                {
                    EditorGUIUtility.systemCopyBuffer = ms.MilestoneID;
                    Log($"Copied: {ms.MilestoneID}", true);
                }

                string btnLabel = isClaimed ? "Claimed" : isPending ? "Pending" : "Claim";
                var prevBg = GUI.backgroundColor;
                GUI.backgroundColor = canClaim ? new Color(0.3f, 0.8f, 0.4f) : GUI.backgroundColor;
                GUI.enabled = canClaim;
                if (GUILayout.Button(btnLabel, S.SmallBtn, GUILayout.Width(56), GUILayout.Height(16)))
                    Fire(ClaimMilestone(def.LeaderboardID, ms.MilestoneID));
                GUI.enabled = true;
                GUI.backgroundColor = prevBg;

                EditorGUILayout.EndHorizontal();
            }
        }

        // ── Top list (top 5) ──────────────────────────────────────────────────

        private void DrawTopList(LeaderboardDefinition def, GetLeaderboardResponse top)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Top Players", EditorStyles.miniBoldLabel, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("↺ Top List", S.SmallBtn, GUILayout.Width(70), GUILayout.Height(18)))
                Fire(RefreshTopList(def.LeaderboardID));
            EditorGUILayout.EndHorizontal();

            if (top?.TopUsers == null || top.TopUsers.Count == 0)
            {
                GUILayout.Label("  (not loaded)", EditorStyles.miniLabel);
                return;
            }

            var rows = (def.ScoreAggregation == LeaderboardScoreAggregation.BestTime
                    ? top.TopUsers.OrderBy(u => u.Score)
                    : top.TopUsers.OrderBy(u => u.Rank))
                .Take(5);

            foreach (var u in rows)
            {
                string prem  = u.PublicProfile?.Premium == true ? "★" : " ";
                string name  = u.PublicProfile?.Username ?? u.UserID;
                string score = FormatScore(u.Score, def);
                GUILayout.Label($"  {prem}  #{u.Rank,-4} {name,-20} {score}", EditorStyles.miniLabel);
            }

            if (top.TopUsers.Count > 5)
                GUILayout.Label($"  … +{top.TopUsers.Count - 5} more", EditorStyles.miniLabel);
        }

        // ── Async actions ─────────────────────────────────────────────────────

        private async Task RefreshDefinitions()
        {
            Log("Loading definitions...", true);
            var r = await LeaderboardService.GetDefinitions();
            Log(r.Success
                ? $"Loaded {r.Data?.Definitions?.Count ?? 0} leaderboard(s)."
                : $"Error: {r.Error}", r.Success);

            if (r.Success && r.Data?.Definitions != null)
            {
                var tasks = new List<Task>();
                foreach (var id in r.Data.Definitions.Keys)
                {
                    tasks.Add(LeaderboardService.GetMyProgress(id));
                    tasks.Add(LeaderboardService.GetLeaderboard(id));
                }
                await Task.WhenAll(tasks);
                Log($"All data loaded for {r.Data.Definitions.Count} leaderboard(s).", true);
            }
        }

        private async Task RefreshAllProgress()
        {
            if (_cachedDefs?.Definitions == null) { Log("Load definitions first.", false); return; }
            Log("Refreshing all progress...", true);
            var tasks = _cachedDefs.Definitions.Keys
                .Select(id => (Task)LeaderboardService.GetMyProgress(id));
            await Task.WhenAll(tasks);
            Log($"Progress refreshed for {_cachedDefs.Definitions.Count} leaderboard(s).", true);
        }

        private async Task RefreshAllTopLists()
        {
            if (_cachedDefs?.Definitions == null) { Log("Load definitions first.", false); return; }
            Log("Refreshing all top lists...", true);
            var tasks = _cachedDefs.Definitions.Keys
                .Select(id => (Task)LeaderboardService.GetLeaderboard(id));
            await Task.WhenAll(tasks);
            Log($"Top lists refreshed for {_cachedDefs.Definitions.Count} leaderboard(s).", true);
        }

        private async Task RefreshProgress(string leaderboardID)
        {
            var r = await LeaderboardService.GetMyProgress(leaderboardID);
            Log(r.Success ? $"✓ Progress updated: {leaderboardID}" : $"Error: {r.Error}", r.Success);
        }

        private async Task RefreshTopList(string leaderboardID)
        {
            Log($"Loading top list: {leaderboardID}...", true);
            var r = await LeaderboardService.GetLeaderboard(leaderboardID);
            Log(r.Success ? $"✓ Top list loaded: {leaderboardID} ({r.Data?.TopUsers?.Count ?? 0} entries)" : $"Error: {r.Error}", r.Success);
        }

        private async Task SubmitScore(string leaderboardID, long score)
        {
            Log($"Submitting {score} → {leaderboardID}...", true);
            var r = await LeaderboardService.SubmitScore(leaderboardID, score);
            Log(r.Success
                ? $"✓ New score: {r.Data?.NewScore:N0}  cycle v{r.Data?.CycleVersion}"
                : $"Error: {r.Error}", r.Success);
        }

        private async Task ClaimCycleReward(string leaderboardID)
        {
            Log($"Claiming cycle reward: {leaderboardID}...", true);
            var r = await LeaderboardService.ClaimCycleReward(leaderboardID);
            Log(r.Success ? $"✓ Claimed! Rank: {r.Data?.Rank}" : $"Error: {r.Error}", r.Success);
        }

        private async Task ClaimMilestone(string leaderboardID, string milestoneID)
        {
            Log($"Claiming {milestoneID}...", true);
            var r = await LeaderboardService.ClaimMilestone(leaderboardID, milestoneID);
            Log(r.Success ? $"✓ Claimed: {milestoneID}" : $"Error: {r.Error}", r.Success);
        }

        private async Task ClaimAllReachable(
            string leaderboardID,
            List<LeaderboardMilestoneDefinition> sorted,
            long scoreEarned,
            string nextMsId,
            EventClaimMode claimMode,
            bool cycleEnded)
        {
            Log($"Claiming all reachable milestones for {leaderboardID}...", true);
            int count = 0;
            bool passedNext = false;

            foreach (var ms in sorted)
            {
                bool isReached = scoreEarned >= ms.RequiredScore;
                bool isClaimed;

                if (string.IsNullOrEmpty(nextMsId))
                    isClaimed = isReached;
                else if (ms.MilestoneID == nextMsId)
                { isClaimed = false; passedNext = true; }
                else
                    isClaimed = isReached && !passedNext;

                if (!isReached || isClaimed || !CanClaimNow(claimMode, ms.IsFeatured, cycleEnded)) continue;

                var r = await LeaderboardService.ClaimMilestone(leaderboardID, ms.MilestoneID);
                if (r.Success) count++;
            }

            Log($"Claimed {count} milestone(s) for {leaderboardID}.", count > 0);
            Fire(RefreshProgress(leaderboardID));
        }

        // ── Shared helpers ────────────────────────────────────────────────────

        private static bool CanClaimNow(EventClaimMode mode, bool isFeatured, bool cycleEnded) =>
            mode switch
            {
                EventClaimMode.Instant          => true,
                EventClaimMode.AfterEventEnd    => cycleEnded,
                EventClaimMode.FeaturedAfterEnd => !isFeatured || cycleEnded,
                _                               => true,
            };

        private static string BuildRewardSummary(ResourceGrant r)
        {
            if (r == null) return "—";
            var parts = new List<string>();

            if (r.Standard?.Entries != null)
                foreach (var e in r.Standard.Entries)
                {
                    long amt = e.Amount ?? 0;
                    if (e.Type == ResourceEntryType.Item)
                        parts.Add($"[{e.ItemID ?? "item"}]×{(amt > 0 ? amt.ToString() : "1")}");
                    else if (amt > 0)
                        parts.Add(string.IsNullOrEmpty(e.CurrencyID)
                            ? $"+{amt:N0}"
                            : $"{e.CurrencyID} +{amt:N0}");
                }

            var bonus = r.PremiumBonuses?.FirstOrDefault();
            if (bonus != null) parts.Add($"+{bonus.BonusPercent:0}%(t{bonus.MinPremiumTier})");

            if (r.PremiumTiers != null)
                foreach (var tier in r.PremiumTiers)
                {
                    if (tier?.Resources?.Entries == null) continue;
                    foreach (var e in tier.Resources.Entries)
                    {
                        long amt = e.Amount ?? 0;
                        if (amt <= 0) continue;
                        string label = string.IsNullOrEmpty(e.CurrencyID)
                            ? $"+{amt:N0}"
                            : $"{e.CurrencyID} +{amt:N0}";
                        parts.Add($"{label}(t{tier.MinPremiumTier})");
                    }
                }

            return parts.Count > 0 ? string.Join("  ", parts) : "—";
        }

        private static string FormatScore(long score, LeaderboardDefinition def)
        {
            string unit = def.ScoreDisplayName ?? "";
            string fmt  = score >= 1_000_000 ? $"{score / 1_000_000f:0.#}M"
                        : score >= 1_000     ? $"{score / 1_000f:0.#}K"
                        : score.ToString("N0");
            return string.IsNullOrEmpty(unit) ? fmt : $"{fmt} {unit}";
        }

        private void Fire(Task t) =>
            t.ContinueWith(_ => Repaint(), TaskScheduler.FromCurrentSynchronizationContext());

        private void Log(string msg, bool ok) { _statusMsg = msg; _statusOk = ok; Repaint(); }

        private static void DrawProgressBar(Rect rect, float pct, string label, bool done)
        {
            EditorGUI.DrawRect(rect, new Color(0.18f, 0.18f, 0.18f));
            if (pct > 0)
            {
                var fill = new Rect(rect.x, rect.y, rect.width * pct, rect.height);
                EditorGUI.DrawRect(fill, done ? new Color(0.25f, 0.6f, 0.3f) : new Color(0.2f, 0.55f, 0.85f));
            }
            GUI.Label(rect, label, new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal    = { textColor = Color.white },
            });
        }

        private static string FormatTime(TimeSpan t)
        {
            if (t.TotalSeconds <= 0) return "expired";
            if (t.TotalDays  >= 1)  return $"{(int)t.TotalDays}d {t.Hours}h";
            if (t.TotalHours >= 1)  return $"{(int)t.TotalHours}h {t.Minutes}m";
            return $"{(int)t.TotalMinutes}m";
        }
    }
}
#endif
