#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace IDosGames
{
    public class LteDebugWindow : EditorWindow
    {
        [MenuItem("iDos Games/QA — LTE Debug")]
        public static void ShowWindow() =>
            GetWindow<LteDebugWindow>("LTE Debug");

        // ── State ───────────────────────────────────────────────────────

        private Vector2 _scroll;
        private string  _statusMsg = "";
        private bool    _statusOk  = true;

        // Snapshot updated only on Layout to keep Layout/Repaint in sync
        private List<ActiveEventInfo> _cachedEvents = new();

        private readonly Dictionary<string, bool>   _foldouts      = new();
        private readonly Dictionary<string, string> _grantAmounts  = new();
        private readonly Dictionary<string, int>    _sourceIndexes = new();

        private static readonly string[] SourceOptions =
            { "DailyLogin", "CustomAction", "ReferralInvite" };

        // ── Styles ──────────────────────────────────────────────────────

        private static class S
        {
            public static GUIStyle EventBox, SmallBtn, TagLabel;
            public static bool Ready;

            public static void Init()
            {
                if (Ready) return;
                Ready    = true;
                EventBox = new GUIStyle(GUI.skin.box)    { padding = new RectOffset(8, 8, 5, 5), margin = new RectOffset(0, 0, 3, 3) };
                SmallBtn = new GUIStyle(GUI.skin.button) { fontSize = 11, padding = new RectOffset(4, 4, 2, 2) };
                TagLabel = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = new Color(0.5f, 0.8f, 1f) } };
            }
        }

        // ── Lifecycle ───────────────────────────────────────────────────

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged    += OnPlayModeChanged;
            TimedEventService.OnActiveEventsLoaded    += OnEventsLoaded;
            TimedEventService.OnMilestoneClaimed      += OnMilestoneClaimed;
            TimedEventService.OnTokensGranted         += OnTokensGranted;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged    -= OnPlayModeChanged;
            TimedEventService.OnActiveEventsLoaded    -= OnEventsLoaded;
            TimedEventService.OnMilestoneClaimed      -= OnMilestoneClaimed;
            TimedEventService.OnTokensGranted         -= OnTokensGranted;
        }

        private void OnPlayModeChanged(PlayModeStateChange _) => Repaint();
        private void OnEventsLoaded(GetActiveEventsResponse r)  { _cachedEvents = r?.ActiveEvents ?? new(); Repaint(); }
        private void OnMilestoneClaimed(EventMilestoneClaimResponse _) => Fire(RefreshActiveEvents());
        private void OnTokensGranted(ResourceOperation _)              => Fire(RefreshActiveEvents());

        // ── Root ────────────────────────────────────────────────────────

        private void OnGUI()
        {
            S.Init();

            if (!Application.isPlaying)
            {
                EditorGUILayout.Space(12);
                EditorGUILayout.HelpBox("Enter Play Mode to use LTE Debug tools.", MessageType.Info);
                return;
            }

            DrawStatusBar();
            DrawToolbar();

            if (_cachedEvents.Count == 0)
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.HelpBox("No active events. Press ↺ Active Events to load.", MessageType.Warning);
                return;
            }

            DrawSummaryBar();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.Space(4);

            foreach (var evt in _cachedEvents)
                DrawEventSection(evt);

            EditorGUILayout.EndScrollView();
        }

        // ── Toolbar ─────────────────────────────────────────────────────

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
            if (GUILayout.Button("↺ Active Events", EditorStyles.toolbarButton, GUILayout.Width(110)))
                Fire(RefreshActiveEvents());
            if (GUILayout.Button("↺ Definitions",   EditorStyles.toolbarButton, GUILayout.Width(100)))
                Fire(RefreshDefinitions());
            if (GUILayout.Button("↺ User State",    EditorStyles.toolbarButton, GUILayout.Width(90)))
                Fire(RefreshUserState());
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        // ── Summary bar ──────────────────────────────────────────────────

        private void DrawSummaryBar()
        {
            var parts = new List<string>();
            foreach (var evt in _cachedEvents)
            {
                string name    = evt.Content?.DisplayName ?? evt.TimedEventID;
                long   earned  = evt.Progress?.Balance?.TotalEarned ?? 0;
                long   maxBal  = evt.Content?.Token?.MaxBalance ?? 0;
                int    msTotal = evt.Content?.Milestones?.Count ?? 0;
                int    msClaimed = evt.Progress?.Milestone?.ClaimedIDs?.Count ?? 0;
                bool   ended   = DateTime.UtcNow >= evt.ComputedEndUtc;
                string timer   = ended ? "ENDED" : FormatTime(evt.ComputedEndUtc - DateTime.UtcNow);

                parts.Add($"{name}: {earned:N0}/{maxBal:N0} tokens, {msClaimed}/{msTotal} ms  ⏱{timer}");
            }

            var prevBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.15f, 0.15f, 0.2f);
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label(string.Join("   |   ", parts), EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
            GUI.backgroundColor = prevBg;
        }

        // ── Event section ────────────────────────────────────────────────

        private void DrawEventSection(ActiveEventInfo evt)
        {
            string id      = evt.TimedEventID;
            string name    = evt.Content?.DisplayName ?? id;
            bool   ended   = DateTime.UtcNow >= evt.ComputedEndUtc;
            string timer   = ended ? "ENDED" : FormatTime(evt.ComputedEndUtc - DateTime.UtcNow);
            string mode    = (evt.Content?.ClaimMode ?? EventClaimMode.Instant) switch
            {
                EventClaimMode.AfterEventEnd    => "AfterEnd",
                EventClaimMode.FeaturedAfterEnd => "FeaturedAfterEnd",
                _                               => "Instant",
            };

            string header = $"  {name}  [{mode}]  ⏱ {timer}" +
                            $"  Earn:{(evt.CanEarn ? "✓" : "✗")}  Claim:{(evt.CanClaim ? "✓" : "✗")}";

            _foldouts.TryGetValue(id, out bool open);
            open = EditorGUILayout.Foldout(open, header, true, EditorStyles.foldoutHeader);
            _foldouts[id] = open;
            if (!open) return;

            EditorGUILayout.BeginVertical(S.EventBox);

            DrawTokenInfo(evt);
            EditorGUILayout.Space(2);
            DrawTokenProgress(evt);
            EditorGUILayout.Space(4);
            DrawGrantRow(evt);
            EditorGUILayout.Space(4);
            DrawMilestones(evt);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(4);
        }

        // ── Token info ────────────────────────────────────────────────────

        private void DrawTokenInfo(ActiveEventInfo evt)
        {
            var token = evt.Content?.Token;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(
                $"Token: {token?.DisplayName ?? "—"}   Max: {token?.MaxBalance:N0}   DailyCap: {token?.DailyEarnCap:N0}   MaxPerGrant: {token?.MaxPerGrant:N0}",
                S.TagLabel, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Copy ID", S.SmallBtn, GUILayout.Width(62), GUILayout.Height(18)))
            {
                EditorGUIUtility.systemCopyBuffer = evt.TimedEventID;
                Log($"Copied: {evt.TimedEventID}", true);
            }
            EditorGUILayout.EndHorizontal();
        }

        // ── Token progress ────────────────────────────────────────────────

        private void DrawTokenProgress(ActiveEventInfo evt)
        {
            long earned    = evt.Progress?.Balance?.TotalEarned ?? 0;
            long current   = evt.Progress?.Balance?.Current ?? 0;
            long maxBal    = evt.Content?.Token?.MaxBalance ?? 0;
            long dailyEarn = evt.Progress?.Daily?.TotalEarned ?? 0;
            long dailyCap  = evt.Content?.Token?.DailyEarnCap ?? 0;

            float totalPct = maxBal  > 0 ? Mathf.Clamp01((float)earned    / maxBal)  : 0f;
            float dailyPct = dailyCap > 0 ? Mathf.Clamp01((float)dailyEarn / dailyCap) : 0f;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Total earned:", EditorStyles.miniLabel, GUILayout.Width(80));
            Rect totalBar = GUILayoutUtility.GetRect(200, 15, GUILayout.Width(200));
            DrawProgressBar(totalBar, totalPct, $"{earned:N0} / {maxBal:N0}", earned >= maxBal);
            GUILayout.Label($"  Balance: {current:N0}", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Daily earned:", EditorStyles.miniLabel, GUILayout.Width(80));
            Rect dailyBar = GUILayoutUtility.GetRect(200, 15, GUILayout.Width(200));
            string dailyLabel = $"{dailyEarn:N0} / {(dailyCap > 0 ? dailyCap.ToString("N0") : "no cap")}";
            DrawProgressBar(dailyBar, dailyPct, dailyLabel, dailyPct >= 1f);
            EditorGUILayout.EndHorizontal();
        }

        // ── Grant row ─────────────────────────────────────────────────────

        private void DrawGrantRow(ActiveEventInfo evt)
        {
            string id = evt.TimedEventID;
            if (!_grantAmounts.ContainsKey(id))  _grantAmounts[id]  = "50";
            if (!_sourceIndexes.ContainsKey(id)) _sourceIndexes[id] = 0;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Grant:", EditorStyles.miniBoldLabel, GUILayout.Width(40));
            GUILayout.Label("Source:", EditorStyles.miniLabel, GUILayout.Width(46));
            _sourceIndexes[id] = EditorGUILayout.Popup(_sourceIndexes[id], SourceOptions, GUILayout.Width(120));
            GUILayout.Label("Amount:", EditorStyles.miniLabel, GUILayout.Width(52));
            _grantAmounts[id] = EditorGUILayout.TextField(_grantAmounts[id], GUILayout.Width(50));

            var prevBg = GUI.backgroundColor;
            GUI.backgroundColor = evt.CanEarn ? new Color(0.3f, 0.75f, 0.4f) : GUI.backgroundColor;
            GUI.enabled = evt.CanEarn;
            if (GUILayout.Button("Grant Tokens", S.SmallBtn, GUILayout.Width(90), GUILayout.Height(18)))
            {
                string source = SourceOptions[_sourceIndexes[id]];
                if (long.TryParse(_grantAmounts[id], out long amount) && amount > 0)
                    Fire(GrantTokens(evt, source, amount));
                else
                    Log("Invalid amount.", false);
            }
            GUI.enabled = true;
            GUI.backgroundColor = prevBg;
            EditorGUILayout.EndHorizontal();
        }

        // ── Milestones ────────────────────────────────────────────────────

        private void DrawMilestones(ActiveEventInfo evt)
        {
            var milestones = evt.Content?.Milestones?.Values
                .OrderBy(m => m.SortOrder).ToList();
            if (milestones == null || milestones.Count == 0) return;

            long   earned    = evt.Progress?.Balance?.TotalEarned ?? 0;
            var    claimed   = evt.Progress?.Milestone?.ClaimedIDs ?? new List<string>();
            bool   ended     = DateTime.UtcNow >= evt.ComputedEndUtc;
            var    claimMode = evt.Content?.ClaimMode ?? EventClaimMode.Instant;

            // Batch button
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Milestones", EditorStyles.miniBoldLabel, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Claim All Reached", S.SmallBtn, GUILayout.Width(110), GUILayout.Height(18)))
                Fire(ClaimAllReached(evt, milestones, earned, claimed, ended, claimMode));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(2);

            foreach (var ms in milestones)
            {
                bool reached = earned >= ms.RequiredTokensEarned;
                bool isClaimed = claimed.Contains(ms.MilestoneID);
                bool pending = reached && !isClaimed && claimMode switch
                {
                    EventClaimMode.AfterEventEnd    => !ended,
                    EventClaimMode.FeaturedAfterEnd => ms.IsFeatured && !ended,
                    _                               => false,
                };
                bool canClaim = reached && !isClaimed && !pending && evt.CanClaim;

                string icon = isClaimed ? "✓" : pending ? "⏳" : reached ? "●" : "○";

                var prevCol = GUI.contentColor;
                GUI.contentColor = isClaimed ? Color.gray
                    : pending  ? new Color(1f, 0.75f, 0.2f)
                    : reached  ? new Color(0.3f, 0.95f, 0.5f)
                    : GUI.contentColor;

                EditorGUILayout.BeginHorizontal();

                // Icon + name
                GUILayout.Label($"{icon}  {ms.DisplayName ?? ms.MilestoneID}", GUILayout.Width(140));
                GUI.contentColor = prevCol;

                // Progress bar (total earned vs this milestone threshold)
                float pct = ms.RequiredTokensEarned > 0
                    ? Mathf.Clamp01((float)earned / ms.RequiredTokensEarned) : 1f;
                Rect bar = GUILayoutUtility.GetRect(110, 14, GUILayout.Width(110));
                DrawProgressBar(bar, pct,
                    $"{Math.Min(earned, ms.RequiredTokensEarned):N0}/{ms.RequiredTokensEarned:N0}",
                    isClaimed);

                // Rewards summary
                GUILayout.Label(BuildRewardSummary(ms.Rewards), S.TagLabel, GUILayout.ExpandWidth(true));

                // Featured badge
                if (ms.IsFeatured)
                {
                    var prevBg2 = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(0.65f, 0.45f, 0.05f);
                    GUILayout.Label("★", S.SmallBtn, GUILayout.Width(20), GUILayout.Height(16));
                    GUI.backgroundColor = prevBg2;
                }

                // Copy milestone ID
                if (GUILayout.Button("⎘", S.SmallBtn, GUILayout.Width(20), GUILayout.Height(16)))
                {
                    EditorGUIUtility.systemCopyBuffer = ms.MilestoneID;
                    Log($"Copied: {ms.MilestoneID}", true);
                }

                // Claim button
                string btnLabel = isClaimed ? "Claimed" : pending ? "Pending" : "Claim";
                var prevBg = GUI.backgroundColor;
                GUI.backgroundColor = canClaim ? new Color(0.3f, 0.8f, 0.4f) : GUI.backgroundColor;
                GUI.enabled = canClaim;
                if (GUILayout.Button(btnLabel, S.SmallBtn, GUILayout.Width(56), GUILayout.Height(16)))
                    Fire(ClaimMilestone(evt.Type, evt.TimedEventID, ms.MilestoneID));
                GUI.enabled = true;
                GUI.backgroundColor = prevBg;

                EditorGUILayout.EndHorizontal();
            }
        }

        // ── Reward summary ────────────────────────────────────────────────

        private static string BuildRewardSummary(ResourceGrant r)
        {
            if (r == null) return "—";
            var parts = new List<string>();

            // Standard free rewards — any CurrencyID or anonymous amounts
            if (r.Standard?.Entries != null)
            {
                foreach (var e in r.Standard.Entries)
                {
                    long amt = e.Amount ?? 0;
                    if (amt <= 0) continue;
                    parts.Add(string.IsNullOrEmpty(e.CurrencyID) ? $"+{amt:N0}" : $"{e.CurrencyID} +{amt:N0}");
                }
            }

            // Premium tier 1 bonus (percentage)
            var bonus = r.PremiumBonuses?.FirstOrDefault();
            if (bonus != null) parts.Add($"+{bonus.BonusPercent:0}%(t{bonus.MinPremiumTier})");

            // Premium tier 2+ fixed bundles
            if (r.PremiumTiers != null)
            {
                foreach (var tier in r.PremiumTiers)
                {
                    if (tier?.Resources?.Entries == null) continue;
                    foreach (var e in tier.Resources.Entries)
                    {
                        long amt = e.Amount ?? 0;
                        if (amt <= 0) continue;
                        string label = string.IsNullOrEmpty(e.CurrencyID) ? $"+{amt:N0}" : $"{e.CurrencyID} +{amt:N0}";
                        parts.Add($"{label}(t{tier.MinPremiumTier})");
                    }
                }
            }

            return parts.Count > 0 ? string.Join("  ", parts) : "—";
        }

        // ── Async actions ─────────────────────────────────────────────────

        private async Task RefreshActiveEvents()
        {
            Log("Loading active events...", true);
            var r = await TimedEventService.GetActiveEvents();
            Log(r.Success ? $"Loaded {r.Data?.ActiveEvents?.Count ?? 0} event(s)." : $"Error: {r.Error}", r.Success);
        }

        private async Task RefreshDefinitions()
        {
            Log("Loading definitions...", true);
            var r = await TimedEventService.GetDefinitions();
            Log(r.Success ? "Definitions loaded." : $"Error: {r.Error}", r.Success);
        }

        private async Task RefreshUserState()
        {
            Log("Loading user state...", true);
            var r = await TimedEventService.GetUserLteState();
            Log(r.Success ? "User state loaded." : $"Error: {r.Error}", r.Success);
        }

        private async Task GrantTokens(ActiveEventInfo evt, string source, long amount)
        {
            Log($"Granting {amount} {evt.Content?.Token?.DisplayName ?? "tokens"} via {source}...", true);
            var r = await TimedEventService.GrantTokens(evt.Type, evt.TimedEventID, source, amountOverride: amount);
            Log(r.Success
                ? $"✓ +{amount} tokens granted to {evt.Content?.DisplayName}"
                : $"Error: {r.Error}", r.Success);
        }

        private async Task ClaimMilestone(TimedEventType type, string lteID, string milestoneID)
        {
            Log($"Claiming {milestoneID}...", true);
            var r = await TimedEventService.ClaimMilestone(type, lteID, milestoneID);
            Log(r.Success ? $"✓ Claimed: {milestoneID}" : $"Error: {r.Error}", r.Success);
        }

        private async Task ClaimAllReached(
            ActiveEventInfo evt,
            List<EventMilestoneDefinition> milestones,
            long earned, List<string> claimed,
            bool ended, EventClaimMode claimMode)
        {
            Log("Claiming all reached milestones...", true);
            int count = 0;
            foreach (var ms in milestones)
            {
                bool reached   = earned >= ms.RequiredTokensEarned;
                bool isClaimed = claimed.Contains(ms.MilestoneID);
                bool pending   = reached && !isClaimed && claimMode switch
                {
                    EventClaimMode.AfterEventEnd    => !ended,
                    EventClaimMode.FeaturedAfterEnd => ms.IsFeatured && !ended,
                    _                               => false,
                };
                if (!reached || isClaimed || pending) continue;

                var r = await TimedEventService.ClaimMilestone(evt.Type, evt.TimedEventID, ms.MilestoneID);
                if (r.Success) count++;
            }
            Log($"Claimed {count} milestone(s).", count > 0);
            Fire(RefreshActiveEvents());
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private void Fire(Task t) =>
            t.ContinueWith(_ => Repaint(), TaskScheduler.FromCurrentSynchronizationContext());

        private void Log(string msg, bool ok)
        {
            _statusMsg = msg;
            _statusOk  = ok;
            Repaint();
        }

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
