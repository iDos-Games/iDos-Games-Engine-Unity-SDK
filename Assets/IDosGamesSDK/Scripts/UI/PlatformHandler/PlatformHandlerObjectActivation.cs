using UnityEngine;

namespace IDosGames
{
	public class PlatformHandlerObjectActivation : MonoBehaviour
	{
		[SerializeField] private GameObject[] _activeOnIOS;
		[SerializeField] private GameObject[] _activeOnAndroid;

		private bool _isAndroid = false;
		private bool _isIOS = false;

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
#endif

			foreach (GameObject go in _activeOnIOS)
			{
				go.SetActive(_isIOS);
			}

			foreach (GameObject go in _activeOnAndroid)
			{
				go.SetActive(_isAndroid);
			}
		}
	}
}
