using UnityEngine;

namespace IDosGames
{
	public class PlatformHandlerObjectActivation : MonoBehaviour
	{
		[SerializeField] private GameObject[] _activeOnIOS;
		[SerializeField] private GameObject[] _activeOnAndroid;
        [SerializeField] private GameObject[] _activeOnWebGL;

        private bool _isAndroid = false;
		private bool _isIOS = false;
        private bool _isWebGL = false;

        void Start()
		{
			SetActivateObjects();
		}

		private void SetActivateObjects()
		{
#if UNITY_ANDROID
			_isAndroid = true;
#elif UNITY_IOS
			_isIOS = true;
#elif UNITY_WEBGL
            _isWebGL = true;
#endif

            foreach (GameObject go in _activeOnIOS)
			{
				go.SetActive(_isIOS);
			}

			foreach (GameObject go in _activeOnAndroid)
			{
				go.SetActive(_isAndroid);
			}

            foreach (GameObject go in _activeOnWebGL)
            {
                go.SetActive(_isWebGL);
            }
        }
	}
}
