#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace IDosGames
{
    public class QuestDebugWindow : EditorWindow
    {
        [MenuItem("iDos Games/QA — Quest Debug")]
        public static void ShowWindow() =>
            GetWindow<QuestDebugWindow>("Quest Debug");

        // ── State ───────────────────────────────────────────────────────

        private Vector2 _scroll;
        private string  _statusMsg  = "";
        private bool    _statusOk   = true;

        // Per-objective amount inputs: key = questID + ":" + objectiveID
        private readonly Dictionary<string, string> _amountInputs = new();

        // Foldout state per cycle/section
        private readonly Dictionary<string, bool> _foldouts = new();

        // ── Styles ──────────────────────────────────────────────────────

        private static class S
        {
            public static GUIStyle SectionHeader;
            public static GUIStyle QuestBox;
            public static GUIStyle MetricLabel;
            public static GUIStyle SmallBtn;
            public static bool     Ready;

            public static void Init()
            {
                if (Ready) return;
                Ready = true;

                SectionHeader = new GUIStyle(EditorStyles.boldLabel)
                    { fontSize = 12 };

                QuestBox = new GUIStyle(GUI.skin.box)
                    { padding = new RectOffset(8, 8, 5, 5), margin = new RectOffset(0, 0, 2, 2) };

                MetricLabel = new GUIStyle(EditorStyles.miniLabel)
                    { normal = { textColor = new Color(0.5f, 0.8f, 1f) } };

                SmallBtn = new GUIStyle(GUI.skin.button)
                    { fontSize = 11, padding = new RectOffset(4, 4, 2, 2) };
            }
        }

        // ── Lifecycle ───────────────────────────────────────────────────

        // Stored delegate — same object each time so -= actually removes it
        private readonly Action<PlayModeStateChange> _onPlayModeChanged = _ => { };

        // Cached per-frame data — updated only on Layout event to keep Layout/Repaint in sync
        private QuestDefinitions _cachedConfig;
        private UserQuestState   _cachedState;

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange _) => Repaint();

        // ── Root ────────────────────────────────────────────────────────

        private void OnGUI()
        {
            S.Init();

            if (!Application.isPlaying)
            {
                EditorGUILayout.Space(12);
                EditorGUILayout.HelpBox(
                    "Enter Play Mode to use Quest Debug tools.\n" +
                    "Authentication is only available at runtime.",
                    MessageType.Info);
                return;
            }

            // Snapshot game state only on Layout — Repaint reuses same snapshot,
            // preventing layout/repaint mismatch when async data arrives mid-frame
            if (Event.current.type == EventType.Layout)
            {
                _cachedConfig = IDosGamesData.Config.TitlePublicConfiguration?.Quest;
                _cachedState  = IDosGamesData.User.State?.Quest;
            }

            DrawStatusBar();
            DrawToolbar();

            if (_cachedConfig == null || _cachedState == null)
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.HelpBox("No quest data loaded. Press Refresh.", MessageType.Warning);
                return;
            }

            DrawSummaryHeader(_cachedState, _cachedConfig);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.Space(4);

            DrawCycles(_cachedState, _cachedConfig);
            DrawPermanentQuests(_cachedState, _cachedConfig);

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
            if (GUILayout.Button("↺ Refresh State",  EditorStyles.toolbarButton, GUILayout.Width(110)))
                Fire(RefreshState());
            if (GUILayout.Button("↺ Refresh Cycles", EditorStyles.toolbarButton, GUILayout.Width(110)))
                Fire(RefreshCycles());
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        // ── Summary header ───────────────────────────────────────────────

        private void DrawSummaryHeader(UserQuestState state, QuestDefinitions config)
        {
            var parts = new List<string>();

            foreach (var cycle in state.Cycles.Values.OrderBy(c => c.CycleID))
            {
                var quests = config.Quests?.Values
                    .Where(q => q.CycleIDs?.Contains(cycle.CycleID) == true)
                    .ToList() ?? new List<QuestDefinition>();

                int total     = quests.Count;
                int completed = quests.Count(q =>
                {
                    cycle.Quests.TryGetValue(q.QuestID, out var p);
                    return p?.Status == QuestStatus.Completed || p?.Status == QuestStatus.Claimed;
                });

                var displayName = config.Cycles?.GetValueOrDefault(cycle.CycleID)?.DisplayName ?? cycle.CycleID;
                parts.Add($"{displayName}: {completed}/{total}");
            }

            var perms = config.Quests?.Values
                .Where(q => q.CycleIDs == null || q.CycleIDs.Count == 0).ToList();
            if (perms?.Count > 0)
            {
                int permDone = perms.Count(q =>
                {
                    UserQuestProgress p = null;
                    state.PermanentQuests?.TryGetValue(q.QuestID, out p);
                    return p?.Status == QuestStatus.Completed || p?.Status == QuestStatus.Claimed;
                });
                parts.Add($"Постоянные: {permDone}/{perms.Count}");
            }

            var prevBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.15f, 0.15f, 0.2f);
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label(string.Join("   |   ", parts), EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
            GUI.backgroundColor = prevBg;
        }

        // ── Cycles ──────────────────────────────────────────────────────

        private void DrawCycles(UserQuestState state, QuestDefinitions config)
        {
            foreach (var cycle in state.Cycles.Values.OrderBy(c => c.CycleID))
            {
                var def   = config.Cycles?.GetValueOrDefault(cycle.CycleID);
                var timer = FormatTime(cycle.CycleEndUtc - DateTime.UtcNow);

                var quests = (config.Quests?.Values
                    .Where(q => q.CycleIDs?.Contains(cycle.CycleID) == true)
                    .OrderBy(q => q.SortOrder)
                    .ToList()) ?? new List<QuestDefinition>();

                int total     = quests.Count;
                int completed = quests.Count(q =>
                {
                    cycle.Quests.TryGetValue(q.QuestID, out var p);
                    return p?.Status == QuestStatus.Completed || p?.Status == QuestStatus.Claimed;
                });

                string header = $"  {def?.DisplayName ?? cycle.CycleID}   {completed}/{total} done   ⏱ {timer}";

                _foldouts.TryGetValue(cycle.CycleID, out bool open);
                EditorGUILayout.BeginHorizontal();
                bool next = EditorGUILayout.Foldout(open, header, true, EditorStyles.foldoutHeader);
                _foldouts[cycle.CycleID] = next;
                EditorGUILayout.EndHorizontal();

                if (!next) continue;

                // Batch actions
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(12);
                if (GUILayout.Button("Complete All", GUILayout.Width(100), GUILayout.Height(20)))
                    Fire(CompleteAll(quests, cycle));
                if (GUILayout.Button("Claim All",    GUILayout.Width(80),  GUILayout.Height(20)))
                    Fire(ClaimAll(quests, cycle.CycleID));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(2);

                // Milestones
                if (def?.Milestones?.Count > 0)
                    DrawMilestones(cycle, def);

                // Quest rows
                foreach (var qDef in quests)
                {
                    cycle.Quests.TryGetValue(qDef.QuestID, out var prog);
                    DrawQuestRow(qDef, prog, cycle.CycleID);
                }

                EditorGUILayout.Space(6);
            }
        }

        // ── Milestones ──────────────────────────────────────────────────

        private void DrawMilestones(UserQuestCycleState cycle, QuestCycleDefinition def)
        {
            EditorGUILayout.BeginVertical(S.QuestBox);
            EditorGUILayout.LabelField("Milestones", EditorStyles.miniBoldLabel);

            foreach (var ms in def.Milestones.Values.OrderBy(m => m.RequiredCompletedQuests))
            {
                bool claimed = cycle.ClaimedMilestoneIDs?.Contains(ms.MilestoneID) == true;
                bool reached = cycle.CompletedQuestsCount >= ms.RequiredCompletedQuests;

                EditorGUILayout.BeginHorizontal();

                string icon = claimed ? "✓" : reached ? "●" : "○";
                var prevCol = GUI.contentColor;
                GUI.contentColor = claimed ? Color.gray : reached ? new Color(0.3f, 0.95f, 0.5f) : GUI.contentColor;
                GUILayout.Label($"{icon}  {ms.MilestoneID}   ({cycle.CompletedQuestsCount}/{ms.RequiredCompletedQuests} quests)",
                    GUILayout.ExpandWidth(true));
                GUI.contentColor = prevCol;

                var prevBg = GUI.backgroundColor;
                GUI.backgroundColor = reached && !claimed ? new Color(0.3f, 0.8f, 0.4f) : GUI.backgroundColor;
                GUI.enabled = reached && !claimed;
                if (GUILayout.Button("Claim", GUILayout.Width(52), GUILayout.Height(18)))
                {
                    string cId = cycle.CycleID, mId = ms.MilestoneID;
                    Fire(ClaimMilestone(cId, mId));
                }
                GUI.enabled = true;
                GUI.backgroundColor = prevBg;

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        // ── Quest row ────────────────────────────────────────────────────

        private void DrawQuestRow(QuestDefinition def, UserQuestProgress prog, string cycleID)
        {
            var status = prog?.Status ?? QuestStatus.Active;

            EditorGUILayout.BeginVertical(S.QuestBox);

            // ── Title row ──
            EditorGUILayout.BeginHorizontal();

            var titleCol = status switch
            {
                QuestStatus.Completed => new Color(0.4f, 0.95f, 0.5f),
                QuestStatus.Claimed   => Color.gray,
                QuestStatus.Expired   => new Color(1f, 0.4f, 0.4f),
                _                     => GUI.contentColor,
            };
            var prevCol = GUI.contentColor;
            GUI.contentColor = titleCol;
            EditorGUILayout.LabelField($"{StatusIcon(status)}  {def.DisplayName ?? def.QuestID}",
                EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
            GUI.contentColor = prevCol;

            if (GUILayout.Button("Copy ID", S.SmallBtn, GUILayout.Width(62), GUILayout.Height(18)))
            {
                EditorGUIUtility.systemCopyBuffer = def.QuestID;
                Log($"Copied: {def.QuestID}", true);
            }

            // Claim button (right side of title)
            if (status == QuestStatus.Completed)
            {
                string qId = def.QuestID;
                var prevBg = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0.3f, 0.8f, 0.4f);
                if (GUILayout.Button("Claim ✓", GUILayout.Width(64), GUILayout.Height(20)))
                    Fire(ClaimQuest(qId, cycleID));
                GUI.backgroundColor = prevBg;
            }

            EditorGUILayout.EndHorizontal();

            // ── Objectives ──
            if (def.Objectives?.Count > 0)
            {
                EditorGUILayout.Space(2);
                foreach (var obj in def.Objectives.Values)
                {
                    long cur    = prog?.Objectives?.GetValueOrDefault(obj.ObjectiveID)?.CurrentValue ?? 0;
                    bool done   = prog?.Objectives?.GetValueOrDefault(obj.ObjectiveID)?.Completed ?? false;
                    long target = obj.TargetValue;
                    float pct   = target > 0 ? Mathf.Clamp01((float)cur / target) : 0f;

                    string amtKey    = def.QuestID + ":" + obj.ObjectiveID + ":amt";
                    string metricKey = def.QuestID + ":" + obj.ObjectiveID + ":metric";

                    if (!_amountInputs.ContainsKey(amtKey))
                        _amountInputs[amtKey] = "1";
                    if (!_amountInputs.ContainsKey(metricKey))
                        _amountInputs[metricKey] = !string.IsNullOrEmpty(obj.MetricID)
                            ? obj.MetricID
                            : obj.ObjectiveID;

                    // Row 1: progress bar + objective label
                    EditorGUILayout.BeginHorizontal();
                    Rect barRect = GUILayoutUtility.GetRect(100, 16, GUILayout.Width(100));
                    DrawProgressBar(barRect, pct, $"{cur}/{target}", done);
                    var prevC = GUI.contentColor;
                    GUI.contentColor = done ? Color.gray : GUI.contentColor;
                    GUILayout.Label(obj.ObjectiveID, EditorStyles.miniLabel, GUILayout.ExpandWidth(true));
                    GUI.contentColor = prevC;
                    if (done)
                    {
                        prevC = GUI.contentColor;
                        GUI.contentColor = new Color(0.4f, 0.9f, 0.5f);
                        GUILayout.Label("✓ done", EditorStyles.miniLabel, GUILayout.Width(46));
                        GUI.contentColor = prevC;
                    }
                    EditorGUILayout.EndHorizontal();

                    // Row 2: source badge + metric input + action buttons
                    if (!done)
                    {
                        bool canProgress = obj.Source == QuestObjectiveSource.ClientApi;
                        long left = Math.Max(1L, target - cur);

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(4);

                        // Source badge
                        var badgeCol = canProgress
                            ? new Color(0.25f, 0.75f, 0.35f)
                            : new Color(0.7f, 0.4f, 0.15f);
                        var prevBg = GUI.backgroundColor;
                        GUI.backgroundColor = badgeCol;
                        GUILayout.Label(obj.Source.ToString(), S.SmallBtn, GUILayout.Width(82), GUILayout.Height(17));
                        GUI.backgroundColor = prevBg;

                        if (canProgress)
                        {
                            GUILayout.Label("metric:", EditorStyles.miniLabel, GUILayout.Width(42));
                            _amountInputs[metricKey] = EditorGUILayout.TextField(
                                _amountInputs[metricKey], S.MetricLabel, GUILayout.Width(90));
                            if (GUILayout.Button("⎘", S.SmallBtn, GUILayout.Width(18), GUILayout.Height(17)))
                            {
                                EditorGUIUtility.systemCopyBuffer = _amountInputs[metricKey];
                                Log($"Copied: {_amountInputs[metricKey]}", true);
                            }

                            GUILayout.Label("val:", EditorStyles.miniLabel, GUILayout.Width(26));
                            _amountInputs[amtKey] = EditorGUILayout.TextField(
                                _amountInputs[amtKey], GUILayout.Width(40));

                            string metric = _amountInputs[metricKey];

                            if (GUILayout.Button("+N", S.SmallBtn, GUILayout.Width(28), GUILayout.Height(17)))
                            {
                                if (long.TryParse(_amountInputs[amtKey], out long n) && n > 0)
                                    Fire(AddProgress(metric, n));
                                else
                                    Log("Invalid amount.", false);
                            }
                            if (GUILayout.Button("+1", S.SmallBtn, GUILayout.Width(26), GUILayout.Height(17)))
                                Fire(AddProgress(metric, 1));
                            if (GUILayout.Button("Done", S.SmallBtn, GUILayout.Width(38), GUILayout.Height(17)))
                                Fire(AddProgress(metric, left));
                        }
                        else
                        {
                            var prevHint = GUI.contentColor;
                            GUI.contentColor = new Color(1f, 0.65f, 0.2f);
                            GUILayout.Label("Set Source = ClientApi in server config to enable progress buttons",
                                EditorStyles.miniLabel);
                            GUI.contentColor = prevHint;
                        }

                        EditorGUILayout.EndHorizontal();
                    }
                }   // foreach objective
            }       // if Objectives

            EditorGUILayout.EndVertical();
        }

        // ── Permanent quests ─────────────────────────────────────────────

        private void DrawPermanentQuests(UserQuestState state, QuestDefinitions config)
        {
            var perms = config.Quests?.Values
                .Where(q => q.CycleIDs == null || q.CycleIDs.Count == 0)
                .OrderBy(q => q.SortOrder).ToList();

            if (perms == null || perms.Count == 0) return;

            const string key = "__permanent__";
            _foldouts.TryGetValue(key, out bool open);
            _foldouts[key] = EditorGUILayout.Foldout(open, $"  Permanent Quests  ({perms.Count})", true, EditorStyles.foldoutHeader);

            if (!_foldouts[key]) return;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(12);
            if (GUILayout.Button("Claim All", GUILayout.Width(80), GUILayout.Height(20)))
                Fire(ClaimAllPermanent(perms, state));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(2);

            foreach (var def in perms)
            {
                UserQuestProgress prog = null;
                state.PermanentQuests?.TryGetValue(def.QuestID, out prog);
                DrawQuestRow(def, prog, null);
            }
            EditorGUILayout.Space(6);
        }

        // ── Async actions ─────────────────────────────────────────────────

        private async Task RefreshState()
        {
            Log("Refreshing...", true);
            var r = await QuestService.GetUserQuestState(autoRefreshCycles: true);
            Log(r.Success ? "State refreshed." : $"Error: {r.Error}", r.Success);
        }

        private async Task RefreshCycles()
        {
            Log("Refreshing cycles...", true);
            var r = await QuestService.RefreshQuestCycles();
            Log(r.Success ? "Cycles refreshed." : $"Error: {r.Error}", r.Success);
        }

        private async Task AddProgress(string metricID, long amount)
        {
            var r = await QuestService.AddQuestProgress(metricID, amount);
            Log(r.Success
                ? $"+{amount} → '{metricID}'   {r.Data?.Updates?.Count ?? 0} objective(s) updated"
                : $"Error: {r.Error}", r.Success);
        }

        private async Task ClaimQuest(string questID, string cycleID)
        {
            var r = await QuestService.ClaimQuestReward(questID, cycleID);
            Log(r.Success ? $"✓ Claimed: {questID}" : $"Error: {r.Error}", r.Success);
        }

        private async Task ClaimMilestone(string cycleID, string milestoneID)
        {
            var r = await QuestService.ClaimMilestoneReward(cycleID, milestoneID);
            Log(r.Success ? $"✓ Milestone claimed: {milestoneID}" : $"Error: {r.Error}", r.Success);
        }

        private async Task CompleteAll(List<QuestDefinition> quests, UserQuestCycleState cycle)
        {
            Log("Completing all active quests...", true);
            int count = 0;
            foreach (var def in quests)
            {
                cycle.Quests.TryGetValue(def.QuestID, out var prog);
                if (prog?.Status != QuestStatus.Active && prog != null) continue;

                foreach (var obj in def.Objectives?.Values ?? Enumerable.Empty<QuestObjectiveDefinition>())
                {
                    if (string.IsNullOrEmpty(obj.MetricID)) continue;
                    long cur  = prog?.Objectives?.GetValueOrDefault(obj.ObjectiveID)?.CurrentValue ?? 0;
                    long left = Math.Max(0L, obj.TargetValue - cur);
                    if (left <= 0) continue;
                    await QuestService.AddQuestProgress(obj.MetricID, left);
                    count++;
                }
            }
            Log($"Sent progress for {count} objective(s). Refresh to see result.", true);
        }

        private async Task ClaimAll(List<QuestDefinition> quests, string cycleID)
        {
            Log("Claiming all completed quests...", true);
            int count = 0;
            foreach (var def in quests)
            {
                var state = IDosGamesData.User.State?.Quest;
                var cycle = state?.Cycles?.GetValueOrDefault(cycleID);
                UserQuestProgress prog = null;
                cycle?.Quests.TryGetValue(def.QuestID, out prog);
                if (prog?.Status != QuestStatus.Completed) continue;
                var r = await QuestService.ClaimQuestReward(def.QuestID, cycleID);
                if (r.Success) count++;
            }
            Log($"Claimed {count} quest(s).", count > 0);
        }

        private async Task ClaimAllPermanent(List<QuestDefinition> defs, UserQuestState state)
        {
            Log("Claiming all completed permanent quests...", true);
            int count = 0;
            foreach (var def in defs)
            {
                UserQuestProgress prog = null;
                state.PermanentQuests?.TryGetValue(def.QuestID, out prog);
                if (prog?.Status != QuestStatus.Completed) continue;
                var r = await QuestService.ClaimQuestReward(def.QuestID, null);
                if (r.Success) count++;
            }
            Log($"Claimed {count} permanent quest(s).", count > 0);
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
                var col  = done ? new Color(0.25f, 0.6f, 0.3f) : new Color(0.2f, 0.55f, 0.85f);
                EditorGUI.DrawRect(fill, col);
            }
            GUI.Label(rect, label, new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal    = { textColor = Color.white },
            });
        }

        private static string StatusIcon(QuestStatus s) => s switch
        {
            QuestStatus.Completed => "●",
            QuestStatus.Claimed   => "✓",
            QuestStatus.Expired   => "✗",
            _                     => "○",
        };

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
