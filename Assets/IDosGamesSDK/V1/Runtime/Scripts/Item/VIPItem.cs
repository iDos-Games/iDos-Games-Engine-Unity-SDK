using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace IDosGames
{
	[RequireComponent(typeof(Button))]
	public class VIPItem : MonoBehaviour
	{
		[SerializeField] private Button _button;
		[SerializeField] private TMP_Text _price;

		public virtual void Fill(Action action, string price)
		{
			ResetButton(action);
			_price.text = price;
		}

		private void ResetButton(Action action)
		{
			_button.onClick.RemoveAllListeners();
			_button.onClick.AddListener(new UnityAction(action));
		}
	}

}