using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace IDosGames
{
	public class LoadMoreButton : MonoBehaviour
	{
		[SerializeField] private Button _button;

		public void ResetButton(Action action)
		{
			if (action == null)
			{
				return;
			}

			_button.onClick.RemoveAllListeners();
			_button.onClick.AddListener(new UnityAction(action));
		}
	}
}
