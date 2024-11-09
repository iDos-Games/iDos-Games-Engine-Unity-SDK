using IDosGames.ClientModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace IDosGames
{
    public class ReferralSystemAppmetrica : MonoBehaviour
    {
        public static string ReferralLink => GetReferralLink();

        [SerializeField] private InviteFriendsPopUp _popUp;

        private const string REFERRAL_CODE_PARAMETER_TRACKER_LINK = "referral_code";

        private const string CLOUD_FUNCTION_ARGUMENT_REFERRAL_CODE = "ReferralCode";

        private const string SHARE_DESCRIPTION = "Install the app from this link and get rewarded from me: ";

        private void Start()
        {
            if (!string.IsNullOrEmpty(Application.absoluteURL))
            {
                OnDeepLinkActivated(Application.absoluteURL);
            }
        }

        private void OnEnable()
        {
            Application.deepLinkActivated += OnDeepLinkActivated;
            UserDataService.CustomUserDataUpdated += _popUp.ResetView;
        }

        private void OnDisable()
        {
            Application.deepLinkActivated -= OnDeepLinkActivated;
            UserDataService.CustomUserDataUpdated -= _popUp.ResetView;
        }

        public static void Share()
        {
            Debug.Log(ReferralLink);

            new NativeShare().SetSubject(Application.productName)
                .SetText(SHARE_DESCRIPTION).SetUrl(ReferralLink)
                .SetCallback((result, shareTarget) => Debug.Log("Share result: " + result + ", selected app: " + shareTarget))
                .Share();
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

        private static string GetReferralLink()
        {
            return IDosGamesSDKSettings.Instance.ReferralTrackerLink + $"{REFERRAL_CODE_PARAMETER_TRACKER_LINK}={AuthService.UserID}";
        }

        private void OnDeepLinkActivated(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                string[] parts = url.Split('?');

                if (parts.Length > 1)
                {
                    string parameters = parts[1];
                    string[] paramPairs = parameters.Split('&');

                    foreach (string paramPair in paramPairs)
                    {
                        string[] keyValue = paramPair.Split('=');
                        if (keyValue.Length == 2)
                        {
                            string key = keyValue[0];
                            string value = keyValue[1];

                            if (key == REFERRAL_CODE_PARAMETER_TRACKER_LINK)
                            {
                                ActivateReferralCode(value);
                                break;
                            }
                        }
                    }
                }
            }
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
                        _popUp.OnSuccessActivated();
                    }
                }
            }
            else
            {
                Message.Show(MessageCode.SOMETHING_WENT_WRONG);
            }
        }

        private void OnActivateResultCallback(CloudScriptModels.ExecuteFunctionResult result)
        {
            if (result != null && result.FunctionResult != null)
            {
                JObject json = JsonConvert.DeserializeObject<JObject>(result.FunctionResult.ToString());

                if (json != null)
                {
                    var message = json[JsonProperty.MESSAGE_KEY].ToString();

                    Message.Show(message);

                    if (message == MessageCode.REFERRAL_MESSAGE_CODE_SUCCESS_ACTIVATED.ToString() ||
                        message == MessageCode.REFERRAL_MESSAGE_CODE_SUCCESS_CHANGED.ToString())
                    {
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
    }
}