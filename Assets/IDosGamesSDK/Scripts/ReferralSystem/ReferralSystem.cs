using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IDosGames
{
    public class ReferralSystem : MonoBehaviour
    {
        public static string ReferralLink { get; private set; }
        private static string ReferralCodeKey { get; set; }
        private bool ReferralCodeActivated;

        public string firebaseBaseDynamicLink;
        public string firebaseDynamicLinkURIPrefix;
        private static string _referralLinkDescription;

        [SerializeField] private InviteFriendsPopUp _popUp;

        [Obsolete]
        private void OnEnable()
        {
            UserDataService.CustomUserDataUpdated += _popUp.ResetView;
        }

        [Obsolete]
        private void OnDisable()
        {
            UserDataService.CustomUserDataUpdated -= _popUp.ResetView;
        }

        [Obsolete]
        private void Start()
        {
            ReferralCodeKey = "ReferralCodeActivated" + AuthService.UserID;
            LoadReferralCodeStatus();
            CreateReferralLink();
            if (!ReferralCodeActivated)
            {
                CheckReferral();
            }
        }

        private void CheckReferral()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            WebSDK.FetchStartAppParameter();
            string startAppValue = WebSDK.GetStartAppParameterValue();

            if (!string.IsNullOrEmpty(startAppValue))
            {
                ActivateReferralCode(startAppValue);
                //Message.Show(startAppValue);
            }
#endif
        }

        public void ActivateReferralCode(string code)
        {
            FunctionParameters parameter = new()
            {
                ReferralCode = code
            };

            _ = IGSClientAPI.ExecuteFunction(
                  functionName: ServerFunctionHandlers.ActivateReferralCode,
                  resultCallback: OnActivateResultCallback,
                  notConnectionErrorCallback: OnActivateErrorCallback,
                  functionParameter: parameter
                  );
        }

        private void OnActivateResultCallback(string result)
        {
            if (result != null)
            {
                JObject json = JsonConvert.DeserializeObject<JObject>(result.ToString());

                if (json != null)
                {
                    var message = json[JsonProperty.MESSAGE_KEY].ToString();

                    Message.Show(message);

                    if (message == MessageCode.REFERRAL_MESSAGE_CODE_SUCCESS_ACTIVATED.ToString() ||
                        message == MessageCode.REFERRAL_MESSAGE_CODE_SUCCESS_CHANGED.ToString())
                    {
                        ReferralCodeActivated = true;
                        SaveReferralCodeStatus();

                        _popUp.OnSuccessActivated();
                    }
                }
            }
            else
            {
                Message.Show(MessageCode.SOMETHING_WENT_WRONG);
            }
        }

        private void OnActivateErrorCallback(string error)
        {
            Message.Show(MessageCode.SOMETHING_WENT_WRONG);
        }

        private void CreateReferralLink()
        {
            string baseLink;
            string titleID = AuthService.GetTitleID();

            if (AuthService.WebGLPlatform == WebGLPlatform.Web)
            {
                baseLink = "https://idosgames.com/en/app/?id=" + titleID;
            }
            else if (AuthService.WebGLPlatform == WebGLPlatform.Telegram)
            {
                baseLink = IDosGamesSDKSettings.Instance.TelegramWebAppLink;
            }
            else
            {
                // Android and iOS link needs to be implemented here  
                baseLink = "https://idosgames.com/en/app/?id=" + titleID; //IDosGamesSDKSettings.Instance.ReferralTrackerLink;
            }

            // Check if the base link already contains parameters
            char separator = baseLink.Contains("?") ? '&' : '?';
            ReferralLink = $"{baseLink}{separator}startapp={AuthService.UserID}";
        }

        public static void Share()
        {
#if UNITY_ANDROID || UNITY_IOS
            _referralLinkDescription = "Play and earn! Install the game using the link and receive a gift"; //LocalizationSystem

            new NativeShare().SetSubject(Application.productName)
                .SetText(_referralLinkDescription).SetUrl(ReferralLink)
                .SetCallback((result, shareTarget) => Debug.Log("Share result: " + result + ", selected app: " + shareTarget))
                .Share();
#endif

#if UNITY_WEBGL
            if (AuthService.WebGLPlatform != WebGLPlatform.None)
            {
                WebSDK.ShareLink(ReferralLink);
            }
#endif
        }

        private void SaveReferralCodeStatus()
        {
            PlayerPrefs.SetInt(ReferralCodeKey, ReferralCodeActivated ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void LoadReferralCodeStatus()
        {
            if (PlayerPrefs.HasKey(ReferralCodeKey))
            {
                ReferralCodeActivated = PlayerPrefs.GetInt(ReferralCodeKey) == 1;
            }
            else
            {
                ReferralCodeActivated = false;
            }
        }
    }
}