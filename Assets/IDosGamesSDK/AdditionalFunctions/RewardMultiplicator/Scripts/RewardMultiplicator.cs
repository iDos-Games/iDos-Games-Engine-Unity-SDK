using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace IDosGames
{
    public class RewardMultiplicator : MonoBehaviour
    {
        public int _multiplicatorIndex = 0;
        public float _multiplicator = 1.0f;
        public bool _shouldMove;

        public TMP_Text _multiplicatorText;
        public GameObject _popUpReward;

        public static int _currentCoinReward = 5;
        public static int _currentEventReward = 1;

        public GameObject[] _multiplicatorObjects;

        private void OnEnable()
        {
            _shouldMove = true;
            WebFunctionHandler.Instance.OnAdCompleteEvent += WebAdComplete;
        }

        private void OnDisable()
        {
            _shouldMove = false;
            WebFunctionHandler.Instance.OnAdCompleteEvent -= WebAdComplete;
        }

        public void GetReward()
        {
#if UNITY_EDITOR

            _shouldMove = false;
            OnFinishedWatchingRewardedVideo(true);

#elif UNITY_WEBGL
            if (AuthService.WebGLPlatform == WebGLPlatform.Telegram)
            {
                _shouldMove = false;
                WebFunctionHandler.Instance.ShowAd(IDosGamesSDKSettings.Instance.AdsGramBlockID.ToString(), "RewardMultiplicator");
            }
            else
            {
                _shouldMove = false;
                OnFinishedWatchingRewardedVideo(true);
            }
#else

            if (AdMediation.Instance != null)
            {
                if (AdMediation.Instance.ShowRewardedVideo(OnFinishedWatchingRewardedVideo))
                {
                    _shouldMove = false;
                    Debug.Log("Show rewarded video.");
                }
                else
                {
                    //For Testing
                    //_shouldMove = false;
                    //OnFinishedWatchingRewardedVideo(true);

                    Message.Show(MessageCode.AD_IS_NOT_READY);
                    ShopSystem.PopUpSystem.ShowVIPPopUp();
                }
            }
            else
            {
                Message.Show(MessageCode.AD_IS_NOT_READY);
                ShopSystem.PopUpSystem.ShowVIPPopUp();
            }
#endif
        }

        public void ClaimX5Reward()
        {
            if (DataService.HasVIPStatus)
            {
                _popUpReward.SetActive(false);
                ClaimRewardSystem.ClaimCoinReward(_currentCoinReward, 5, _currentEventReward);
            }
            else
            {
                ShopSystem.PopUpSystem.ShowVIPPopUp();
            }
        }

        private void Update()
        {
            if (_shouldMove)
            {
                for (int i = 0; i < _multiplicatorObjects.Length; i++)
                {
                    if (i == _multiplicatorIndex)
                    {
                        _multiplicatorObjects[i].transform.localScale = new Vector3(1.15f, 1.15f, 1.15f);
                    }
                    else
                    {
                        _multiplicatorObjects[i].transform.localScale = Vector3.one;
                    }
                }

                _multiplicatorText.text = " x" + _multiplicator;
            }
        }

        private void WebAdComplete(string args)
        {
            if (args == "RewardMultiplicator")
            {
                bool finished = true;
                OnFinishedWatchingRewardedVideo(finished);
            }
        }

        private void OnFinishedWatchingRewardedVideo(bool finished)
        {
            _popUpReward.SetActive(false);
            ClaimRewardSystem.ClaimCoinReward(_currentCoinReward, _multiplicator, _currentEventReward);
        }
    }
}
