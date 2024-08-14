using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
    public class GetPrivateKey : MonoBehaviour
    {
        public Button[] numberButtons;
        public Button deleteButton;
        public Image[] passwordDots;
        public TextMeshProUGUI statusText;

        private string password = "";
        private bool isProcessingInput = false;
        private bool showPrivateKey = false;

        public GameObject privateKeyPanel;
        public TextMeshProUGUI privateKeyText;

        void Start()
        {
            foreach (Button button in numberButtons)
            {
                string number = button.GetComponentInChildren<TextMeshProUGUI>().text;
                button.onClick.AddListener(() => OnNumberButtonClick(number));
            }
            deleteButton.onClick.AddListener(OnDeleteButtonClick);
        }

        void OnEnable()
        {
            //ShowGetPrivateKey();
        }

        public void ShowGetPrivateKey(bool privateKey)
        {
            showPrivateKey = privateKey;

            this.gameObject.SetActive(true);
            privateKeyPanel.SetActive(false);
            ResetPassword("Enter your passcode");
            EnableButtons(true);
        }

        private void OnNumberButtonClick(string number)
        {
            if (isProcessingInput) return;
            isProcessingInput = true;

            if (password.Length < 6)
            {
                password += number;
            }

            CheckPassword();
            isProcessingInput = false;
        }

        private void OnDeleteButtonClick()
        {
            if (isProcessingInput) return;
            isProcessingInput = true;

            if (password.Length > 0)
            {
                password = password.Substring(0, password.Length - 1);
            }

            UpdatePasswordDots();
            isProcessingInput = false;
        }

        private void UpdatePasswordDots()
        {
            for (int i = 0; i < passwordDots.Length; i++)
            {
                passwordDots[i].color = i < password.Length ? new Color32(0x16, 0x2A, 0x58, 0xFF) : Color.white;
            }
        }

        private void CheckPassword()
        {
            if (password.Length == 6)
            {
                EnableButtons(false);
                RetrievePrivateKey();
                EnableButtons(true);
            }
            UpdatePasswordDots();
        }

        private void ResetPassword(string text)
        {
            password = "";
            statusText.text = text;
            ClearPasswordDots();
        }

        private void ClearPasswordDots()
        {
            for (int i = 0; i < passwordDots.Length; i++)
            {
                passwordDots[i].color = Color.white;
            }
        }

        private void RetrievePrivateKey()
        {
            (string seedPhrase, string privateKey) = PrivateKeyManager.GetSeedPhrase(password);
            if (!string.IsNullOrEmpty(seedPhrase))
            {
                if(showPrivateKey)
                {
                    privateKeyPanel.SetActive(true);
                    privateKeyText.text = "Seed Phrase: \n" + seedPhrase + "\n \nPrivate Key: \n" + privateKey;
                    
                    Debug.Log(privateKey);
                    this.gameObject.SetActive(false);
                }
                else
                {
                    WalletManager.SeedPhrase = seedPhrase;
                    WalletManager.PrivateKey = privateKey;

                    statusText.text = "Seed Phrase retrieved successfully!";
                    this.gameObject.SetActive(false);
                }
            }
            else
            {
                ResetPassword("Invalid passcode. Try again.");
            }
        }

        private void EnableButtons(bool enable)
        {
            foreach (Button btn in numberButtons)
            {
                btn.interactable = enable;
            }
            deleteButton.interactable = enable;
        }

    }
}
