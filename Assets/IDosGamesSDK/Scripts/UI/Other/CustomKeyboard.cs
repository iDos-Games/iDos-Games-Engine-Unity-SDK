using TMPro;
using UnityEngine;

namespace IDosGames
{
    public class CustomKeyboard : MonoBehaviour
    {
        public TMP_InputField inputField;
        public GameObject popupPanel;
        public TextMeshProUGUI keyboardText;
        private string currentInput = "";

        void Start()
        {
            popupPanel.SetActive(false);

            inputField.onSelect.AddListener(delegate { ShowPopup(); });
        }

        public void ShowPopup()
        {
            popupPanel.SetActive(true);
            currentInput = inputField.text;
            keyboardText.text = currentInput;
        }

        public void OnButtonClick(string number)
        {
            currentInput += number;
            keyboardText.text = currentInput;
            inputField.text = currentInput;
        }

        public void OnDeleteButtonClick()
        {
            if (currentInput.Length > 0)
            {
                currentInput = currentInput.Substring(0, currentInput.Length - 1);
                keyboardText.text = currentInput;
                inputField.text = currentInput;
            }
        }

        public void OnCloseButtonClick()
        {
            popupPanel.SetActive(false);
            inputField.text = currentInput;
        }
    }
}
