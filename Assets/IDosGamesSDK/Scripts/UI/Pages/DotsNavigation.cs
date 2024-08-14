using System.Collections.Generic;
using UnityEngine;

namespace IDosGames
{
	public class DotsNavigation : MonoBehaviour
	{
		[SerializeField] private DotPageObject _dotPagePrefab;
		[SerializeField] private Transform _dotPagesParent;

		private List<DotPageObject> _dotPagesObjects = new();

		public int CurrentPageIndex => _currentPageIndex;

		private int _currentPageIndex;

		public void Initialize(int pagesCount)
		{
			if (pagesCount < 1)
			{
				return;
			}

			_dotPagesObjects.Clear();

			foreach (Transform child in _dotPagesParent)
			{
				Destroy(child.gameObject);
			}

			for (int i = 0; i < pagesCount; i++)
			{
				var dotPageObject = Instantiate(_dotPagePrefab, _dotPagesParent);
				dotPageObject.Deselect();
				_dotPagesObjects.Add(dotPageObject);
			}

			_currentPageIndex = 0;
			SetSelectedCurrentPage();
		}

		public void SwitchToNext()
		{
			if (_dotPagesObjects.Count == 0)
			{
				return;
			}

			_currentPageIndex++;

			if (_currentPageIndex > _dotPagesObjects.Count - 1)
			{
				_currentPageIndex = 0;
			}

			SetSelectedCurrentPage();
		}

		public void SwitchToPrevious()
		{
			if (_dotPagesObjects.Count == 0)
			{
				return;
			}

			_currentPageIndex--;

			if (_currentPageIndex < 0)
			{
				_currentPageIndex = _dotPagesObjects.Count - 1;
			}

			SetSelectedCurrentPage();
		}

		private void SetSelectedCurrentPage()
		{
			foreach (var page in _dotPagesObjects)
			{
				page.Deselect();
			}

			_dotPagesObjects[_currentPageIndex].Select();
		}
	}
}