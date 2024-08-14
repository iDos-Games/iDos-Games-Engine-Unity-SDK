using System.Collections;
using UnityEngine;

namespace IDosGames
{
	public class PopUpCopied : PopUp
	{
		private const float _appearScaleSpeed = 0.25f;
		private const float _appearSpeed = 0.01f;

		private void OnEnable()
		{
			ShowAnimation();
		}

		private void OnDisable()
		{
			Hide();
		}

		private void ShowAnimation()
		{
			StopCoroutine(nameof(Animate));
			StartCoroutine(nameof(Animate));
		}

		private void Hide()
		{
			gameObject.SetActive(false);
		}

		private IEnumerator Animate()
		{
			transform.localScale = Vector3.zero;

			yield return new WaitForEndOfFrame();

			while (transform.localScale.x <= 1)
			{
				yield return new WaitForSecondsRealtime(_appearSpeed);
				transform.localScale += Vector3.one * _appearScaleSpeed;
			}

			transform.localScale = Vector3.one;

			yield return new WaitForSecondsRealtime(1);

			Hide();
		}
	}
}