using System;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	public class ButtonContinue : MonoBehaviour
	{
		[SerializeField] private Button _button;

		public Button Button => _button;

		private void OnEnable()
		{
			SetActive(true);
		}

		public void SetActive(bool active)
		{
			_button.gameObject.SetActive(active);
		}

		public void ResetListener(Action action)
		{
			_button.onClick.RemoveAllListeners();
			_button.onClick.AddListener(() => action?.Invoke());
		}
	}
}