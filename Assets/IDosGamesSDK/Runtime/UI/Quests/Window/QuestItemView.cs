using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace IDosGames.UI.Quest
{
    public class QuestItemView : MonoBehaviour
    {
        [Header("Content")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descText;
        [SerializeField] private Image           _iconImage;

        [Header("Objectives")]
        [SerializeField] private RectTransform      _objectiveContainer; // parent that holds spawned objective rows
        [SerializeField] private QuestObjectiveView _objectiveTemplate;  // inactive template

        [Header("Rewards")]
        [SerializeField] private RectTransform _rewardContainer;    // ItemFrame — holds reward rows
        [SerializeField] private RectTransform _rewardRowTemplate;  // RewardRow_1 — inactive template

        [Header("Visual State")]
        [SerializeField] private UnityEngine.UI.Image _bgInner;
        [SerializeField] private UnityEngine.UI.Image _statusAccent; // left-edge color strip
        [SerializeField] private GameObject           _dim;

        [Header("Status")]
        [SerializeField] private GameObject _completedBadge;
        [SerializeField] private GameObject _claimedBadge;
        [SerializeField] private GameObject _expiredBadge;
        [SerializeField] private TextMeshProUGUI _objectiveSummaryText; // shown only for multi-objective quests

        [Header("Claim Button")]
        [SerializeField] private Button          _claimButton;
        [SerializeField] private TextMeshProUGUI _claimButtonText;

        private Coroutine _pulseCoroutine;

        // ─────────────────────────────────────────────────────────────

        public void Setup(
            string title,
            string description,
            string iconPath,
            QuestStatus status,
            bool canClaim,
            List<ObjectiveUIItem> objectives,
            List<ResourceEntry>   rewards,
            UnityAction onClaimClicked)
        {
            if (_titleText != null)
            {
                _titleText.text  = title;
                _titleText.color = (status == QuestStatus.Completed)
                    ? new Color(0.102f, 0.910f, 0.447f) // #1AE872 green — ready to claim
                    : Color.white;
            }
            if (_descText  != null) _descText.text  = description;

            if (_iconImage != null && !string.IsNullOrEmpty(iconPath))
                LoadIconAsync(iconPath);

            // _completedBadge is wired to BgInner (background), SetVisualState always overrides it.
            // State is communicated via background color + title color + Claim button.
            if (_claimedBadge != null) _claimedBadge.SetActive(status == QuestStatus.Claimed);
            if (_expiredBadge != null) _expiredBadge.SetActive(status == QuestStatus.Expired);

            if (_claimButton != null)
            {
                _claimButton.interactable = canClaim;
                _claimButton.gameObject.SetActive(status == QuestStatus.Completed);

                _claimButton.onClick.RemoveAllListeners();
                if (canClaim && onClaimClicked != null)
                    _claimButton.onClick.AddListener(onClaimClicked);

                if (_pulseCoroutine != null) StopCoroutine(_pulseCoroutine);
                if (status == QuestStatus.Completed && canClaim)
                    _pulseCoroutine = StartCoroutine(PulseLoop(_claimButton));
                else
                    _claimButton.transform.localScale = Vector3.one;
            }

            PopulateObjectives(objectives);
            PopulateRewards(rewards);

            bool hasProgress = false;
            int completedObjectives = 0;
            if (objectives != null)
                foreach (var obj in objectives)
                {
                    if (obj.Current > 0) hasProgress = true;
                    if (obj.Current >= obj.Target) completedObjectives++;
                }

            if (_objectiveSummaryText != null)
            {
                bool showSummary = objectives != null && objectives.Count > 1 && status == QuestStatus.Active;
                _objectiveSummaryText.gameObject.SetActive(showSummary);
                if (showSummary)
                    _objectiveSummaryText.text = $"{completedObjectives}/{objectives.Count} целей выполнено";
            }

            SetVisualState(status, hasProgress);
        }

        // ─────────────────────────────────────────────────────────────

        private void PopulateObjectives(List<ObjectiveUIItem> objectives)
        {
            if (_objectiveContainer == null || _objectiveTemplate == null) return;

            bool hasObjectives = objectives != null && objectives.Count > 0;
            _objectiveContainer.gameObject.SetActive(hasObjectives);

            // Destroy all non-template children
            for (int i = _objectiveContainer.childCount - 1; i >= 0; i--)
            {
                var child = _objectiveContainer.GetChild(i);
                if (child == _objectiveTemplate.transform) continue;
                DestroyImmediate(child.gameObject);
            }

            if (!hasObjectives) return;

            foreach (var obj in objectives)
            {
                var row = Instantiate(_objectiveTemplate, _objectiveContainer);
                row.gameObject.SetActive(true);
                row.Setup(obj);
            }

            // Rebuild layout
            LayoutRebuilder.ForceRebuildLayoutImmediate(_objectiveContainer);
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
            }

        private void PopulateRewards(List<ResourceEntry> rewards)
        {
            if (_rewardContainer == null || _rewardRowTemplate == null) return;

            bool hasRewards = rewards != null && rewards.Count > 0;
            _rewardContainer.gameObject.SetActive(hasRewards);

            // Destroy all non-template children
            for (int i = _rewardContainer.childCount - 1; i >= 0; i--)
            {
                var child = _rewardContainer.GetChild(i);
                if (child == _rewardRowTemplate) continue;
                DestroyImmediate(child.gameObject);
            }

            if (!hasRewards) return;

            var glg = _rewardContainer.GetComponent<GridLayoutGroup>();
            if (glg != null)
            {
                // We use Flexible constraint in the prefab now.
                // But we can still nudge it for very large or very small counts.
                if (rewards.Count == 1)
                {
                    glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                    glg.constraintCount = 1;
                }
                else
                {
                    glg.constraint = GridLayoutGroup.Constraint.Flexible;
                }
            }

            foreach (var reward in rewards)
            {
                var row = Instantiate(_rewardRowTemplate, _rewardContainer);
                row.gameObject.SetActive(true);

                var icon = row.GetComponentInChildren<Image>(true);
                var text = row.GetComponentInChildren<TextMeshProUGUI>(true);

                if (text != null)
                {
                    bool isCurrency = reward.Type == null || reward.Type == ResourceEntryType.VirtualCurrency;
                    text.text = isCurrency
                        ? $"+{reward.Amount}"
                        : (reward.ItemID ?? reward.CurrencyID ?? $"+{reward.Amount}");
                }
            }

            // Rebuild layout to ensure everything fits perfectly
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rewardContainer);
            // Also rebuild parent to account for reward container height changes
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
        }

        // ─────────────────────────────────────────────────────────────

        private async void LoadIconAsync(string path)
        {
            try
            {
                var sprite = await ImageLoader.GetSpriteAsync(path);
                if (this != null && _iconImage != null && sprite != null)
                    _iconImage.sprite = sprite;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[QuestItemView] Icon load failed: {ex.Message}");
            }
        }

        private void OnDisable()
        {
            if (_pulseCoroutine != null)
            {
                StopCoroutine(_pulseCoroutine);
                _pulseCoroutine = null;
            }
            if (_claimButton != null)
            {
                _claimButton.transform.localScale = Vector3.one;
                var cb = _claimButton.colors;
                cb.normalColor = Color.white;
                _claimButton.colors = cb;
            }
        }

        private static IEnumerator PulseLoop(Button btn)
        {
            const float speed    = 1.5f;
            const float minScale = 1.00f;
            const float maxScale = 1.12f;

            Transform target    = btn.transform;
            Color     baseColor = btn.colors.normalColor;
            Color     glowColor = Color.Lerp(baseColor, Color.white, 0.55f);

            float t = 0f;
            while (true)
            {
                t += Time.deltaTime * speed;
                float ping = Mathf.PingPong(t, 1f);
                float s    = Mathf.SmoothStep(minScale, maxScale, ping);
                target.localScale = new Vector3(s, s, 1f);

                var cb = btn.colors;
                cb.normalColor = Color.Lerp(baseColor, glowColor, ping);
                btn.colors = cb;

                yield return null;
            }
        }

        // ── Colors ───────────────────────────────────────────────────────
        private static readonly Color BgActive      = new Color(0x10 / 255f, 0x1C / 255f, 0x38 / 255f, 1f);
        private static readonly Color BgInProgress  = new Color(0x12 / 255f, 0x22 / 255f, 0x46 / 255f, 1f);
        private static readonly Color BgCompleted   = new Color(0x11 / 255f, 0x6B / 255f, 0x34 / 255f, 1f);
        private static readonly Color BgClaimed     = new Color(0x0A / 255f, 0x0E / 255f, 0x1A / 255f, 1f);
        private static readonly Color BgExpired     = new Color(0x12 / 255f, 0x12 / 255f, 0x1C / 255f, 1f);

        private static readonly Color AccentCompleted  = new Color(0x1A / 255f, 0xE8 / 255f, 0x72 / 255f, 1f); // bright green
        private static readonly Color AccentInProgress = new Color(0x31 / 255f, 0x81 / 255f, 1f,          1f); // blue
        private static readonly Color AccentActive     = new Color(0x28 / 255f, 0x38 / 255f, 0x60 / 255f, 1f); // subtle navy
        private static readonly Color AccentNone       = new Color(0, 0, 0, 0);

        private void SetVisualState(QuestStatus status, bool hasProgress = false)
        {
            bool dimmed = status == QuestStatus.Claimed || status == QuestStatus.Expired;
            if (_dim != null) _dim.SetActive(dimmed);

            if (_bgInner != null)
            {
                _bgInner.gameObject.SetActive(true);
                _bgInner.color = status switch
                {
                    QuestStatus.Completed => BgCompleted,
                    QuestStatus.Claimed   => BgClaimed,
                    QuestStatus.Expired   => BgExpired,
                    QuestStatus.Active    => hasProgress ? BgInProgress : BgActive,
                    _                     => BgActive,
                };
            }

            if (_statusAccent != null)
            {
                _statusAccent.color = status switch
                {
                    QuestStatus.Completed => AccentCompleted,
                    QuestStatus.Claimed   => AccentNone,
                    QuestStatus.Expired   => AccentNone,
                    QuestStatus.Active    => hasProgress ? AccentInProgress : AccentActive,
                    _                     => AccentActive,
                };
            }
        }
    }
}
