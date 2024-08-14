//using Firebase.DynamicLinks;
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

        public string firebaseBaseDynamicLink;
        public string firebaseDynamicLinkURIPrefix;

        [SerializeField] private InviteFriendsPopUp _popUp;

        private const string CLOUD_FUNCTION_ARGUMENT_REFERRAL_CODE = "ReferralCode";

        private static string _referralLinkDescription;

        [Obsolete]
        private void OnEnable()
        {
            //DynamicLinks.DynamicLinkReceived += OnDynamicLink;
            UserDataService.UserReadOnlyDataUpdated += _popUp.ResetView;
        }

        [Obsolete]
        private void OnDisable()
        {
            //DynamicLinks.DynamicLinkReceived -= OnDynamicLink;
            UserDataService.UserReadOnlyDataUpdated -= _popUp.ResetView;
        }

        [Obsolete]
        private void Start()
        {
            //CreateReferralDynamicLink();
        }

        private void OnDynamicLink(object sender, EventArgs args)
        {
            /*
            var dynamicLinkEventArgs = args as ReceivedDynamicLinkEventArgs;

            Debug.LogFormat("Received dynamic link {0}", dynamicLinkEventArgs.ReceivedDynamicLink.Url.OriginalString);

            string link = dynamicLinkEventArgs.ReceivedDynamicLink.Url.OriginalString;

            string playfabID = link.Split('=').Last();

            if (playfabID != string.Empty)
            {
                ActivateReferralCode(playfabID);
            }
            */
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

        [Obsolete]
        private void CreateReferralDynamicLink()
        {
            /*
            var baseLink = firebaseBaseDynamicLink;
            var uriPrefix = firebaseDynamicLinkURIPrefix;

            var iosParameters = new IOSParameters(IDosGamesSDKSettings.Instance.IosBundleID)
            {
                AppStoreId = IDosGamesSDKSettings.Instance.IosAppStoreID
            };

            var components = new DynamicLinkComponents(

            new Uri(baseLink + "?Referral_ID=" + AuthService.UserID),
                    uriPrefix)
            {
                IOSParameters = iosParameters,
                AndroidParameters = new AndroidParameters(IDosGamesSDKSettings.Instance.AndroidBundleID),
            };

            ReferralLink = components.LongDynamicLink.ToString();

            var options = new DynamicLinkOptions
            {
                PathLength = DynamicLinkPathLength.Unguessable
            };

            DynamicLinks.GetShortLinkAsync(components, options).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("GetShortLinkAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("GetShortLinkAsync encountered an error: " + task.Exception);
                    return;
                }

                ShortDynamicLink link = task.Result;

                ReferralLink = link.Url.ToString();
            });
            */
        }

        public static void Share()
        {
            _referralLinkDescription = "Play and earn! Install the game using the link and receive a gift"; //LocalizationSystem

            new NativeShare().SetSubject(Application.productName)
                .SetText(_referralLinkDescription).SetUrl(ReferralLink)
                .SetCallback((result, shareTarget) => Debug.Log("Share result: " + result + ", selected app: " + shareTarget))
                .Share();
        }
    }
}