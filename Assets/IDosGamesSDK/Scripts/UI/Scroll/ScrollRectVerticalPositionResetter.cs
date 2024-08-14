using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	[RequireComponent(typeof(ScrollRect))]
	public class ScrollRectVerticalPositionResetter : MonoBehaviour
	{
		private ScrollRect _scrollRect;

		private void Awake()
		{
			_scrollRect = GetComponent<ScrollRect>();
		}

		private void OnEnable()
		{
			ResetPosition();
		}

		public void ResetPosition()
		{
			_scrollRect.verticalNormalizedPosition = 1f;
		}
	}
}