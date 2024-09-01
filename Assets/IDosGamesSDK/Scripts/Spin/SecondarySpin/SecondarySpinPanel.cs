using UnityEngine;

namespace IDosGames
{
    public class SecondarySpinPanel : MonoBehaviour
    {
        [SerializeField] private SecondarySpinPanelView _view;

        private void OnEnable()
        {
            UserInventory.InventoryUpdated += ResetSpinButton;
            WebFunctionHandler.Instance.OnAdCompleteEvent += WebAdComplete;

            ResetSpinButton();
        }

        private void OnDisable()
        {
            UserInventory.InventoryUpdated -= ResetSpinButton;
            WebFunctionHandler.Instance.OnAdCompleteEvent -= WebAdComplete;
        }

        public void ShowInterstitial()
        {
            if (AdMediation.Instance != null)
            {
                if (!UserInventory.HasVIPStatus)
                {
                    AdMediation.Instance.ShowInterstitialAd();
                }
            }
        }

        public void TryToSpin(bool showAd)
        {
#if UNITY_EDITOR
            showAd = false;
#endif
            if (showAd)
            {
#if UNITY_WEBGL

                if (AuthService.WebGLPlatform == WebGLPlatform.Telegram)
                {
                    WebFunctionHandler.Instance.ShowAd(IDosGamesSDKSettings.Instance.AdsGramBlockID.ToString(), "SecondarySpin");
                }
                else
                {
                    ShopSystem.PopUpSystem.ShowVIPPopUp();
                }
#else
                if (AdMediation.Instance.ShowRewardedVideo((finished) => TryExecuteSpinFunction()))
                {
                    Debug.Log("Show rewarded video.");
                    return;
                }
                else
                {
                    Message.Show(MessageCode.AD_IS_NOT_READY);
                    ShopSystem.PopUpSystem.ShowVIPPopUp();
                    return;
                }
#endif
            }
            else
            {
                int ticketAmount = UserInventory.GetVirtualCurrencyAmount(VirtualCurrencyID.SS);

                if (ticketAmount <= 0)
                {
                    ShopSystem.PopUpSystem.ShowCoinPopUp();
                }
                else
                {
                    TryExecuteSpinFunction();
                }
            }
        }

        public void TryToSpinCoin()
        {
            int coinAmount = UserInventory.GetVirtualCurrencyAmount(VirtualCurrencyID.CO);

            if (coinAmount >= 100)
            {
                TryExecuteSpinFunctionForVC();
            }
            else
            {
                ShopSystem.PopUpSystem.ShowCoinPopUp();
            }
        }

        private void ResetSpinButton()
        {
            _view.ResetSpinButton(TryToSpin);
        }

        private void WebAdComplete(string args)
        {
            if (args == "SecondarySpin")
            {
                TryExecuteSpinFunction();
            }
        }

        private void TryExecuteSpinFunction()
        {
            _ = IGSClientAPI.ExecuteFunction(
                functionName: ServerFunctionHandlers.GetSecondarySpinReward,
                resultCallback: OnSuccessResponseSpinResult,
                  notConnectionErrorCallback: (error) => OnErrorExecuteSpinCloudFunction()
                );
        }

        private void OnSuccessResponseSpinResult(string result)
        {
            if (result != null)
            {
                int.TryParse(result.ToString(), out int targetIndex);

                _view.SpinWheel.Spin(targetIndex);

                Loading.HideAllPanels();
            }
            else
            {
                Message.Show(MessageCode.SOMETHING_WENT_WRONG);
            }
        }

        private void TryExecuteSpinFunctionForVC()
        {
            _ = IGSClientAPI.ExecuteFunction(
                 functionName: ServerFunctionHandlers.GetSecondarySpinRewardForVC,
                resultCallback: OnSuccessResponseSpinResult,
                  notConnectionErrorCallback: (error) => OnErrorExecuteSpinCloudFunction()
                );
        }

        private void OnErrorExecuteSpinCloudFunction()
        {
            Loading.HideAllPanels();
            Loading.UnblockTouch();
            Message.Show(MessageCode.FAILED_TO_EXECUTE_SPIN_FUNCTION);
        }

    }
}
