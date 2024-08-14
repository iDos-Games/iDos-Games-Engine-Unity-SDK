using System.Collections;
using UnityEngine;

namespace IDosGames
{
	public class DelayToShowObject : MonoBehaviour
	{
		[SerializeField] private GameObject _objectToShow;
		[SerializeField] private float _delayInSeconds = 3;

		private void OnEnable()
		{
			_objectToShow.gameObject.SetActive(false);
			StartCoroutine(Show());
		}

		private IEnumerator Show()
		{
			yield return new WaitForSecondsRealtime(_delayInSeconds);
			_objectToShow.SetActive(true);
		}
	}
}