// File: Assets/IDosGamesSDK/Runtime/UI/Social/Views/TimelineItemView.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using IDosGames.ClientModels;
using IDosGames.UI.Shared;

namespace IDosGames.UI.Social
{
    public class TimelineItemView : MonoBehaviour
    {
        [Header("Profile")]
        [SerializeField] private Image _avatarImage;
        [SerializeField] private TMP_Text _actorNameText;

        [Header("Event Info")]
        [SerializeField] private TMP_Text _eventDescriptionText;
        [SerializeField] private TMP_Text _timeAgoText;
        [SerializeField] private TMP_Text _amountText;
        [SerializeField] private Image _eventIcon;

        [Header("Icons per EventType")]
        [SerializeField] private Sprite _attackIcon;
        [SerializeField] private Sprite _raidIcon;
        [SerializeField] private Sprite _friendAddIcon;
        [SerializeField] private Sprite _defaultAvatar;

        public void Setup(SocialTimelineEventDocument evt)
        {
            // Actor name
            string actorName = "Someone";
            string avatarUrl = null;

            if (evt.ActorProfile != null)
            {
                actorName = !string.IsNullOrEmpty(evt.ActorProfile.Username)
                    ? evt.ActorProfile.Username
                    : evt.ActorUserID;
                avatarUrl = evt.ActorProfile.AvatarUrl;
            }

            if (_actorNameText != null) _actorNameText.text = actorName;

            // Description
            string description = BuildDescription(evt, actorName);
            if (_eventDescriptionText != null) _eventDescriptionText.text = description;

            // Time
            if (_timeAgoText != null) _timeAgoText.text = TimeFormatUtil.FormatRelativeTime(evt.CreatedAt);

            // Amount
            if (_amountText != null)
            {
                if (evt.Amount != 0)
                {
                    _amountText.gameObject.SetActive(true);
                    _amountText.text = TimeFormatUtil.FormatNumber(evt.Amount);
                }
                else
                {
                    _amountText.gameObject.SetActive(false);
                }
            }

            // Event icon
            if (_eventIcon != null)
            {
                _eventIcon.sprite = GetEventIcon(evt.Type);
            }

            // Avatar
            LoadAvatar(avatarUrl);
        }

        private string BuildDescription(SocialTimelineEventDocument evt, string actorName)
        {
            string target = !string.IsNullOrEmpty(evt.TargetObjectName) ? evt.TargetObjectName : "";

            switch (evt.Type)
            {
                case TimelineEventType.Attack:
                    return $"{actorName} attacked {target}";
                case TimelineEventType.Raid:
                    return $"{actorName} raided {target}";
                case TimelineEventType.FriendAdd:
                    return $"{actorName} added you as a friend";
                default:
                    return $"{actorName} did something";
            }
        }

        private Sprite GetEventIcon(TimelineEventType type)
        {
            switch (type)
            {
                case TimelineEventType.Attack: return _attackIcon;
                case TimelineEventType.Raid: return _raidIcon;
                case TimelineEventType.FriendAdd: return _friendAddIcon;
                default: return _attackIcon;
            }
        }

        private async void LoadAvatar(string url)
        {
            if (_avatarImage == null) return;

            if (_defaultAvatar != null) _avatarImage.sprite = _defaultAvatar;
            if (string.IsNullOrEmpty(url)) return;

            var sprite = await ImageLoader.GetSpriteAsync(url);
            if (sprite != null && this != null && gameObject.activeInHierarchy)
            {
                _avatarImage.sprite = sprite;
            }
        }
    }
}
