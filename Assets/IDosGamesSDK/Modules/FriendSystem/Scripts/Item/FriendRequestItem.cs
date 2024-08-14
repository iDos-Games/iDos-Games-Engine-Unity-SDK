using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
namespace IDosGames.Friends
{
    public class FriendRequestItem : MonoBehaviour
    {
        [SerializeField] private Button _acceptButton;
        [SerializeField] private Button _rejectButton;
        [SerializeField] private TMP_Text _name;

        public virtual void Fill(Action acceptAction, Action rejectAction, string playfabID)
        {
            ResetButton(acceptAction, rejectAction);
            _name.text = playfabID;
        }

        private void ResetButton(Action acceptAction, Action rejectAction)
        {
            if (acceptAction == null)
            {
                return;
            }
            if (rejectAction == null)
            {
                return;
            }
            _acceptButton.onClick.RemoveAllListeners();
            _rejectButton.onClick.RemoveAllListeners();
            _acceptButton.onClick.AddListener(new UnityAction(acceptAction));
            _rejectButton.onClick.AddListener(new UnityAction(rejectAction));
        }
    }
}