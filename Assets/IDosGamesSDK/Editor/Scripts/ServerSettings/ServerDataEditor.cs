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

            settings.ServerLink = EditorGUILayout.TextField("Server Link", settings.ServerLink);
            GUILayout.Space(5);
            settings.DeveloperSecretKey = EditorGUILayout.TextField("Developer Secret Key", settings.DeveloperSecretKey);
            settings.TitleID = EditorGUILayout.TextField("Title ID", settings.TitleID);
            settings.TitleTemplateID = EditorGUILayout.TextField("Title ID", settings.TitleTemplateID);

            GUILayout.Space(10);

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("IGS Admin API Link", settings.IgsAdminApiLink);
            EditorGUILayout.TextField("IGS Client API Link", settings.IgsClientApiLink);
            EditorGUILayout.TextField("Telegram Webhook Link", settings.TelegramWebhookLink);
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            settings.WebGLBuildPath = EditorGUILayout.TextField("WebGL build path", settings.WebGLBuildPath);
            
            if (!string.IsNullOrEmpty(settings.TitleID) && !string.IsNullOrEmpty(settings.DeveloperSecretKey) && !string.IsNullOrEmpty(settings.ServerLink))
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
            settings.DevBuild = EditorGUILayout.Toggle("Development Build", settings.DevBuild);
            settings.ClearDirectory = EditorGUILayout.Toggle("Clear Directory Before Uploading", settings.ClearDirectory);
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

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("Do not forget to register Telegram Webhook");
            EditorGUI.EndDisabledGroup();

            if (!string.IsNullOrEmpty(settings.TitleID) && !string.IsNullOrEmpty(settings.DeveloperSecretKey) && !string.IsNullOrEmpty(settings.ServerLink))
            {
                if (GUILayout.Button("Register Telegram Webhook"))
                {
                    GUILayout.Space(5);

                    ServerDataUploader.RegisterTelegramWebhook();

                    GUILayout.Space(5);
                }
            }
            EditorGUILayout.EndHorizontal();

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