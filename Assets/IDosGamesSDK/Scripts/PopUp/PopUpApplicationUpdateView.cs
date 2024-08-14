using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	public class PopUpApplicationUpdateView : MonoBehaviour
	{
		[SerializeField] private Button _buttonUpdate;
		[SerializeField] private Button _buttonClose;

		[SerializeField] private TMP_Text _yourVersion;
		[SerializeField] private TMP_Text _actualVersion;

		public void SetVersionInfo(string actualVersion)
		{
			_yourVersion.text = Application.version;
			_actualVersion.text = actualVersion;
		}

		public void SetActiveCloseButton(bool active)
		{
			_buttonClose.gameObject.SetActive(active);
		}

		public void ResetUpdateButton(Action action)
		{
			_buttonUpdate.onClick.RemoveAllListeners();
			_buttonUpdate.onClick.AddListener(() => action?.Invoke());
		}
	}
}