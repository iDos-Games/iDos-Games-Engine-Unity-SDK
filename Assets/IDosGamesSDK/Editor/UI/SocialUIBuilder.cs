// File: Assets/IDosGamesSDK/Editor/UI/Social/SocialUIBuilder.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IDosGames.UI.Social;

namespace IDosGames.UI.Social.EditorTools
{
    public class SocialUIBuilder : EditorWindow
    {
        private static readonly Color BG_DARK = new Color(0.12f, 0.12f, 0.18f, 1f);
        private static readonly Color BG_CARD = new Color(0.22f, 0.22f, 0.32f, 1f);
        private static readonly Color BG_HEADER = new Color(0.10f, 0.10f, 0.16f, 1f);
        private static readonly Color TAB_ACTIVE = new Color(0.35f, 0.55f, 0.95f, 1f);
        private static readonly Color TAB_INACTIVE = new Color(0.25f, 0.25f, 0.35f, 1f);
        private static readonly Color BTN_PRIMARY = new Color(0.30f, 0.65f, 0.45f, 1f);
        private static readonly Color BTN_DANGER = new Color(0.75f, 0.25f, 0.25f, 1f);
        private static readonly Color BTN_SECONDARY = new Color(0.45f, 0.45f, 0.55f, 1f);
        private static readonly Color TEXT_WHITE = Color.white;
        private static readonly Color TEXT_GRAY = new Color(0.65f, 0.65f, 0.70f, 1f);

        private const float SCREEN_WIDTH = 1080f;
        private const float SCREEN_HEIGHT = 1920f;

        [MenuItem("iDos Games/UI/Build Social UI")]
        public static void ShowWindow()
        {
            GetWindow<SocialUIBuilder>("Social UI Builder");
        }

        private void OnGUI()
        {
            GUILayout.Space(20);
            GUILayout.Label("Social UI Builder", EditorStyles.boldLabel);
            GUILayout.Space(10);
            GUILayout.Label("Creates a complete Social UI hierarchy\nin the active scene as a new Canvas.", EditorStyles.wordWrappedLabel);
            GUILayout.Space(20);

            if (GUILayout.Button("Build Social UI", GUILayout.Height(50)))
            {
                BuildUI();
            }

            GUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "After building:\n" +
                "1) Assign default avatar sprites on FriendItemView prefabs\n" +
                "2) Assign event icons on TimelineItemView prefab\n" +
                "3) Optionally save as Prefab by dragging to Project",
                MessageType.Info);
        }

        private void BuildUI()
        {
            var rootGO = new GameObject("SocialUI_Canvas");
            Undo.RegisterCreatedObjectUndo(rootGO, "Build Social UI");

            var canvas = rootGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = rootGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(SCREEN_WIDTH, SCREEN_HEIGHT);
            scaler.matchWidthOrHeight = 0.5f;

            rootGO.AddComponent<GraphicRaycaster>();

            var screenGO = CreatePanel("SocialScreen", rootGO.transform, BG_DARK, stretch: true);
            var screenComp = screenGO.AddComponent<SocialScreen>();

            // ============ HEADER ============
            var header = CreatePanel("Header", screenGO.transform, BG_HEADER);
            SetAnchors(header, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            header.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 120);

            var titleText = CreateTMPText("TitleText", header.transform, "Social", 42, TEXT_WHITE, TextAlignmentOptions.MidlineLeft);
            var titleRT = titleText.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 0);
            titleRT.anchorMax = new Vector2(0.7f, 1);
            titleRT.offsetMin = new Vector2(40, 0);
            titleRT.offsetMax = new Vector2(0, 0);

            var closeBtnGO = CreateButton("CloseButton", header.transform, "X", new Vector2(80, 80), BTN_DANGER);
            var closeBtnRT = closeBtnGO.GetComponent<RectTransform>();
            closeBtnRT.anchorMin = new Vector2(1, 0.5f);
            closeBtnRT.anchorMax = new Vector2(1, 0.5f);
            closeBtnRT.pivot = new Vector2(1, 0.5f);
            closeBtnRT.anchoredPosition = new Vector2(-20, 0);

            var refreshBtnGO = CreateButton("RefreshButton", header.transform, "\u21BB", new Vector2(80, 80), BTN_SECONDARY);
            var refreshBtnRT = refreshBtnGO.GetComponent<RectTransform>();
            refreshBtnRT.anchorMin = new Vector2(1, 0.5f);
            refreshBtnRT.anchorMax = new Vector2(1, 0.5f);
            refreshBtnRT.pivot = new Vector2(1, 0.5f);
            refreshBtnRT.anchoredPosition = new Vector2(-110, 0);

            // ============ TABS BAR ============
            var tabsBar = CreatePanel("TabsBar", screenGO.transform, new Color(0, 0, 0, 0));
            SetAnchors(tabsBar, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1));
            tabsBar.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 100);
            tabsBar.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -120);
            var tabsHLG = tabsBar.AddComponent<HorizontalLayoutGroup>();
            tabsHLG.spacing = 8;
            tabsHLG.padding = new RectOffset(16, 16, 8, 8);
            tabsHLG.childForceExpandWidth = true;
            tabsHLG.childForceExpandHeight = true;

            var tabsComp = tabsBar.AddComponent<SocialTabsView>();

            string[] tabNames = { "Friends", "Requests", "Find", "Timeline" };
            Button[] tabButtons = new Button[4];
            TMP_Text[] tabTexts = new TMP_Text[4];

            for (int i = 0; i < tabNames.Length; i++)
            {
                var tabBtnGO = CreateButton($"Tab_{tabNames[i]}", tabsBar.transform, tabNames[i], Vector2.zero, i == 0 ? TAB_ACTIVE : TAB_INACTIVE, 28);
                tabButtons[i] = tabBtnGO.GetComponent<Button>();
                tabTexts[i] = tabBtnGO.GetComponentInChildren<TMP_Text>();
            }

            var tabsSO = new SerializedObject(tabsComp);
            tabsSO.FindProperty("_friendsTabButton").objectReferenceValue = tabButtons[0];
            tabsSO.FindProperty("_requestsTabButton").objectReferenceValue = tabButtons[1];
            tabsSO.FindProperty("_recommendedTabButton").objectReferenceValue = tabButtons[2];
            tabsSO.FindProperty("_timelineTabButton").objectReferenceValue = tabButtons[3];
            tabsSO.FindProperty("_friendsTabText").objectReferenceValue = tabTexts[0];
            tabsSO.FindProperty("_requestsTabText").objectReferenceValue = tabTexts[1];
            tabsSO.FindProperty("_recommendedTabText").objectReferenceValue = tabTexts[2];
            tabsSO.FindProperty("_timelineTabText").objectReferenceValue = tabTexts[3];
            tabsSO.ApplyModifiedProperties();

            // ============ CONTENT AREA ============
            var contentArea = CreatePanel("ContentArea", screenGO.transform, new Color(0, 0, 0, 0));
            SetAnchors(contentArea, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
            var contentRT = contentArea.GetComponent<RectTransform>();
            contentRT.offsetMin = new Vector2(0, 0);
            contentRT.offsetMax = new Vector2(0, -220);

            // ============ BUILD PANELS ============
            var friendsPanel = BuildListPanel("FriendsPanel", contentArea.transform);
            var friendsPanelComp = friendsPanel.AddComponent<SocialFriendsPanel>();

            var requestsPanel = BuildListPanel("RequestsPanel", contentArea.transform);
            var requestsPanelComp = requestsPanel.AddComponent<SocialRequestsPanel>();

            var recommendedPanel = BuildListPanel("RecommendedPanel", contentArea.transform);
            var recommendedPanelComp = recommendedPanel.AddComponent<SocialRecommendedPanel>();

            var timelinePanel = BuildListPanel("TimelinePanel", contentArea.transform);
            var timelinePanelComp = timelinePanel.AddComponent<SocialTimelinePanel>();

            // ============ BUILD ITEM PREFABS ============
            var friendItemPrefab = BuildFriendItemPrefab("FriendItemPrefab", friendsPanel.transform.Find("ScrollView/Viewport/Content"));
            var friendItemPrefabReq = BuildFriendItemPrefab("FriendItemPrefab", requestsPanel.transform.Find("ScrollView/Viewport/Content"));
            var friendItemPrefabRec = BuildFriendItemPrefab("FriendItemPrefab", recommendedPanel.transform.Find("ScrollView/Viewport/Content"));
            var timelineItemPrefab = BuildTimelineItemPrefab("TimelineItemPrefab", timelinePanel.transform.Find("ScrollView/Viewport/Content"));

            // ============ WIRE PANELS ============
            WireFriendPanel(friendsPanelComp, friendsPanel, friendItemPrefab);
            WireFriendPanel(requestsPanelComp, requestsPanel, friendItemPrefabReq);
            WireFriendPanel(recommendedPanelComp, recommendedPanel, friendItemPrefabRec);
            WireTimelinePanel(timelinePanelComp, timelinePanel, timelineItemPrefab);

            // ============ WIRE SCREEN ============
            var screenSO = new SerializedObject(screenComp);
            screenSO.FindProperty("_closeButton").objectReferenceValue = closeBtnGO.GetComponent<Button>();
            screenSO.FindProperty("_refreshButton").objectReferenceValue = refreshBtnGO.GetComponent<Button>();
            screenSO.FindProperty("_tabsView").objectReferenceValue = tabsComp;
            screenSO.FindProperty("_friendsPanel").objectReferenceValue = friendsPanelComp;
            screenSO.FindProperty("_requestsPanel").objectReferenceValue = requestsPanelComp;
            screenSO.FindProperty("_recommendedPanel").objectReferenceValue = recommendedPanelComp;
            screenSO.FindProperty("_timelinePanel").objectReferenceValue = timelinePanelComp;
            screenSO.ApplyModifiedProperties();

            requestsPanel.SetActive(false);
            recommendedPanel.SetActive(false);
            timelinePanel.SetActive(false);

            Selection.activeGameObject = rootGO;
            EditorGUIUtility.PingObject(rootGO);

            Debug.Log("\u2705 Social UI built successfully! See 'SocialUI_Canvas' in Hierarchy.");
        }

        // ==================================================================================
        // PANEL BUILDER
        // ==================================================================================
        private static GameObject BuildListPanel(string name, Transform parent)
        {
            var panel = CreatePanel(name, parent, new Color(0, 0, 0, 0), stretch: true);

            var scrollViewGO = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
            scrollViewGO.transform.SetParent(panel.transform, false);
            var scrollRT = scrollViewGO.GetComponent<RectTransform>();
            scrollRT.anchorMin = Vector2.zero;
            scrollRT.anchorMax = Vector2.one;
            scrollRT.offsetMin = Vector2.zero;
            scrollRT.offsetMax = Vector2.zero;
            scrollViewGO.GetComponent<Image>().color = new Color(0, 0, 0, 0.01f);
            var scrollRect = scrollViewGO.GetComponent<ScrollRect>();
            scrollRect.horizontal = false;

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scrollViewGO.transform, false);
            var vpRT = viewport.GetComponent<RectTransform>();
            vpRT.anchorMin = Vector2.zero;
            vpRT.anchorMax = Vector2.one;
            vpRT.offsetMin = Vector2.zero;
            vpRT.offsetMax = Vector2.zero;
            viewport.GetComponent<Image>().color = new Color(1, 1, 1, 0.01f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;
            scrollRect.viewport = vpRT;

            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            var contentRT2 = content.GetComponent<RectTransform>();
            contentRT2.anchorMin = new Vector2(0, 1);
            contentRT2.anchorMax = new Vector2(1, 1);
            contentRT2.pivot = new Vector2(0.5f, 1);
            contentRT2.offsetMin = new Vector2(16, 0);
            contentRT2.offsetMax = new Vector2(-16, 0);
            var vlg = content.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 12;
            vlg.padding = new RectOffset(0, 0, 16, 16);
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlHeight = false;
            vlg.childControlWidth = true;
            var csf = content.GetComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scrollRect.content = contentRT2;

            var loadingGO = CreatePanel("LoadingView", panel.transform, new Color(0, 0, 0, 0.3f), stretch: true);
            var loadingText = CreateTMPText("LoadingText", loadingGO.transform, "Loading...", 36, TEXT_WHITE, TextAlignmentOptions.Center);
            var ltRT = loadingText.GetComponent<RectTransform>();
            ltRT.anchorMin = Vector2.zero;
            ltRT.anchorMax = Vector2.one;
            ltRT.offsetMin = Vector2.zero;
            ltRT.offsetMax = Vector2.zero;
            loadingGO.AddComponent<LoadingView>();
            loadingGO.SetActive(false);

            var emptyGO = CreatePanel("EmptyStateView", panel.transform, new Color(0, 0, 0, 0), stretch: true);
            var emptyText = CreateTMPText("EmptyText", emptyGO.transform, "Nothing here yet", 32, TEXT_GRAY, TextAlignmentOptions.Center);
            var etRT = emptyText.GetComponent<RectTransform>();
            etRT.anchorMin = Vector2.zero;
            etRT.anchorMax = Vector2.one;
            etRT.offsetMin = Vector2.zero;
            etRT.offsetMax = Vector2.zero;
            var emptyComp = emptyGO.AddComponent<EmptyStateView>();
            var emptySO = new SerializedObject(emptyComp);
            emptySO.FindProperty("_messageText").objectReferenceValue = emptyText.GetComponent<TMP_Text>();
            emptySO.ApplyModifiedProperties();
            emptyGO.SetActive(false);

            return panel;
        }

        // ==================================================================================
        // FRIEND ITEM PREFAB
        // ==================================================================================
        private static GameObject BuildFriendItemPrefab(string name, Transform parent)
        {
            var card = CreatePanel(name, parent, BG_CARD);
            card.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 160);
            var cardLE = card.AddComponent<LayoutElement>();
            cardLE.minHeight = 160;
            cardLE.preferredHeight = 160;

            var hlg = card.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 16;
            hlg.padding = new RectOffset(16, 16, 16, 16);
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            var avatarGO = new GameObject("Avatar", typeof(RectTransform), typeof(Image));
            avatarGO.transform.SetParent(card.transform, false);
            avatarGO.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
            var avatarImg = avatarGO.GetComponent<Image>();
            avatarImg.color = new Color(0.3f, 0.3f, 0.4f, 1f);
            var avatarLE = avatarGO.AddComponent<LayoutElement>();
            avatarLE.minWidth = 100; avatarLE.minHeight = 100;
            avatarLE.preferredWidth = 100; avatarLE.preferredHeight = 100;

            var infoCol = new GameObject("InfoColumn", typeof(RectTransform), typeof(VerticalLayoutGroup));
            infoCol.transform.SetParent(card.transform, false);
            var infoVlg = infoCol.GetComponent<VerticalLayoutGroup>();
            infoVlg.spacing = 4;
            infoVlg.childForceExpandWidth = true; infoVlg.childForceExpandHeight = false;
            infoVlg.childControlWidth = true; infoVlg.childControlHeight = true;
            infoVlg.childAlignment = TextAnchor.MiddleLeft;
            var infoLE = infoCol.AddComponent<LayoutElement>();
            infoLE.flexibleWidth = 1; infoLE.minHeight = 100;

            var usernameTMP = CreateTMPText("UsernameText", infoCol.transform, "PlayerName", 32, TEXT_WHITE, TextAlignmentOptions.MidlineLeft);
            var levelTMP = CreateTMPText("LevelText", infoCol.transform, "Lv.0", 24, TEXT_GRAY, TextAlignmentOptions.MidlineLeft);
            var powerTMP = CreateTMPText("PowerText", infoCol.transform, "0", 24, TEXT_GRAY, TextAlignmentOptions.MidlineLeft);

            var btnsCol = new GameObject("ButtonsColumn", typeof(RectTransform), typeof(VerticalLayoutGroup));
            btnsCol.transform.SetParent(card.transform, false);
            var btnsVlg = btnsCol.GetComponent<VerticalLayoutGroup>();
            btnsVlg.spacing = 8;
            btnsVlg.childForceExpandWidth = true; btnsVlg.childForceExpandHeight = false;
            btnsVlg.childControlWidth = true; btnsVlg.childControlHeight = true;
            btnsVlg.childAlignment = TextAnchor.MiddleCenter;
            var btnsLE = btnsCol.AddComponent<LayoutElement>();
            btnsLE.minWidth = 180; btnsLE.preferredWidth = 180; btnsLE.minHeight = 100;

            var primaryBtn = CreateButton("PrimaryButton", btnsCol.transform, "Action", new Vector2(160, 50), BTN_PRIMARY, 24);
            var secondaryBtn = CreateButton("SecondaryButton", btnsCol.transform, "Cancel", new Vector2(160, 50), BTN_DANGER, 24);

            var itemComp = card.AddComponent<FriendItemView>();
            var itemSO = new SerializedObject(itemComp);
            itemSO.FindProperty("_avatarImage").objectReferenceValue = avatarImg;
            itemSO.FindProperty("_usernameText").objectReferenceValue = usernameTMP.GetComponent<TMP_Text>();
            itemSO.FindProperty("_levelText").objectReferenceValue = levelTMP.GetComponent<TMP_Text>();
            itemSO.FindProperty("_powerText").objectReferenceValue = powerTMP.GetComponent<TMP_Text>();
            itemSO.FindProperty("_primaryButton").objectReferenceValue = primaryBtn.GetComponent<Button>();
            itemSO.FindProperty("_primaryButtonText").objectReferenceValue = primaryBtn.GetComponentInChildren<TMP_Text>();
            itemSO.FindProperty("_secondaryButton").objectReferenceValue = secondaryBtn.GetComponent<Button>();
            itemSO.FindProperty("_secondaryButtonText").objectReferenceValue = secondaryBtn.GetComponentInChildren<TMP_Text>();
            itemSO.ApplyModifiedProperties();

            card.SetActive(false);
            return card;
        }

        // ==================================================================================
        // TIMELINE ITEM PREFAB
        // ==================================================================================
        private static GameObject BuildTimelineItemPrefab(string name, Transform parent)
        {
            var card = CreatePanel(name, parent, BG_CARD);
            card.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 140);
            var cardLE = card.AddComponent<LayoutElement>();
            cardLE.minHeight = 140; cardLE.preferredHeight = 140;

            var hlg = card.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 16;
            hlg.padding = new RectOffset(16, 16, 16, 16);
            hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = false;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false; hlg.childControlHeight = false;

            var iconGO = new GameObject("EventIcon", typeof(RectTransform), typeof(Image));
            iconGO.transform.SetParent(card.transform, false);
            iconGO.GetComponent<RectTransform>().sizeDelta = new Vector2(64, 64);
            var iconImg = iconGO.GetComponent<Image>();
            iconImg.color = new Color(0.4f, 0.4f, 0.5f, 1f);
            var iconLE = iconGO.AddComponent<LayoutElement>();
            iconLE.minWidth = 64; iconLE.minHeight = 64;
            iconLE.preferredWidth = 64; iconLE.preferredHeight = 64;

            var avatarGO = new GameObject("Avatar", typeof(RectTransform), typeof(Image));
            avatarGO.transform.SetParent(card.transform, false);
            avatarGO.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 80);
            var avatarImg = avatarGO.GetComponent<Image>();
            avatarImg.color = new Color(0.3f, 0.3f, 0.4f, 1f);
            var avatarLE = avatarGO.AddComponent<LayoutElement>();
            avatarLE.minWidth = 80; avatarLE.minHeight = 80;
            avatarLE.preferredWidth = 80; avatarLE.preferredHeight = 80;

            var infoCol = new GameObject("InfoColumn", typeof(RectTransform), typeof(VerticalLayoutGroup));
            infoCol.transform.SetParent(card.transform, false);
            var infoVlg = infoCol.GetComponent<VerticalLayoutGroup>();
            infoVlg.spacing = 4;
            infoVlg.childForceExpandWidth = true; infoVlg.childForceExpandHeight = false;
            infoVlg.childControlWidth = true; infoVlg.childControlHeight = true;
            infoVlg.childAlignment = TextAnchor.MiddleLeft;
            var infoLE = infoCol.AddComponent<LayoutElement>();
            infoLE.flexibleWidth = 1; infoLE.minHeight = 80;

            var actorTMP = CreateTMPText("ActorNameText", infoCol.transform, "PlayerName", 28, TEXT_WHITE, TextAlignmentOptions.MidlineLeft);
            var descTMP = CreateTMPText("DescriptionText", infoCol.transform, "did something", 22, TEXT_GRAY, TextAlignmentOptions.MidlineLeft);
            var timeTMP = CreateTMPText("TimeAgoText", infoCol.transform, "just now", 20, TEXT_GRAY, TextAlignmentOptions.MidlineLeft);

            var amountTMP = CreateTMPText("AmountText", card.transform, "0", 30, new Color(1f, 0.85f, 0.3f, 1f), TextAlignmentOptions.MidlineRight);
            var amountLE = amountTMP.AddComponent<LayoutElement>();
            amountLE.minWidth = 120; amountLE.preferredWidth = 120; amountLE.minHeight = 40;

            var itemComp = card.AddComponent<TimelineItemView>();
            var itemSO = new SerializedObject(itemComp);
            itemSO.FindProperty("_avatarImage").objectReferenceValue = avatarImg;
            itemSO.FindProperty("_actorNameText").objectReferenceValue = actorTMP.GetComponent<TMP_Text>();
            itemSO.FindProperty("_eventDescriptionText").objectReferenceValue = descTMP.GetComponent<TMP_Text>();
            itemSO.FindProperty("_timeAgoText").objectReferenceValue = timeTMP.GetComponent<TMP_Text>();
            itemSO.FindProperty("_amountText").objectReferenceValue = amountTMP.GetComponent<TMP_Text>();
            itemSO.FindProperty("_eventIcon").objectReferenceValue = iconImg;
            itemSO.ApplyModifiedProperties();

            card.SetActive(false);
            return card;
        }

        // ==================================================================================
        // WIRING
        // ==================================================================================
        private static void WireFriendPanel(MonoBehaviour panelComp, GameObject panelGO, GameObject itemPrefab)
        {
            var so = new SerializedObject(panelComp);
            so.FindProperty("_listContent").objectReferenceValue = panelGO.transform.Find("ScrollView/Viewport/Content");
            so.FindProperty("_itemPrefab").objectReferenceValue = itemPrefab.GetComponent<FriendItemView>();
            so.FindProperty("_scrollRect").objectReferenceValue = panelGO.transform.Find("ScrollView").GetComponent<ScrollRect>();
            so.FindProperty("_loadingView").objectReferenceValue = panelGO.transform.Find("LoadingView").GetComponent<LoadingView>();
            so.FindProperty("_emptyView").objectReferenceValue = panelGO.transform.Find("EmptyStateView").GetComponent<EmptyStateView>();
            so.ApplyModifiedProperties();
        }

        private static void WireTimelinePanel(SocialTimelinePanel panelComp, GameObject panelGO, GameObject itemPrefab)
        {
            var so = new SerializedObject(panelComp);
            so.FindProperty("_listContent").objectReferenceValue = panelGO.transform.Find("ScrollView/Viewport/Content");
            so.FindProperty("_itemPrefab").objectReferenceValue = itemPrefab.GetComponent<TimelineItemView>();
            so.FindProperty("_scrollRect").objectReferenceValue = panelGO.transform.Find("ScrollView").GetComponent<ScrollRect>();
            so.FindProperty("_loadingView").objectReferenceValue = panelGO.transform.Find("LoadingView").GetComponent<LoadingView>();
            so.FindProperty("_emptyView").objectReferenceValue = panelGO.transform.Find("EmptyStateView").GetComponent<EmptyStateView>();
            so.ApplyModifiedProperties();
        }

        // ==================================================================================
        // FACTORY HELPERS
        // ==================================================================================
        private static GameObject CreatePanel(string name, Transform parent, Color bgColor, bool stretch = false)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = bgColor;
            if (stretch)
            {
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            }
            return go;
        }

        private static void SetAnchors(GameObject go, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }

        private static GameObject CreateButton(string name, Transform parent, string text, Vector2 size, Color bgColor, int fontSize = 32)
        {
            var btnGO = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            btnGO.transform.SetParent(parent, false);
            btnGO.GetComponent<Image>().color = bgColor;
            if (size != Vector2.zero) btnGO.GetComponent<RectTransform>().sizeDelta = size;
            var le = btnGO.AddComponent<LayoutElement>();
            if (size != Vector2.zero)
            {
                le.minWidth = size.x; le.minHeight = size.y;
                le.preferredWidth = size.x; le.preferredHeight = size.y;
            }
            var textGO = CreateTMPText("Text", btnGO.transform, text, fontSize, TEXT_WHITE, TextAlignmentOptions.Center);
            var textRT = textGO.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero; textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero; textRT.offsetMax = Vector2.zero;
            return btnGO;
        }

        private static GameObject CreateTMPText(string name, Transform parent, string text, int fontSize, Color color, TextAlignmentOptions alignment)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.enableAutoSizing = false;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            return go;
        }
    }
}
#endif
