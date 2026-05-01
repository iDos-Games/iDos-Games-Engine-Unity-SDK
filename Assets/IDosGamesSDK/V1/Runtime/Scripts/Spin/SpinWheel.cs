using System;
using System.Collections;
using UnityEngine;

namespace IDosGames
{
	public class SpinWheel : MonoBehaviour
	{
		[Range(1, 10)]
		[SerializeField] private int _rotationTime = 5;
		[SerializeField] private AnimationCurve _animationCurve;

		private const float INITIAL_ANGLE = -22.5f;
		private const float TARGET_ANGLE_OFFSET = 2.5f;
		private const int SECTOR_ANGLE = 45;

		public event Action SpinStarted;
		public event Action<int> SpinEnded;

		private readonly System.Random _random = new();

		private void OnEnable()
		{
			ResetWheel();
		}

		private void ResetWheel()
		{
			transform.localEulerAngles = new Vector3(0, 0, INITIAL_ANGLE);
		}

		public void Spin(int targetIndex)
		{
			StartCoroutine(SpinTheWheel(targetIndex));
		}

		private IEnumerator SpinTheWheel(int targetIndex)
		{
			SpinStarted?.Invoke();

			float startAngle = transform.eulerAngles.z;
			float maxAngle = 360 * _rotationTime + GetTargetAngleByIndex(targetIndex) - startAngle;
			float elapsedTime = 0.0f;
			float currentAngle = 0.0f;

			while (elapsedTime < _rotationTime)
			{
				currentAngle = maxAngle * _animationCurve.Evaluate(elapsedTime / _rotationTime);
				transform.eulerAngles = new Vector3(0.0f, 0.0f, currentAngle + startAngle);
				elapsedTime += Time.unscaledDeltaTime;

				yield return null;
			}

			transform.eulerAngles = new Vector3(0.0f, 0.0f, maxAngle + startAngle);

			yield return new WaitForSecondsRealtime(0.25f);

			SpinEnded?.Invoke(targetIndex - 1);
		}

		private float GetTargetAngleByIndex(int index)
		{
			float targetAngle = SECTOR_ANGLE * (index - 1);

			float offset = (SECTOR_ANGLE / 2) - TARGET_ANGLE_OFFSET;

			float result = targetAngle + _random.Next((int)-offset, (int)offset + 1);

			return result;
		}
	}
}