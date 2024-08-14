using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace IDosGames
{
	public class PopUpTransferConfirmation : PopUp
	{
		[SerializeField] private ButtonWithOptionalIcon _confirmButtonWithOptionalIcon;
		[SerializeField] private TMP_Text _itemDesription;
		[SerializeField] private TMP_Text _amount;
		[SerializeField] private TMP_Text _directionToText;
		[SerializeField] private Image _itemIcon;

		public override void Set(Action confirmAction)
		{
			_confirmButtonWithOptionalIcon.Button.onClick.RemoveAllListeners();
			_confirmButtonWithOptionalIcon.Button.onClick.AddListener(() => gameObject.SetActive(false));
			_confirmButtonWithOptionalIcon.Button.onClick.AddListener(new UnityAction(confirmAction));
		}

		public void Set(Action confirmAction, TransactionDirection transactionDirection, Sprite itemIcon, int amount, string itemDescription)
		{
			Set(confirmAction);

			_directionToText.text = GetDirectionText(transactionDirection);
			_itemIcon.sprite = itemIcon;
			_amount.text = amount.ToString();
			_itemDesription.text = itemDescription;

			_confirmButtonWithOptionalIcon.SetActivateIcon(transactionDirection == TransactionDirection.UsersCryptoWallet);
		}

		private string GetDirectionText(TransactionDirection transactionDirection)
		{
			var text = transactionDirection.ToString();

			if (transactionDirection == TransactionDirection.Game)
			{
				text = "Game";
			}
			else if (transactionDirection == TransactionDirection.UsersCryptoWallet)
			{
				text = "CryptoWallet";
			}

			return text;
		}
	}
}