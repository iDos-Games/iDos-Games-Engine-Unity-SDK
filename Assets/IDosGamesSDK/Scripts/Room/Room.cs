using UnityEngine;

namespace IDosGames
{
	public abstract class Room : MonoBehaviour
	{
		[SerializeField] private MainSceneViewObjects _mainSceneViewObjects;

		protected virtual void OnEnable()
		{
			SetActiveMainSceneObjects(false);
		}

		protected virtual void OnDisable()
		{
			SetActiveMainSceneObjects(true);
		}

		protected void SetActiveMainSceneObjects(bool active)
		{
			if (_mainSceneViewObjects == null)
			{
				return;
			}

			_mainSceneViewObjects.gameObject.SetActive(active);
		}

		protected void SetActiveRoom(bool active)
		{
			gameObject.SetActive(active);
		}
	}
}
