using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IDosGames
{
	public class SceneSwitcher : MonoBehaviour
	{
		public static event Action SwitchSceneStarted;

		public static event Action SwitchSceneFinished;

		private int _nextSceneIndex => SceneManager.GetActiveScene().buildIndex + 1;
		private int _previousSceneIndex => SceneManager.GetActiveScene().buildIndex - 1;

		public void SwitchToNextScene()
		{
			StartLoad(_nextSceneIndex);
		}

		public void SwitchToPreviousScene()
		{
			int sceneIndex = _previousSceneIndex;

			if (_previousSceneIndex < 0)
			{
				sceneIndex = 0;
			}

			StartLoad(sceneIndex);
		}

        public void SwitchToLoginScene()
        {
            StartLoad(0);
        }

        private void StartLoad(int scenebuildIndex)
		{
			OnStartLoad();

			StartCoroutine(LoadSceneAsync(scenebuildIndex));
		}

		private void OnStartLoad()
		{
			SwitchSceneStarted?.Invoke();
		}

		private void OnFinishLoad()
		{
			SwitchSceneFinished?.Invoke();
		}

		private IEnumerator LoadSceneAsync(int sceneBuildIndex)
		{
			var asyncLoad = SceneManager.LoadSceneAsync(sceneBuildIndex);
			asyncLoad.allowSceneActivation = false;

			while (!asyncLoad.isDone)
			{
				if (asyncLoad.progress >= 0.9f)
				{
					yield return new WaitForSeconds(2.0f);
					asyncLoad.allowSceneActivation = true;
				}
				yield return null;
			}

			OnFinishLoad();
		}
	}
}