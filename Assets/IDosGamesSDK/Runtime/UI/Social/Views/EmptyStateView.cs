// File: Assets/IDosGamesSDK/Runtime/UI/Social/Views/EmptyStateView.cs
using TMPro;
using UnityEngine;

namespace IDosGames.UI.Social
{
    public class EmptyStateView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _messageText;

        public void Show(string message = "Nothing here yet")
        {
            if (_messageText != null) _messageText.text = message;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
