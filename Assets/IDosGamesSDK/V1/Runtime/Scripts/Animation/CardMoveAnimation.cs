using System.Collections;
using UnityEngine;

namespace IDosGames
{
	public class CardMoveAnimation : MonoBehaviour
	{
		[SerializeField] private RectTransform _card;
		[SerializeField] private RectTransform _pointA;
		[SerializeField] private RectTransform _pointB;

		private float m_AnimationTime;
		private float m_LastKeyTime;

		[Header("Animation Curves")]
		[SerializeField] private AnimationCurve m_MoveCurve;
		[SerializeField] private AnimationCurve m_RotationCurve;
		[SerializeField] private AnimationCurve m_ScaleCurve;

		private void Start()
		{
			ResetAnimation();
		}

		public void ResetAnimation()
		{
			_card.position = _pointA.position;
			_card.rotation = _pointA.rotation;
			_card.localScale = _pointA.localScale;

			m_AnimationTime = 0;

			Keyframe lastKey = m_MoveCurve[m_MoveCurve.length - 1];
			Keyframe tempKey = m_RotationCurve[m_RotationCurve.length - 1];

			if (lastKey.time < tempKey.time)
			{
				lastKey = tempKey;
			}

			tempKey = m_ScaleCurve[m_ScaleCurve.length - 1];

			if (lastKey.time < tempKey.time)
			{
				lastKey = tempKey;
			}

			m_LastKeyTime = lastKey.time;

			StopAllCoroutines();
		}

		public void StartAnimation()
		{
			ResetAnimation();
			StartCoroutine(Animate());
		}

		private IEnumerator Animate()
		{
			while (m_AnimationTime < m_LastKeyTime)
			{
				m_AnimationTime += Time.deltaTime;
				_card.position = Vector3.Lerp(_pointA.position, _pointB.position, m_MoveCurve.Evaluate(m_AnimationTime));
				_card.rotation = Quaternion.Lerp(_pointA.rotation, _pointB.rotation, m_RotationCurve.Evaluate(m_AnimationTime));
				_card.localScale = Vector3.Lerp(_pointA.localScale, _pointB.localScale, m_ScaleCurve.Evaluate(m_AnimationTime));
				yield return new WaitForEndOfFrame();
			}
		}
	}
}