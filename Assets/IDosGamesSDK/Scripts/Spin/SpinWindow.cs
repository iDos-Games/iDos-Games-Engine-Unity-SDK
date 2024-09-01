using System;
using UnityEngine;

namespace IDosGames
{
    public class SpinWindow : MonoBehaviour
    {
        [SerializeField] private SpinWheel _spinWheel;
        [SerializeField] private SpinWindowView _spinWindowView;

        private void Start()
        {
            _spinWindowView.ResetSpinButtonsListener(TryToSpin);
        }

        private void OnEnable()
        {
            WebFunctionHandler.Instance.OnAdCompleteEvent += WebAdComplete;
        }

        private void OnDisable()
        {
            WebFunctionHandler.Instance.OnAdCompleteEvent -= WebAdComplete;
        }

        private void TryToSpin(SpinTicketType type)
        {
            var ticketAmount = UserInventory.GetSpinTicketAmount(type);

            if (ticketAmount <= 0)
            {
                ShopSystem.PopUpSystem.ShowSpinTicketPopUp();
                return;
            }

            ServerFunctionHandlers functionName = type switch
            {
                SpinTicketType.Standard => ServerFunctionHandlers.GetStandardSpinReward,
                SpinTicketType.Premium => ServerFunctionHandlers.GetPremiumSpinReward,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };

            ExecuteSpinCloudFunction(functionName);
        }

        public void TryToFreeSpin(bool showAd)
        {
            if (showAd) // && AdMediation.Instance != null
            {
                if (AdMediation.Instance != null)
                {
                    if (AdMediation.Instance.ShowRewardedVideo(GetFreeSpin))
                    {
                        Debug.Log("Show rewarded video.");
                    }
                    else
                    {
                        ///Message.Show("Ad is not ready.");
                        ShopSystem.PopUpSystem.ShowVIPPopUp();
                    }
                }
                else
                {
                    if (AuthService.WebGLPlatform == WebGLPlatform.Telegram)
                    {
#if UNITY_WEBGL
                        WebFunctionHandler.Instance.ShowAd(IDosGamesSDKSettings.Instance.AdsGramBlockID.ToString(), "FreeSpin");
#endif
                    }
                    else
                    {
                        ShopSystem.PopUpSystem.ShowVIPPopUp();
                    }
                }
            }
            else
            {
                GetFreeSpin();
            }
        }

        private void WebAdComplete(string args)
        {
            if (args == "FreeSpin")
            {
                GetFreeSpin();
            }
        }

        private void GetFreeSpin(bool adCompleted = true)
        {
            ExecuteSpinCloudFunction(ServerFunctionHandlers.GetFreeSpinReward);
        }

        private void ExecuteSpinCloudFunction(ServerFunctionHandlers functionName)
        {
            _ = IGSClientAPI.ExecuteFunction(
                functionName: functionName,
                resultCallback: OnSuccessResponseSpinResult,
                 notConnectionErrorCallback: (error) => OnErrorExecuteSpinCloudFunction()
                );

        }

        private void OnErrorExecuteSpinCloudFunction()
        {
            Message.Show(MessageCode.FAILED_TO_EXECUTE_SPIN_FUNCTION);
        }

        private void OnSuccessResponseSpinResult(string result)
        {
            if (result != null)
            {
                int.TryParse(result.ToString(), out int targetIndex);

                _spinWheel.Spin(targetIndex);

                //AdditionalDataService.RequestAllData();
                //Loading.HideAllPanels();
            }
            else
            {
                Message.Show(MessageCode.SOMETHING_WENT_WRONG);
            }
        }

        private void OnSuccessResponseSpinResult(CloudScriptModels.ExecuteFunctionResult result)
        {
            if (result != null && result.FunctionResult != null)
            {
                int.TryParse(result.FunctionResult.ToString(), out int targetIndex);

                _spinWheel.Spin(targetIndex);
            }
            else
            {
                Message.Show(MessageCode.SOMETHING_WENT_WRONG);
            }
        }

        public void OpenFreeSpin()
        {
            ActivateMainWindow();
            _spinWindowView.ShowSpinView(SpinTicketType.Free);
        }

        public void OpenStandardSpin()
        {
            ActivateMainWindow();
            _spinWindowView.ShowSpinView(SpinTicketType.Standard);
        }

        private void ActivateMainWindow()
        {
            gameObject.SetActive(true);
        }
    }
}