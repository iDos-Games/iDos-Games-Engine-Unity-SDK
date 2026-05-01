using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	[RequireComponent(typeof(Button))]
	public class TabItem : MonoBehaviour
	{
		[SerializeField] private Color _onSelectedColor;
		[SerializeField] private Color _onDelectedColor;
		[SerializeField] private GameObject _onSelectedFocusObject;

		[SerializeField] private TMP_Text _text;

		private Button _button;

		private void Awake()
		{
			_button = GetComponent<Button>();
		}

		public void Select()
		{
			_text.color = _onSelectedColor;

			if (_onSelectedFocusObject != null)
			{
				_onSelectedFocusObject.SetActive(true);
			}

			SetTabInteractable(false);
		}

		public void Deselect()
		{
			_text.color = _onDelectedColor;

			if (_onSelectedFocusObject != null)
			{
				_onSelectedFocusObject.SetActive(false);
			}

			SetTabInteractable(true);
		}

		private void SetTabInteractable(bool interactable)
		{
			if (_button == null)
			{
				return;
			}

			_button.interactable = interactable;
		}
	}
}
