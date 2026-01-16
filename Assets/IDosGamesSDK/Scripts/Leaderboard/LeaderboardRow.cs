using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	public class LeaderboardRow : MonoBehaviour
	{
		[SerializeField] private Image _frame;
		[SerializeField] private TMP_Text _place;
		[SerializeField] private TMP_Text _playerID;
		[SerializeField] private TMP_Text _value;
		[SerializeField] private Image _rankIcon;
        [SerializeField] private Image _userAvatar;

        public void Set(int place, string playerID, string statValue, string avatarUrl, Sprite rankIcon = null)
		{
			SetPlace(place);
			SetPlayerID(playerID);
			SetStatValue(statValue);
			SetRankIcon(rankIcon);
			SetRowFrameColor(playerID);
			SetUserAvatar(avatarUrl);
        }

		private void SetPlace(int place)
		{
			string placeText = "-";

			if (place >= 0)
			{
				placeText = $"{place + 1}";
			}

			_place.text = placeText;
		}

		private void SetStatValue(string statValue)
		{
			_value.text = $"{statValue}";
		}

		private void SetPlayerID(string playerID)
		{
			if (playerID == AuthService.UserID)
			{
				playerID = $"you: {playerID}";
			}

			_playerID.text = playerID;
		}

		private async void SetUserAvatar(string avatarUrl)
		{
            _userAvatar.gameObject.SetActive(avatarUrl != null);
            _userAvatar.sprite = await ImageLoader.GetSpriteAsync(avatarUrl);
        }

        private void SetRankIcon(Sprite rankIcon)
        {
            _rankIcon.gameObject.SetActive(rankIcon != null);
            _rankIcon.sprite = rankIcon;
        }

        private void SetRowFrameColor(string playerID)
		{
			if (playerID != AuthService.UserID)
			{
				return;
			}

			_frame.color = Color.green;
		}
	}
}