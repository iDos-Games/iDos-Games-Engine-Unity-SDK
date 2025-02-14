using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	public class InviteFriendsPopUp : PopUp
	{
		private const int MIN_USER_ID_LENGHT = 10;

		[SerializeField] private ReferralSystem _referralSystem;

		[SerializeField] private TMP_Text _numberOfFollowers;
		[SerializeField] private TMP_Text _subscribedTo;
		[SerializeField] private TMP_Text _currentUserReferralCode;
		[SerializeField] private TMP_InputField _referralCodeInputField;
		[SerializeField] private Button _activateButton;
		[SerializeField] private Button _shareButton;
		[SerializeField] private ReferralInviteRewardsView _inviteRewardsView;
		[SerializeField] private GameObject _firstRewardActivationInfo;

		private void Start()
		{
			ResetReferralCodeInputField();
			ResetActivateButton();
			ResetShareButton();

			if (PlayerPrefs.GetInt(AlarmType.OpenedInviteFriendsPopUp.ToString(), 0) == 0)
			{
				if (AlarmSystem.Instance != null)
				{
					PlayerPrefs.SetInt(AlarmType.OpenedInviteFriendsPopUp.ToString(), 1);
					PlayerPrefs.Save();
					AlarmSystem.Instance.SetAlarmState(AlarmType.OpenedInviteFriendsPopUp, false);
				}
			}
		}

		private void ResetActivateButton()
		{
			_activateButton.interactable = false;
			_activateButton.onClick.RemoveAllListeners();
			_activateButton.onClick.AddListener(ActivateReferralCode);
		}

		private void SetActivateFirstActivationInfo(bool active)
		{
			_firstRewardActivationInfo.SetActive(active);
		}

		private void ActivateReferralCode()
		{
			_referralSystem.ActivateReferralCode(_referralCodeInputField.text);
		}

		private void ResetShareButton()
		{
			_shareButton.onClick.RemoveAllListeners();
			_shareButton.onClick.AddListener(ShareReferralCode);
		}

		private void ShareReferralCode()
		{
			ReferralSystem.Share();
		}

		private void ResetReferralCodeInputField()
		{
			_referralCodeInputField.onValueChanged.RemoveAllListeners();
			_referralCodeInputField.onValueChanged.AddListener(ChangeActivateButtonState);
		}

		public void ResetView()
		{
			var referralSystemData = UserDataService.GetCachedCustomUserData(CustomUserDataKey.referral_system);
			var properties = JsonConvert.DeserializeObject<JObject>(referralSystemData);

			properties ??= new JObject();

			var subscribedTo = properties[JsonProperty.REFERRAL_SUBSCRIBED_TO] + "";

			var followersAmount = properties[JsonProperty.REFERRAL_FOLLOWERS_AMOUNT] + "";

			if (subscribedTo == null || subscribedTo == "")
			{
				subscribedTo = MessageCode.NOT_ACTIVATED_YET.ToString(); //LocalizationSystem
                SetActivateFirstActivationInfo(true);
			}
			else
			{
				SetActivateFirstActivationInfo(false);
			}

			if (followersAmount == null || followersAmount == "")
			{
				followersAmount = "0";
			}

			UpdateView(subscribedTo, followersAmount);

			ResetInviteRewards(followersAmount);
		}

		private void ResetInviteRewards(string followersAmount)
		{
			var referralInviteRewardsData = UserDataService.GetCachedTitlePublicConfig(TitleDataKey.ReferralInviteRewards);
			var userReadOnlyReferralInviteRewardsData = UserDataService.GetCachedCustomUserData(CustomUserDataKey.referral_invite_rewards);

			var rewards = JsonConvert.DeserializeObject<JArray>(referralInviteRewardsData);
			rewards ??= new JArray();

			for (int i = 0; i < rewards.Count; i++)
			{
				var reward = rewards[i][JsonProperty.REWARD];

				if (reward != null)
				{
					var item = _inviteRewardsView.Rewards.First(x => x.FollowersAmount == (int)rewards[i][JsonProperty.REFERRAL_FOLLOWERS_AMOUNT]);

                    string imagePath = reward[JsonProperty.IMAGE_PATH].ToString();
                    var iconPath = (imagePath == JsonProperty.TOKEN_IMAGE_PATH) ? IGSUserData.Currency.CurrencyData.Find(c => c.CurrencyCode == "IG")?.ImageUrl ?? JsonProperty.TOKEN_IMAGE_PATH : imagePath;

                    item.Set(iconPath, (int)reward[JsonProperty.AMOUNT]);
				}
			}

			var UserInviteRewardsData = JsonConvert.DeserializeObject<JArray>(userReadOnlyReferralInviteRewardsData);

			UserInviteRewardsData ??= new JArray();

			foreach (var inviteRewardItem in _inviteRewardsView.Rewards)
			{
				inviteRewardItem.SetActiveCheckMark(false);
			}

			for (int i = 0; i < UserInviteRewardsData.Count; i++)
			{
				bool.TryParse((string)UserInviteRewardsData[i][JsonProperty.IS_GRANTED], out bool isGranted);

				var item = _inviteRewardsView.Rewards.First(x => x.FollowersAmount == (int)UserInviteRewardsData[i][JsonProperty.REFERRAL_FOLLOWERS_AMOUNT]);

				item.SetActiveCheckMark(isGranted);
			}

			int.TryParse(followersAmount, out int invites);

			_inviteRewardsView.SetSliderValue(invites);
		}

		public void UpdateView(string subscribedTo, string followersAmount)
		{
			UpdateSubscribedToText(subscribedTo);
			UpdateNumberOfReferralsText(followersAmount);

			_currentUserReferralCode.text = AuthService.UserID;
		}

		public void UpdateSubscribedToText(string subscribedTo)
		{
			_subscribedTo.text = subscribedTo;
		}

		private void UpdateNumberOfReferralsText(string followersAmount)
		{
			_numberOfFollowers.text = followersAmount;
		}

		public void OnSuccessActivated()
		{
			UpdateSubscribedToText(_referralCodeInputField.text);
			_referralCodeInputField.text = string.Empty;
		}

		private bool IsValidInputCode(string input)
		{
			if (input == string.Empty)
			{
				return false;
			}

			if (input.Length < MIN_USER_ID_LENGHT)
			{
				return false;
			}

			if (input == AuthService.UserID)
			{
				return false;
			}

			if (input == _subscribedTo.text)
			{
				return false;
			}

			return true;
		}

		private void ChangeActivateButtonState(string input)
		{
			_activateButton.interactable = IsValidInputCode(input);
		}
	}
}