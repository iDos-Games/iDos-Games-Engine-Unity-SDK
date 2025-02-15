using Nethereum.HdWallet;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
    public class WalletImportManager : MonoBehaviour
    {
        public WalletManager walletManager;
        public Button[] numberButtons;
        public Button deleteButton;
        public Image[] passwordDots;
        public TextMeshProUGUI statusText;
        public TMP_InputField seedPhraseInputField;
        public GameObject passwordPanel;
        public GameObject seedPhrasePanel;

        private string firstPassword = "";
        private string secondPassword = "";
        private bool isFirstPasswordEntered = false;
        private bool isProcessingInput = false;

        private string temporarySeedPhrase = null;
        private string temporaryPrivateKey = null;
        private string temporaryAddress = null;

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
            ResetPasswords("Enter your seed phrase");
            passwordPanel.SetActive(false);
            seedPhrasePanel.SetActive(true);
            EnableButtons(false);
        }

        public void OnImportButtonClick()
        {
            string seedPhrase = seedPhraseInputField.text.Trim();
            if (string.IsNullOrEmpty(seedPhrase))
            {
                statusText.text = "Seed phrase cannot be empty.";
                return;
            }

            try
            {
                var wallet = new Wallet(seedPhrase, null);
                var account = wallet.GetAccount(0);
                temporaryAddress = account.Address;
                temporarySeedPhrase = seedPhrase;
                temporaryPrivateKey = account.PrivateKey;

                //Debug.Log(temporaryAddress + " / " + temporaryPrivateKey);

                statusText.text = "Seed phrase imported successfully. Create your passcode.";
                seedPhrasePanel.SetActive(false);
                passwordPanel.SetActive(true);
                EnableButtons(true);
            }
            catch
            {
                statusText.text = "Invalid seed phrase. Please try again.";
                Debug.Log("Invalid seed phrase. Please try again.");
            }
        }

        private void OnNumberButtonClick(string number)
        {
            if (isProcessingInput) return;
            isProcessingInput = true;

            if (isFirstPasswordEntered && secondPassword.Length < 6)
            {
                secondPassword += number;
            }
            else if (!isFirstPasswordEntered && firstPassword.Length < 6)
            {
                firstPassword += number;
            }

            CheckPasswords();
            isProcessingInput = false;
        }

        private void OnDeleteButtonClick()
        {
            if (isProcessingInput) return;
            isProcessingInput = true;

            if (isFirstPasswordEntered && secondPassword.Length > 0)
            {
                secondPassword = secondPassword.Substring(0, secondPassword.Length - 1);
            }
            else if (!isFirstPasswordEntered && firstPassword.Length > 0)
            {
                firstPassword = firstPassword.Substring(0, firstPassword.Length - 1);
            }

            UpdatePasswordDots();
            isProcessingInput = false;
        }

        private void UpdatePasswordDots()
        {
            string currentPassword = isFirstPasswordEntered ? secondPassword : firstPassword;
            for (int i = 0; i < passwordDots.Length; i++)
            {
                passwordDots[i].color = i < currentPassword.Length ? new Color32(0x16, 0x2A, 0x58, 0xFF) : Color.white;
            }
        }

        private void CheckPasswords()
        {
            if (firstPassword.Length == 6 && !isFirstPasswordEntered)
            {
                isFirstPasswordEntered = true;
                statusText.text = "Confirm your passcode";
                ClearPasswordDots();
            }
            else if (secondPassword.Length == 6)
            {
                EnableButtons(false);
                if (firstPassword == secondPassword)
                {
                    statusText.text = "Password successfully saved!";
                    SavePassword();
                }
                else
                {
                    ResetPasswords("Passwords don't match. Create your passcode again.");
                }
                EnableButtons(true);
            }
            UpdatePasswordDots();
        }

        private void ClearPasswordDots()
        {
            for (int i = 0; i < passwordDots.Length; i++)
            {
                passwordDots[i].color = Color.white;
            }
        }

        private void ResetPasswords(string text)
        {
            firstPassword = "";
            secondPassword = "";
            isFirstPasswordEntered = false;
            statusText.text = text;
            ClearPasswordDots();
        }

        private void SavePassword()
        {
            PlayerPrefs.SetString(WalletManager.PLAYER_PREFS_WALLET_ADDRESS, temporaryAddress);
            PlayerPrefs.Save();

            PrivateKeyManager.SaveSeedPhrase(temporarySeedPhrase, temporaryPrivateKey, firstPassword);
            WalletManager.WalletAddress = temporaryAddress;

            temporarySeedPhrase = null;
            temporaryPrivateKey = null;
            temporaryAddress = null;

            ResetPasswords(null);

            StartCoroutine(OpenConnectedWalletWithDelay());
        }

        private IEnumerator OpenConnectedWalletWithDelay()
        {
            yield return null;
            yield return null;
            walletManager.OpenConnectedWallet();
        }

        private void EnableButtons(bool enable)
        {
            foreach (Button btn in numberButtons)
            {
                btn.interactable = enable;
            }
            deleteButton.interactable = enable;
        }

        public void CopySeedPhrase()
        {
            GUIUtility.systemCopyBuffer = seedPhraseInputField.text;
        }

        public void ClearSeedPhrase()
        {
            seedPhraseInputField.text = "";
        }

        public void PasteSeedPhrase()
        {
            seedPhraseInputField.text = GUIUtility.systemCopyBuffer;

#if UNITY_WEBGL
            WebSDK.PasteTextFromClipboard();
            Invoke(nameof(CopyClipboardText), 0.3f);
#endif
        }

        private void CopyClipboardText()
        {
            seedPhraseInputField.text = WebFunctionHandler.Instance._clipboardText;
        }
    }
}
