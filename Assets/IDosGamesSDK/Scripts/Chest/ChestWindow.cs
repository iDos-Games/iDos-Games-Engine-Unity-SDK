using System;
using UnityEngine;

namespace IDosGames
{
    public class ChestWindow : MonoBehaviour
    {
        [SerializeField] private ChestRewardRoom _rewardRoom;
        [SerializeField] private SpinWindow _spinWindow;

        public static string SOMETHING_WENT_WRONG_CONTACT_TECHNICAL_SUPPORT;

        private ChestRarityType _chestRarity;

        public void OpenSpinWindow()
        {
            if (_spinWindow != null)
            {
                _spinWindow.OpenStandardSpin();
            }
        }

        public void TryToOpenChest(ChestRarityType rarity)
        {
            ServerFunctionHandlers functionName = rarity switch
            {
                ChestRarityType.Common => ServerFunctionHandlers.GetCommonChestReward,
                ChestRarityType.Rare => ServerFunctionHandlers.GetRareChestReward,
                ChestRarityType.Legendary => ServerFunctionHandlers.GetLegendaryChestReward,
                _ => throw new ArgumentOutOfRangeException(nameof(rarity), rarity, null)
            };

            _chestRarity = rarity;

            _ = IGSClientAPI.ExecuteFunction(
                functionName: functionName,
                resultCallback: OnSuccessResponseChestResult,
                notConnectionErrorCallback: OnErrorResponseOpenChest
            );
        }


        private void OnSuccessResponseChestResult(string result)
        {
            if (result != null)
            {
                var itemID = result.ToString();
                //Debug.Log(itemID);
                var item = UserDataService.GetCachedSkinItem(itemID);

                _rewardRoom.ShowReward(_chestRarity, item);

                UserDataService.RequestUserAllData();
            }
            else
            {
                Message.Show(MessageCode.SOMETHING_WENT_WRONG);
            }
        }

        private void OnErrorResponseOpenChest(string error)
        {
            Message.Show(MessageCode.SOMETHING_WENT_WRONG.ToString() + " " + error?.ToString()); //LocalizationSystem
        }
    }
}