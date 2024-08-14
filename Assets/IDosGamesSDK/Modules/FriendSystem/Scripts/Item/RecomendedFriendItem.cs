using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace IDosGames.Friends
{
    public class RecomendedFriendItem : MonoBehaviour
    {
        [SerializeField] private Button _sendRequestButton;
        [SerializeField] private TMP_Text _name;

        public virtual void Fill(Action sendAction, string playfabID)
        {
            ResetButton(sendAction);
            _name.text = playfabID;
        }

        private void ResetButton(Action sendAction)
        {
            if (sendAction == null)
            {
                return;
            }
            _sendRequestButton.onClick.RemoveAllListeners();
            _sendRequestButton.onClick.AddListener(new UnityAction(sendAction));
        }

        public void SetInactivebutton(bool state)
        {
            _sendRequestButton.interactable = state;
        }
    }

}