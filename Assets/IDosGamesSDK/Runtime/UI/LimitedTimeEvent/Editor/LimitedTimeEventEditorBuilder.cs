#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IDosGames.UI.LimitedTimeEvent.Editor
{
    /// <summary>
    /// Builds the entire LimitedTimeEvent UI hierarchy in the current scene.
    /// Run via: iDos Games → UI → Build LimitedTimeEvent UI
    /// </summary>
    public static class LimitedTimeEventEditorBuilder
    {
        private const string ROOT_NAME = "LimitedTimeEventScreen";

        [MenuItem("iDos Games/UI/Build LimitedTimeEvent UI")]
        public static void Build()
        {
            if (!EditorUtility.DisplayDialog(
                    "Build LimitedTimeEvent UI",
                    "This will create the full LimitedTimeEvent UI hierarchy in the active scene.\nContinue?",
                    "Build", "Cancel"))
                return;

            var canvas = GetOrCreateCanvas();
            var root   = CreateRoot(canvas.transform);

            BuildBackground(root);
            BuildHeader(root);
            BuildProgressBar(root);
            BuildColumnLabels(root);
            BuildScrollView(root);
            BuildBonusWindowBanner(root);
            BuildStateViews(root);

            // Attach screen script
            if (root.GetComponent<LimitedTimeEventScreen>() == null)
                root.AddComponent<LimitedTimeEventScreen>();

            Selection.activeGameObject = root;
            EditorUtility.SetDirty(root);
            Debug.Log("[LimitedTimeEvent] UI hierarchy built successfully.");
        }

        // ─── Canvas ───────────────────────────────────────────────────────────

        private static Canvas GetOrCreateCanvas()
        {
            var existing = Object.FindObjectOfType<Canvas>();
            if (existing != null) return existing;

            var go = new GameObject("Canvas");
            var c  = go.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            go.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            go.AddComponent<GraphicRaycaster>();
            return c;
        }

        // ─── Root ──────────────────────────────────────────────────────────────

        private static GameObject CreateRoot(Transform parent)
        {
            var go = CreateUIObject(ROOT_NAME, parent);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            var img = go.AddComponent<Image>();
            img.color = new Color(0.18f, 0.12f, 0.28f); // Deep purple BG
            return go;
        }

        // ─── Background gradient panel ────────────────────────────────────────

        private static void BuildBackground(GameObject root)
        {
            var bg = CreateUIObject("Background", root.transform);
            SetAnchors(bg, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var img = bg.AddComponent<Image>();
            img.color = new Color(0.15f, 0.10f, 0.25f, 1f);
        }

        // ─── Header ───────────────────────────────────────────────────────────

        private static void BuildHeader(GameObject root)
        {
            // Header container (top 200px)
            var header = CreateUIObject("Header", root.transform);
            SetAnchors(header, new Vector2(0, 0.8f), Vector2.one, Vector2.zero, Vector2.zero);

            // Banner background image
            var banner = CreateUIObject("BannerImage", header.transform);
            SetAnchors(banner, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            banner.AddComponent<Image>().color = new Color(0.35f, 0.18f, 0.08f);

            // Title text
            var titleGO = CreateUIObject("EventTitle", header.transform);
            SetAnchors(titleGO, new Vector2(0.1f, 0.55f), new Vector2(0.75f, 0.90f), Vector2.zero, Vector2.zero);
            var title = titleGO.AddComponent<TextMeshProUGUI>();
            title.text      = "Battle Pass";
            title.fontSize  = 36;
            title.fontStyle = FontStyles.Bold;
            title.alignment = TextAlignmentOptions.Center;
            title.color     = new Color(1f, 0.85f, 0.2f);

            // Timer
            var timerGO = CreateUIObject("Timer", header.transform);
            SetAnchors(timerGO, new Vector2(0.25f, 0.25f), new Vector2(0.75f, 0.55f), Vector2.zero, Vector2.zero);
            var timer = timerGO.AddComponent<TextMeshProUGUI>();
            timer.text     = "⏱ 8D 18H";
            timer.fontSize = 22;
            timer.alignment = TextAlignmentOptions.Center;
            timer.color    = Color.white;

            // Activate button (top right)
            var activateGO = CreateButton("ActivatePassButton", header.transform,
                "Activate!", new Color(0.92f, 0.72f, 0.05f));
            SetAnchors(activateGO, new Vector2(0.78f, 0.60f), new Vector2(0.98f, 0.95f), Vector2.zero, Vector2.zero);

            // Help button (top left)
            var helpGO = CreateButton("HelpButton", header.transform, "?", new Color(0.3f, 0.3f, 0.5f));
            SetAnchors(helpGO, new Vector2(0.02f, 0.65f), new Vector2(0.12f, 0.95f), Vector2.zero, Vector2.zero);
        }

        // ─── Progress Bar ──────────────────────────────────────────────────────

        private static void BuildProgressBar(GameObject root)
        {
            var bar = CreateUIObject("ProgressBar", root.transform);
            SetAnchors(bar, new Vector2(0, 0.72f), new Vector2(1, 0.80f), new Vector2(20, 0), new Vector2(-20, 0));

            // BG panel
            var bg = bar.AddComponent<Image>();
            bg.color = new Color(0.22f, 0.15f, 0.35f);

            // Token icon
            var iconGO = CreateUIObject("TokenIcon", bar.transform);
            SetAnchors(iconGO, Vector2.zero, new Vector2(0.08f, 1f), Vector2.zero, Vector2.zero);
            iconGO.AddComponent<Image>().color = Color.white;

            // Slider
            var sliderGO = CreateUIObject("TokenSlider", bar.transform);
            SetAnchors(sliderGO, new Vector2(0.09f, 0.1f), new Vector2(0.72f, 0.9f), Vector2.zero, Vector2.zero);
            var slider = sliderGO.AddComponent<Slider>();
            slider.minValue = 0; slider.maxValue = 1; slider.value = 0.15f;

            // Create slider fill area child
            var fillArea = CreateUIObject("Fill Area", sliderGO.transform);
            SetAnchors(fillArea, new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            var fill = CreateUIObject("Fill", fillArea.transform);
            SetAnchors(fill, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var fillImg = fill.AddComponent<Image>();
            fillImg.color = new Color(1f, 0.75f, 0f);
            slider.fillRect = fill.GetComponent<RectTransform>();

            // Progress text
            var progressTextGO = CreateUIObject("ProgressText", bar.transform);
            SetAnchors(progressTextGO, new Vector2(0.09f, 0f), new Vector2(0.72f, 1f), Vector2.zero, Vector2.zero);
            var pt = progressTextGO.AddComponent<TextMeshProUGUI>();
            pt.text      = "30 / 200";
            pt.fontSize  = 20;
            pt.alignment = TextAlignmentOptions.Center;
            pt.color     = Color.white;

            // Next milestone label
            var nextGO = CreateUIObject("NextMilestoneLabel", bar.transform);
            SetAnchors(nextGO, new Vector2(0.73f, 0f), Vector2.one, Vector2.zero, Vector2.zero);
            var nl = nextGO.AddComponent<TextMeshProUGUI>();
            nl.text      = "Next: 200";
            nl.fontSize  = 16;
            nl.alignment = TextAlignmentOptions.Center;
            nl.color     = new Color(1f, 0.85f, 0.2f);
        }

        // ─── Column Labels ────────────────────────────────────────────────────

        private static void BuildColumnLabels(GameObject root)
        {
            var labelsBar = CreateUIObject("ColumnLabels", root.transform);
            SetAnchors(labelsBar, new Vector2(0, 0.67f), new Vector2(1, 0.72f), new Vector2(10, 0), new Vector2(-10, 0));

            var freeLabel = CreateUIObject("FreeLabelBG", labelsBar.transform);
            SetAnchors(freeLabel, Vector2.zero, new Vector2(0.42f, 1f), Vector2.zero, Vector2.zero);
            freeLabel.AddComponent<Image>().color = new Color(0.4f, 0.4f, 0.5f);
            var freeText = CreateUIObject("FreeLabelText", freeLabel.transform);
            SetAnchors(freeText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var ft = freeText.AddComponent<TextMeshProUGUI>();
            ft.text = "FREE"; ft.fontSize = 22; ft.fontStyle = FontStyles.Bold;
            ft.alignment = TextAlignmentOptions.Center; ft.color = Color.white;

            var passLabel = CreateUIObject("PassLabelBG", labelsBar.transform);
            SetAnchors(passLabel, new Vector2(0.58f, 0f), Vector2.one, Vector2.zero, Vector2.zero);
            passLabel.AddComponent<Image>().color = new Color(0.8f, 0.55f, 0.05f);
            var passText = CreateUIObject("PassLabelText", passLabel.transform);
            SetAnchors(passText, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var ps = passText.AddComponent<TextMeshProUGUI>();
            ps.text = "PASS"; ps.fontSize = 22; ps.fontStyle = FontStyles.Bold;
            ps.alignment = TextAlignmentOptions.Center; ps.color = Color.white;
        }

        // ─── Scroll View ──────────────────────────────────────────────────────

        private static void BuildScrollView(GameObject root)
        {
            var scrollGO = CreateUIObject("MilestonesScrollView", root.transform);
            SetAnchors(scrollGO, Vector2.zero, new Vector2(1, 0.67f), new Vector2(0, 60), new Vector2(0, 0));

            var scrollRect = scrollGO.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;

            // Viewport
            var viewportGO = CreateUIObject("Viewport", scrollGO.transform);
            SetAnchors(viewportGO, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            viewportGO.AddComponent<Image>().color = Color.clear;
            var mask = viewportGO.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            scrollRect.viewport  = viewportGO.GetComponent<RectTransform>();

            // Content
            var contentGO = CreateUIObject("Content", viewportGO.transform);
            var contentRT = contentGO.GetComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot     = new Vector2(0.5f, 1);
            contentRT.anchoredPosition = Vector2.zero;
            contentRT.sizeDelta = new Vector2(0, 0);

            var vlg = contentGO.AddComponent<VerticalLayoutGroup>();
            vlg.spacing           = 8;
            vlg.padding           = new RectOffset(10, 10, 10, 10);
            vlg.childAlignment    = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight= false;
            vlg.childForceExpandWidth = true;

            var csf = contentGO.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.content = contentRT;

            // Build a sample MilestoneRow prefab placeholder
            BuildSampleMilestoneRow(contentGO.transform);
        }

        private static void BuildSampleMilestoneRow(Transform parent)
        {
            var row = CreateUIObject("MilestoneRow_PREFAB_SAMPLE", parent);
            var rowRT = row.GetComponent<RectTransform>();
            rowRT.sizeDelta = new Vector2(0, 160);

            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing             = 0;
            hlg.childAlignment      = TextAnchor.MiddleCenter;
            hlg.childControlHeight  = true;
            hlg.childControlWidth   = false;
            hlg.childForceExpandHeight = true;

            // ─ Free Card ─
            var freeCard = BuildRewardCardPlaceholder("FreeCard", row.transform, new Color(0.35f, 0.28f, 0.52f));
            freeCard.GetComponent<LayoutElement>().preferredWidth = 150;

            // ─ Center badge ─
            var center = CreateUIObject("MilestoneBadge", row.transform);
            center.AddComponent<LayoutElement>().preferredWidth = 60;
            var badgeImg = center.AddComponent<Image>();
            badgeImg.color = new Color(0.8f, 0.55f, 0.05f);
            var badgeRt = center.GetComponent<RectTransform>();
            badgeRt.sizeDelta = new Vector2(60, 60);

            var numGO = CreateUIObject("NumberText", center.transform);
            SetAnchors(numGO, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var numT = numGO.AddComponent<TextMeshProUGUI>();
            numT.text = "1"; numT.fontSize = 22; numT.fontStyle = FontStyles.Bold;
            numT.alignment = TextAlignmentOptions.Center; numT.color = Color.white;

            // ─ Pass Card ─
            var passCard = BuildRewardCardPlaceholder("PassCard", row.transform, new Color(0.45f, 0.33f, 0.08f));
            passCard.GetComponent<LayoutElement>().preferredWidth = 150;

            // Attach MilestoneRowView
            row.AddComponent<MilestoneRowView>();
        }

        private static GameObject BuildRewardCardPlaceholder(string name, Transform parent, Color bg)
        {
            var card = CreateUIObject(name, parent);
            var le   = card.AddComponent<LayoutElement>();
            le.preferredWidth  = 150;
            le.preferredHeight = 150;

            var img = card.AddComponent<Image>();
            img.color = bg;

            var inner = CreateUIObject("RewardIcon", card.transform);
            SetAnchors(inner, new Vector2(0.15f, 0.3f), new Vector2(0.85f, 0.9f), Vector2.zero, Vector2.zero);
            inner.AddComponent<Image>().color = new Color(1,1,1,0.3f);

            var amtGO = CreateUIObject("AmountText", card.transform);
            SetAnchors(amtGO, new Vector2(0, 0f), new Vector2(1, 0.3f), Vector2.zero, Vector2.zero);
            var amt = amtGO.AddComponent<TextMeshProUGUI>();
            amt.text = "100"; amt.fontSize = 20; amt.fontStyle = FontStyles.Bold;
            amt.alignment = TextAlignmentOptions.Center; amt.color = Color.white;

            // Claimed overlay
            var claimed = CreateUIObject("ClaimedOverlay", card.transform);
            SetAnchors(claimed, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var claimedImg = claimed.AddComponent<Image>();
            claimedImg.color = new Color(0, 0.6f, 0.2f, 0.7f);
            claimed.SetActive(false);

            // Locked overlay
            var locked = CreateUIObject("LockedOverlay", card.transform);
            SetAnchors(locked, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            locked.AddComponent<Image>().color = new Color(0, 0, 0, 0.5f);
            locked.SetActive(false);

            // VIP locked overlay
            var vipLocked = CreateUIObject("VipLockedOverlay", card.transform);
            SetAnchors(vipLocked, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            vipLocked.AddComponent<Image>().color = new Color(0, 0, 0, 0.7f);
            vipLocked.SetActive(false);

            // Claim button
            var claimBtn = CreateButton("ClaimButton", card.transform, "Claim", new Color(0.2f, 0.75f, 0.35f));
            SetAnchors(claimBtn, new Vector2(0.05f, 0.02f), new Vector2(0.95f, 0.28f), Vector2.zero, Vector2.zero);
            claimBtn.SetActive(false);

            card.AddComponent<RewardCardView>();
            return card;
        }

        // ─── Bonus Window Banner ──────────────────────────────────────────────

        private static void BuildBonusWindowBanner(GameObject root)
        {
            var banner = CreateUIObject("BonusWindowBanner", root.transform);
            SetAnchors(banner, new Vector2(0, 0.62f), new Vector2(1, 0.67f), new Vector2(10, 0), new Vector2(-10, 0));
            banner.AddComponent<Image>().color = new Color(0.9f, 0.5f, 0.05f);
            banner.SetActive(false);

            var textGO = CreateUIObject("BonusText", banner.transform);
            SetAnchors(textGO, Vector2.zero, Vector2.one, new Vector2(10, 0), new Vector2(-10, 0));
            var t = textGO.AddComponent<TextMeshProUGUI>();
            t.text = "🔥 Bonus x2.0 active!"; t.fontSize = 20;
            t.alignment = TextAlignmentOptions.Center; t.color = Color.white;
        }

        // ─── State Views ──────────────────────────────────────────────────────

        private static void BuildStateViews(GameObject root)
        {
            // Loading
            var loading = CreateUIObject("LoadingView", root.transform);
            SetAnchors(loading, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            loading.AddComponent<Image>().color = new Color(0, 0, 0, 0.5f);
            var loadText = CreateUIObject("LoadingText", loading.transform);
            SetAnchors(loadText, new Vector2(0.3f, 0.4f), new Vector2(0.7f, 0.6f), Vector2.zero, Vector2.zero);
            var lt = loadText.AddComponent<TextMeshProUGUI>();
            lt.text = "Loading..."; lt.fontSize = 28; lt.alignment = TextAlignmentOptions.Center;
            lt.color = Color.white;
            loading.SetActive(true); // visible by default, screen hides after load

            // Empty State
            var empty = CreateUIObject("EmptyStateView", root.transform);
            SetAnchors(empty, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            empty.AddComponent<Image>().color = new Color(0, 0, 0, 0.3f);
            var emptyText = CreateUIObject("EmptyText", empty.transform);
            SetAnchors(emptyText, new Vector2(0.1f, 0.4f), new Vector2(0.9f, 0.6f), Vector2.zero, Vector2.zero);
            var et = emptyText.AddComponent<TextMeshProUGUI>();
            et.text = "No active events"; et.fontSize = 24; et.alignment = TextAlignmentOptions.Center;
            et.color = new Color(0.7f, 0.7f, 0.8f);
            empty.SetActive(false);

            // Content View — wraps everything except states
            // (In practice, the editor assigns content via the Screen script serialized fields)
        }

        // ─── Helpers ──────────────────────────────────────────────────────────

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static GameObject CreateButton(string name, Transform parent, string label, Color color)
        {
            var go  = CreateUIObject(name, parent);
            var img = go.AddComponent<Image>();
            img.color = color;
            go.AddComponent<Button>();

            var textGO = CreateUIObject("Label", go.transform);
            SetAnchors(textGO, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var t = textGO.AddComponent<TextMeshProUGUI>();
            t.text = label; t.fontSize = 20; t.fontStyle = FontStyles.Bold;
            t.alignment = TextAlignmentOptions.Center; t.color = Color.white;

            return go;
        }

        private static void SetAnchors(
            GameObject go,
            Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            var rt         = go.GetComponent<RectTransform>();
            rt.anchorMin   = anchorMin;
            rt.anchorMax   = anchorMax;
            rt.offsetMin   = offsetMin;
            rt.offsetMax   = offsetMax;
        }
    }
}
#endif
