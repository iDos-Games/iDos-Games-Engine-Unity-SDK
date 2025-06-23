using System;
using TMPro;
using UnityEngine;

namespace IDosGames
{
	public class WalletTransferDirection : MonoBehaviour
	{
		public TransactionDirection Direction { get; private set; } = TransactionDirection.UsersCryptoWallet;

		[SerializeField] private TMP_Text _fromText;
		[SerializeField] private TMP_Text _toText;
        [SerializeField] private GameObject _externalAddress;

		public event Action ValueChanged;

		private void OnEnable()
		{
			ResetDirection();
		}

		public void Switch()
		{
			SwitchDirection();
			ReplaceText();

			ValueChanged?.Invoke();
		}

		private void ResetDirection()
		{
			if (Direction == TransactionDirection.UsersCryptoWallet)
			{
				return;
			}

			Switch();
		}

        private void SwitchDirection()
        {
            switch (Direction)
            {
                case TransactionDirection.Game:
                    Direction = TransactionDirection.UsersCryptoWallet;
                    break;
                case TransactionDirection.UsersCryptoWallet:
                    Direction = TransactionDirection.ExternalWalletAddress;
                    break;
                case TransactionDirection.ExternalWalletAddress:
                    Direction = TransactionDirection.Game;
                    break;
            }
        }

        private void ReplaceText()
		{
            switch (Direction)
            {
                case TransactionDirection.Game:
                    _fromText.text = "Crypto Wallet";
                    _toText.text = "Game";
                    _externalAddress.SetActive(false);
                    break;
                case TransactionDirection.UsersCryptoWallet:
                    _fromText.text = "Game ";
                    _toText.text = "Crypto Wallet";
                    _externalAddress.SetActive(false);
                    break;
                case TransactionDirection.ExternalWalletAddress:
                    _fromText.text = "Crypto Wallet";
                    _toText.text = "External Address";
                    _externalAddress.SetActive(true);
                    break;
            }
        }
	}
}