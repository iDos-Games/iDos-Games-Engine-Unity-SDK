using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	public class AmountInputField : MonoBehaviour
	{
		[SerializeField] private TMP_InputField _inputField;
		[SerializeField] private TMP_Text _availableAmountText;
		[SerializeField] private Image _outerFrame;

		private Color CorrectInputColor = Color.white;
		private Color WrongInputColor = Color.red;

		private int _availableAmount = 0;

		public bool IsAmountCorrect { get; private set; }

		private void Start()
		{
			_inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
			_inputField.onValueChanged.AddListener(ValidateInput);
		}

		public string GetInput()
		{
			return _inputField.text;
		}

		public void ResetInput()
		{
			_inputField.text = string.Empty;
			ValidateInput(_inputField.text);
		}

		public void UpdateAvailableAmount(int amount)
		{
			_availableAmount = amount;
			_availableAmountText.text = _availableAmount.ToString();

			ValidateInput(GetInput());
		}

		private void ValidateInput(string input)
		{
			if (input == string.Empty)
			{
				IsAmountCorrect = false;
				ChangeOuterFrameColor(true);
				return;
			}

			if (int.TryParse(input, out int amount))
			{
				IsAmountCorrect = amount <= _availableAmount && amount > 0;
			}
			else
			{
				IsAmountCorrect = false;
			}

			ChangeOuterFrameColor(IsAmountCorrect);
		}

		public void ChangeOuterFrameColor(bool isCorrectState)
		{
			_outerFrame.color = isCorrectState ? CorrectInputColor : WrongInputColor;
		}
	}
}