#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace IDosGames
{
    public class ServerDataEditor : EditorWindow
    {
        [Header("Server Settings")]

        private IDosGamesSDKSettings settings;

        [MenuItem("iDos Games/1. Server Settings")]
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

            EditorGUI.BeginDisabledGroup(true);
            settings.ServerLink = EditorGUILayout.TextField("Server Link", settings.ServerLink);
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(5);
            settings.DeveloperSecretKey = EditorGUILayout.TextField("Developer Secret Key", settings.DeveloperSecretKey);
            settings.TitleID = EditorGUILayout.TextField("Title ID", settings.TitleID);
            settings.TitleTemplateID = EditorGUILayout.TextField("Title Template ID", settings.TitleTemplateID);
            settings.BuildKey = EditorGUILayout.TextField("Build Key", settings.BuildKey);

            GUILayout.Space(10);

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("Admin API Link", settings.IgsAdminApiLink);
            EditorGUILayout.TextField("User Data Link", settings.UserDataSystemLink);
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

            settings.DevBuild = EditorGUILayout.Toggle("Development Build", settings.DevBuild);

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