using System.Collections.Generic;
using UnityEngine;

namespace IDosGames
{
    public class TEST : MonoBehaviour
    {
        public void Test()
        {
            Quest();
        }

        public void SaveValueToServer()
        {
            UserDataService.UpdateCustomUserData("test", "test value");
        }

        public void GetValue()
        {
            string value = UserDataService.GetCachedCustomUserData("test");
            Debug.Log(value);
        }

        public void GetToken()
        {
            ClaimRewardSystem.ClaimTokenReward(10, 1);
        }

        public void Craft()
        {
            List<string> itemIDs = new()
            {
                "avatar_hat_v1_01", "avatar_glasses_v3_01", "avatar_glasses_v4_01", "avatar_glasses_v5_01", "avatar_glasses_v5_01",
            };
            _ = CraftService.Craft("test", itemIDs, 2, 1);
        }

        public void Match()
        {
            _ = MatchService.GetAvailableMatches();
            //_ = MatchService.CreateMatch(50);
            //_ = MatchService.InstantBattle("698cf91dea662ed856763429");
        }

        public void Quest()
        {
            _ = QuestService.GetQuestDefinitions();
        }
    }
}
