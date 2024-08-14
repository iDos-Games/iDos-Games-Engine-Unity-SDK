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
			if (Direction == TransactionDirection.Game)
			{
				Direction = TransactionDirection.UsersCryptoWallet;
			}
			else if (Direction == TransactionDirection.UsersCryptoWallet)
			{
				Direction = TransactionDirection.Game;
			}
		}

		private void ReplaceText()
		{
			(_toText.text, _fromText.text) = (_fromText.text, _toText.text);
		}
	}
}