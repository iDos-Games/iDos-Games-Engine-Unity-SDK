using System.Collections;
using UnityEngine;

namespace IDosGames
{
	public class AutoHidePanelAfterDelay : MonoBehaviour
	{
		[SerializeField, Range(5, 15)] private int _hideDelayInSeconds = 7;

		private void OnEnable()
		{
			StopCoroutine(HideDelay());
			StartCoroutine(HideDelay());
		}

		private void OnDisable()
		{
			StopCoroutine(HideDelay());
		}

		private IEnumerator HideDelay()
		{
			yield return new WaitForSecondsRealtime(_hideDelayInSeconds);

			gameObject.SetActive(false);
		}
	}
}
