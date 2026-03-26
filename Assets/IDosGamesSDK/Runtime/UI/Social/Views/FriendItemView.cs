// File: Assets/IDosGamesSDK/Runtime/UI/Social/Views/FriendItemView.cs
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using IDosGames.ClientModels;

namespace IDosGames.UI.Social
{
    /// <summary>
    /// Universal card for displaying a FriendPublicProfile.
    /// Buttons are shown/hidden depending on the context (tab).
    /// </summary>
    public class FriendItemView : MonoBehaviour
    {
        [Header("Profile")]
        [SerializeField] private Image _avatarImage;
        [SerializeField] private TMP_Text _usernameText;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _powerText;

        [Header("Action Buttons")]
        [SerializeField] private Button _primaryButton;
        [SerializeField] private TMP_Text _primaryButtonText;
        [SerializeField] private Button _secondaryButton;
        [SerializeField] private TMP_Text _secondaryButtonText;

        [Header("Fallback")]
        [SerializeField] private Sprite _defaultAvatar;

        private string _userId;
        private bool _isBusy;

        public string UserId => _userId;

        /// <summary>
        /// Configure the card for a specific context.
        /// </summary>
        public void Setup(
            FriendPublicProfile profile,
            FriendItemMode mode,
            Action<string> onPrimaryAction = null,
            Action<string> onSecondaryAction = null)
        {
            _userId = profile.UserID;
            _isBusy = false;

            // Profile data
            var pub = profile.PublicData;
            string displayName = "Unknown";
            int level = 0;
            long power = 0;
            string avatarUrl = null;

            if (pub != null)
            {
                displayName = !string.IsNullOrEmpty(pub.Username) ? pub.Username : profile.UserID;
                level = pub.Level;
                power = pub.Power;
                avatarUrl = pub.AvatarUrl;
            }

            if (_usernameText != null) _usernameText.text = displayName;
            if (_levelText != null) _levelText.text = $"Lv.{level}";
            if (_powerText != null) _powerText.text = Shared.TimeFormatUtil.FormatNumber(power);

            // Avatar
            LoadAvatar(avatarUrl);

            // Buttons setup based on mode
            SetupButtons(mode, onPrimaryAction, onSecondaryAction);
        }

        private void SetupButtons(FriendItemMode mode, Action<string> onPrimary, Action<string> onSecondary)
        {
            // Reset
            _primaryButton.gameObject.SetActive(false);
            _secondaryButton.gameObject.SetActive(false);
            _primaryButton.onClick.RemoveAllListeners();
            _secondaryButton.onClick.RemoveAllListeners();

            switch (mode)
            {
                case FriendItemMode.Friend:
                    SetButton(_primaryButton, _primaryButtonText, "Remove", onPrimary);
                    break;

                case FriendItemMode.IncomingRequest:
                    SetButton(_primaryButton, _primaryButtonText, "Accept", onPrimary);
                    SetButton(_secondaryButton, _secondaryButtonText, "Decline", onSecondary);
                    break;

                case FriendItemMode.Recommended:
                    SetButton(_primaryButton, _primaryButtonText, "Add", onPrimary);
                    break;
            }
        }

        private void SetButton(Button btn, TMP_Text label, string text, Action<string> callback)
        {
            if (btn == null) return;
            btn.gameObject.SetActive(true);
            if (label != null) label.text = text;
            btn.interactable = true;
            btn.onClick.AddListener(() => OnButtonClicked(btn, callback));
        }

        private void OnButtonClicked(Button btn, Action<string> callback)
        {
            if (_isBusy || callback == null) return;
            _isBusy = true;
            btn.interactable = false;

            try
            {
                callback.Invoke(_userId);
            }
            finally
            {
                // Re-enable will be handled by parent panel after refresh
                // or item might be removed from list
            }
        }

        /// <summary>
        /// Re-enable buttons after an action completes (called by parent panel).
        /// </summary>
        public void SetBusy(bool busy)
        {
            _isBusy = busy;
            if (_primaryButton != null) _primaryButton.interactable = !busy;
            if (_secondaryButton != null) _secondaryButton.interactable = !busy;
        }

        private async void LoadAvatar(string url)
        {
            if (_avatarImage == null) return;

            // Set fallback immediately
            if (_defaultAvatar != null)
            {
                _avatarImage.sprite = _defaultAvatar;
            }

            if (string.IsNullOrEmpty(url)) return;

            // Use SDK ImageLoader
            var sprite = await ImageLoader.GetSpriteAsync(url);
            if (sprite != null && this != null && gameObject.activeInHierarchy)
            {
                _avatarImage.sprite = sprite;
            }
        }
    }

    public enum FriendItemMode
    {
        Friend,
        IncomingRequest,
        Recommended
    }
}
