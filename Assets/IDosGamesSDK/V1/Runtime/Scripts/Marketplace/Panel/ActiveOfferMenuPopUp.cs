using System;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	public class ActiveOfferMenuPopUp : MonoBehaviour
	{
		[SerializeField] private ActiveOfferMenu _activeOfferMenu;
		[SerializeField] private Button _editButton;
		[SerializeField] private Button _deleteButton;
		[SerializeField] private Button _inspectButton;

#if IDOSGAMES_MARKETPLACE
		public void SetPosition(Transform target)
		{
			transform.position = target.position;
		}

		public void SetButtons(Action inspectAction, Action editAction, Action deleteAction)
		{
			SetEditButton(editAction);
			SetDeleteButton(deleteAction);
			SetInspectButton(inspectAction);
		}

		private void SetEditButton(Action action)
		{
			_editButton.onClick.RemoveAllListeners();
			_editButton.onClick.AddListener(() => action?.Invoke());
			_editButton.onClick.AddListener(() => _activeOfferMenu.SetActivateMenu(false));
		}

		private void SetDeleteButton(Action action)
		{
			_deleteButton.onClick.RemoveAllListeners();
			_deleteButton.onClick.AddListener(() => action?.Invoke());
			_deleteButton.onClick.AddListener(() => _activeOfferMenu.SetActivateMenu(false));
		}

		private void SetInspectButton(Action action)
		{
			_inspectButton.onClick.RemoveAllListeners();
			_inspectButton.onClick.AddListener(() => action?.Invoke());
			_inspectButton.onClick.AddListener(() => _activeOfferMenu.SetActivateMenu(false));
		}
#endif

    }
}
