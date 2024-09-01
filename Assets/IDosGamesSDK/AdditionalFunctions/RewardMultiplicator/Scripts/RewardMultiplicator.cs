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
            if (UserInventory.HasVIPStatus)
            {
                ClaimRewardSystem.ClaimX5CoinReward(_currentCoinReward, _currentEventReward);
                //WeeklyEventSystem.AddEventPoints(_currentEventReward * 5);

                _popUpReward.SetActive(false);

                RewardAnimations.ShowIgcAnimation();
                RewardAnimations.ShowEventPointAnimation();
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
            int coinReward = Mathf.CeilToInt(_currentCoinReward * _multiplicator);
            int eventPoints = Mathf.CeilToInt(_currentEventReward * _multiplicator);

            if(_multiplicator == 3f || _multiplicator == 2.5f)
            {
                ClaimRewardSystem.ClaimX3CoinReward(_currentCoinReward, _currentEventReward);
                //WeeklyEventSystem.AddEventPoints(eventPoints);

                _popUpReward.SetActive(false);

                RewardAnimations.ShowIgcAnimation();
                RewardAnimations.ShowEventPointAnimation();

                //Debug.Log("Coins: " + coinReward + "/ Points: " + eventPoints);
            }
            else
            {
                ClaimRewardSystem.ClaimCoinReward(coinReward, eventPoints);
                //WeeklyEventSystem.AddEventPoints(eventPoints);

                _popUpReward.SetActive(false);

                RewardAnimations.ShowIgcAnimation();
                RewardAnimations.ShowEventPointAnimation();

                //Debug.Log("Coins: " + coinReward + "/ Points: " + eventPoints);
            }
        }
    }
}