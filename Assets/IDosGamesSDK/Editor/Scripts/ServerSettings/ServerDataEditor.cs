#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace IDosGames
{
    public class ServerDataEditor : EditorWindow
    {
        [Header("Server Settings")]

        private IDosGamesSDKSettings settings;

        [MenuItem("Tools/iDos Games SDK/1. Server Settings")]
        public static void ShowWindow()
        {
            GetWindow<ServerDataEditor>("Server Settings");
        }

        private void OnEnable()
        {
            settings = IDosGamesSDKSettings.Instance;
        }

        private void OnDisable()
        {
            if (settings != null)
            {
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
            }
        }

        void OnGUI()
        {
            GUILayout.Space(20);
            GUILayout.Label("Server Settings", EditorStyles.boldLabel);
            GUILayout.Space(5);

            EditorGUILayout.HelpBox("To make the solution work, you need to make Server Settings! To start Configuration, you must first fill in the fields below - Title ID, Server Connection String and Developer Secret Key.", MessageType.None);
            GUILayout.Space(5);

            settings.ServerConnectionString = EditorGUILayout.TextField("Server Connection String", settings.ServerConnectionString);
            GUILayout.Space(5);
            settings.DeveloperSecretKey = EditorGUILayout.TextField("Developer Secret Key", settings.DeveloperSecretKey);
            settings.TitleID = EditorGUILayout.TextField("Title ID", settings.TitleID);

            GUILayout.Space(10);

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("IGS Admin API Link", settings.IgsAdminApiLink);
            EditorGUILayout.TextField("User Data System Link", settings.UserDataSystemLink);
            EditorGUILayout.TextField("Try Make Transaction Link", settings.TryMakeTransactionLink);
            EditorGUILayout.TextField("Try Do Marketplace Action Link", settings.TryDoMarketplaceActionLink);
            EditorGUILayout.TextField("Get Data From Marketplace Link", settings.GetDataFromMarketplaceLink);
            EditorGUILayout.TextField("Validate IAP Subscription Link", settings.ValidateIAPSubscriptionLink);
            EditorGUILayout.TextField("Validate IAP Link", settings.ValidateIAPLink);
            EditorGUILayout.TextField("Friend System Link", settings.FriendSystemLink);
            EditorGUILayout.TextField("Spin System Link", settings.SpinSystemLink);
            EditorGUILayout.TextField("Chest System Link", settings.ChestSystemLink);
            EditorGUILayout.TextField("Reward And Profit System Link", settings.RewardAndProfitSystemLink);
            EditorGUILayout.TextField("Referral System Link", settings.ReferralSystemLink);
            EditorGUILayout.TextField("Event System Link", settings.EventSystemLink);
            EditorGUILayout.TextField("Shop System Link", settings.ShopSystemLink);
            EditorGUILayout.TextField("Deal Offer System Link", settings.DealOfferSystemLink);
            EditorGUILayout.TextField("Login System Link", settings.LoginSystemLink);
            EditorGUILayout.TextField("IGS Client API Link", settings.IgsClientApiLink);
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            settings.WebGLBuildPath = EditorGUILayout.TextField("WebGL build path", settings.WebGLBuildPath);
            if (!string.IsNullOrEmpty(settings.TitleID) && !string.IsNullOrEmpty(settings.DeveloperSecretKey) && !string.IsNullOrEmpty(settings.IgsAdminApiLink) && !string.IsNullOrEmpty(settings.ServerConnectionString))
            {
                if (GUILayout.Button("Upload WebGL"))
                {
                    GUILayout.Space(5);

                    ServerDataUploader.UploadDataFromDirectory(settings.WebGLBuildPath);

                    GUILayout.Space(5);
                }
            }
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("WebGL URL", settings.WebGLUrl);
            EditorGUI.EndDisabledGroup();
            
            if (!string.IsNullOrEmpty(settings.WebGLUrl))
            {
                if (GUILayout.Button("Copy WebGL URL"))
                {
                    GUILayout.Space(5);

                    EditorGUIUtility.systemCopyBuffer = settings.WebGLUrl;

                    GUILayout.Space(5);
                }
            }
            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(settings.TitleID) && !string.IsNullOrEmpty(settings.DeveloperSecretKey) && !string.IsNullOrEmpty(settings.ServerConnectionString))
            {
                GUILayout.Space(5);

                if (GUILayout.Button("Get Configure Server"))
                {
                    AzureFunctionLinkUpdater.UpdateJsonFiles();
                }

                GUILayout.Space(5);
            }

            if (!string.IsNullOrEmpty(settings.TitleID) && !string.IsNullOrEmpty(settings.DeveloperSecretKey) && !string.IsNullOrEmpty(settings.IgsAdminApiLink))
            {
                if (GUILayout.Button("Upload Server Data"))
                {
                    GUILayout.Space(5);

                    ServerDataUploader.UploadData();

                    GUILayout.Space(5);
                }
            }

            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();

            GUIStyle saveButtonStyle = new GUIStyle(GUI.skin.button);
            saveButtonStyle.normal.textColor = Color.green;

            if (GUILayout.Button("Save Server Settings", saveButtonStyle))
            {
                if (settings != null)
                {
                    EditorUtility.SetDirty(settings);
                    AssetDatabase.SaveAssets();
                    Debug.Log("Server Settings Saved!");
                }
            }

            GUIStyle clearButtonStyle = new GUIStyle(GUI.skin.button);
            clearButtonStyle.normal.textColor = Color.red;

            GUILayout.Space(20);
            if (GUILayout.Button("Clear All Settings", clearButtonStyle))
            {
                if (settings != null)
                {
                    ServerDataUploader.DeleteAllSettings();
                    Debug.Log("Cleared all settings data");
                }
            }
            EditorGUILayout.EndHorizontal();
        }

    }
}
#endif