using NBitcoin;
using Nethereum.HdWallet;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
    public class WalletCreationManager : MonoBehaviour
    {
        public Button[] numberButtons;
        public Button deleteButton;
        public Image[] passwordDots;
        public TextMeshProUGUI statusText;
        public TextMeshProUGUI seedPhraseText;
        public GameObject passwordPanel;
        public GameObject seedPhrasePanel;
        private string firstPassword = "";
        private string secondPassword = "";
        private bool isFirstPasswordEntered = false;
        private bool isProcessingInput = false;

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
            ResetPasswords("Create your passcode");
            passwordPanel.SetActive(true);
            seedPhrasePanel.SetActive(false);
            EnableButtons(true);
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
                    CreateWallet();
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

        private void CreateWallet()
        {
            var wallet = new Wallet(Wordlist.English, WordCount.Twelve);
            string mnemonic = String.Join(" ", wallet.Words);

            var account = wallet.GetAccount(0);
            string address = account.Address;
            string privateKey = account.PrivateKey;
            seedPhraseText.text = mnemonic;

            if(IDosGamesSDKSettings.Instance.DebugLogging)
            {
                Debug.Log("Mnemonics: " + mnemonic);
                Debug.Log("Address: " + address);
                Debug.Log("PrivateKey: " + privateKey);
            }
            
            WalletManager.WalletAddress = address;
            PlayerPrefs.SetString(WalletManager.PLAYER_PREFS_WALLET_ADDRESS, address);
            PlayerPrefs.Save();
            PrivateKeyManager.SaveSeedPhrase(mnemonic, privateKey, firstPassword);

            passwordPanel.SetActive(false);
            seedPhrasePanel.SetActive(true);
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
            GUIUtility.systemCopyBuffer = seedPhraseText.text;

#if UNITY_WEBGL && !UNITY_EDITOR
            WebSDK.CopyTextToClipboard(seedPhraseText.text);
#endif
        }
    }
}
