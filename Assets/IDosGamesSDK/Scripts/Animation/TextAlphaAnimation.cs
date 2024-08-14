using System.Collections;
using TMPro;
using UnityEngine;

namespace IDosGames
{
	[RequireComponent(typeof(TMP_Text))]
	public class TextAlphaAnimation : MonoBehaviour
	{
		private const float ANIMATION_SPEED = 0.003f;

		private TMP_Text _text;

		private float _animationTime;

		private void Awake()
		{
			_text = GetComponent<TMP_Text>();
		}

		private void OnEnable()
		{
			StartCoroutine(Animate());
		}

		private void OnDisable()
		{
			StopAllCoroutines();
		}

		private IEnumerator Animate()
		{
			_animationTime = 0;

			while (true)
			{
				_animationTime += ANIMATION_SPEED;
				_text.alpha = Mathf.SmoothStep(0, 1.0f, Mathf.PingPong(_animationTime, 1));
				yield return null;
			}
		}
	}
}