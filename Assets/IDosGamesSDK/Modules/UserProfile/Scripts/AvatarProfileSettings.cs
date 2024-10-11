using IDosGames.TitlePublicConfiguration;
using UnityEngine;

namespace IDosGames.UserProfile
{
    public class AvatarProfileSettings : MonoBehaviour
    {
        [SerializeField] private UserProfileWindow _userProfileWindow;

        public void ChangeGender(Gender gender)
        {
            _userProfileWindow.ChangeGender(gender);
        }

        public Gender GetAvatarGender()
        {
            return _userProfileWindow.UserAvatar.Gender;
        }
    }
}