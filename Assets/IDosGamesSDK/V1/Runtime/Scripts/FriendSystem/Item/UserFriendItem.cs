using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace IDosGames.Friends
{
    public class UserFriendItem : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Button _deleteButton;
        [SerializeField] private TMP_Text _name;

        private string userID;

        public virtual void Fill(Action attackAction, Action deleteAction, string playfabID)
        {
            ResetButton(attackAction, deleteAction);
            _name.text = playfabID;
            this.userID = playfabID;
        }
        public virtual void Fill(Action deleteAction, string playfabID)
        {
            ResetButton(null, deleteAction);
            _name.text = playfabID;
            this.userID = playfabID;
        }

        private void ResetButton(Action attackAction, Action deleteAction)
        {
            if (attackAction == null)
            {
                return;
            }
            if (deleteAction == null)
            {
                return;
            }
            _button.onClick.RemoveAllListeners();
            _deleteButton.onClick.RemoveAllListeners();
            _button.onClick.AddListener(new UnityAction(attackAction));
            _deleteButton.onClick.AddListener(new UnityAction(deleteAction));
        }

    }
}
