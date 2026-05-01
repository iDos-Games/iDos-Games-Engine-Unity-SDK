using System;
using UnityEngine;

namespace IDosGames
{
	public class SpinButtonsSwitcher : MonoBehaviour
	{
		[SerializeField] private SpinButton _free;
		[SerializeField] private SpinButton _standard;
		[SerializeField] private SpinButton _premium;

		private void OnEnable()
		{
			Switch(SpinTicketType.Standard);
		}

		public void Switch(SpinTicketType type)
		{
			_free.gameObject.SetActive(type == SpinTicketType.Free);
			_standard.gameObject.SetActive(type == SpinTicketType.Standard);
			_premium.gameObject.SetActive(type == SpinTicketType.Premium);
		}

		public void ResetListeners(Action<SpinTicketType> tryToSpinAction)
		{
			_standard.Button.onClick.RemoveAllListeners();
			_premium.Button.onClick.RemoveAllListeners();

			_standard.Button.onClick.AddListener(() => tryToSpinAction(SpinTicketType.Standard));
			_premium.Button.onClick.AddListener(() => tryToSpinAction(SpinTicketType.Premium));
		}
	}
}