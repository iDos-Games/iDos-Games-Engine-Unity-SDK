using UnityEditor;
using UnityEngine;

namespace IDosGames
{
    public class ImageManagerWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private ImageData[] _allImageData;

        [MenuItem("iDos Games/3. UI Image Manager")]
        public static void ShowWindow()
        {
            GetWindow<ImageManagerWindow>("Image Manager");
        }

        private void OnEnable()
        {
            LoadImageData();
        }

        private void LoadImageData()
        {
            _allImageData = Resources.LoadAll<ImageData>("");
            Debug.Log("Loaded " + _allImageData.Length + " ImageData objects");
        }

        private void OnGUI()
        {
            if (_allImageData == null || _allImageData.Length == 0)
            {
                GUILayout.Label("No ImageData objects found in Resources.");
                return;
            }

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);
            foreach (var imageData in _allImageData)
            {
                GUILayout.Label("Image Data: " + imageData.name);

                for (int i = 0; i < imageData.images.Count; i++)
                {
                    var pair = imageData.images[i];
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(pair.imageType.ToString(), GUILayout.Width(150));
                    pair.imageSprite = (Sprite)EditorGUILayout.ObjectField(pair.imageSprite, typeof(Sprite), false);
                    imageData.images[i] = pair;
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndScrollView();

            if (GUILayout.Button("Save Changes"))
            {
                foreach (var imageData in _allImageData)
                {
                    EditorUtility.SetDirty(imageData);
                }
                AssetDatabase.SaveAssets();
                Debug.Log("Images Updated");
            }
        }
    }
}