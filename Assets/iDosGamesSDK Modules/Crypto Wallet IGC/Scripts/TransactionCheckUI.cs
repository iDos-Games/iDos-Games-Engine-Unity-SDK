using System;
using TMPro;
using UnityEngine;

namespace IDosGames
{
    public class TransactionCheckUI : MonoBehaviour
    {
        [Header("Switch: Direction (active visuals)")]
        [SerializeField] private GameObject switchDepositOn;   // active visual for "Deposit"
        [SerializeField] private GameObject switchWithdrawOn;  // active visual for "Withdraw"

        [Header("Switch: Type (active visuals)")]
        [SerializeField] private GameObject switchTokenOn;     // active visual for "Token"
        [SerializeField] private GameObject switchNftOn;       // active visual for "NFT"

        [Header("UI")]
        [SerializeField] private TMP_InputField hashInput;
        [SerializeField] private TextMeshProUGUI statusTxt;

        // Current state
        private TransactionDirection _direction = TransactionDirection.Game;   // Deposit -> to Game
        private CryptoTransactionType _txType = CryptoTransactionType.Token; // Default: Token

        private void OnEnable()
        {
            statusTxt.text = string.Empty;
            SelectDeposit();
            SelectToken();
        }

        // ===== PUBLIC METHODS (to be wired in the Inspector OnClick) =====
        public void SelectDeposit()
        {
            _direction = TransactionDirection.Game; // Deposit = into Game
            SetActiveSafe(switchDepositOn, true);
            SetActiveSafe(switchWithdrawOn, false);
        }

        public void SelectWithdraw()
        {
            _direction = TransactionDirection.UsersCryptoWallet; // Withdraw = to user's wallet
            SetActiveSafe(switchDepositOn, false);
            SetActiveSafe(switchWithdrawOn, true);
        }

        public void SelectToken()
        {
            _txType = CryptoTransactionType.Token;
            SetActiveSafe(switchTokenOn, true);
            SetActiveSafe(switchNftOn, false);
        }

        public void SelectNft()
        {
            _txType = CryptoTransactionType.NFT;
            SetActiveSafe(switchTokenOn, false);
            SetActiveSafe(switchNftOn, true);
        }

        public async void OnCheckClicked()
        {
            if (statusTxt) statusTxt.text = "Checking...";
            var hash = (hashInput?.text ?? "").Trim();

            if (string.IsNullOrWhiteSpace(hash))
            {
                if (statusTxt) statusTxt.text = "Enter the transaction hash.";
                return;
            }

            try
            {
                string msg = await CheckTransactionHash.CheckHash(hash, _txType, _direction);
                if (statusTxt) statusTxt.text = msg ?? "No response";
            }
            catch (Exception e)
            {
                if (statusTxt) statusTxt.text = $"Error: {e.Message}";
            }
        }
        // =================================================================

        private static void SetActiveSafe(GameObject go, bool value)
        {
            if (go && go.activeSelf != value) go.SetActive(value);
        }
    }
}
