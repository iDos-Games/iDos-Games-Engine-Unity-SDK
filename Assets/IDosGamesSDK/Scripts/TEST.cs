using System.Collections.Generic;
using UnityEngine;

namespace IDosGames
{
    public class TEST : MonoBehaviour
    {
        public void Test()
        {
            GameLoop();
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
            _ = QuestService.GetUserQuestState();
            //_ = QuestService.AddProgress("Metric1", 1);
            _ = QuestService.ClaimQuestReward("Quest1", "Daily");
            //_ = QuestService.ClaimMilestoneReward("Daily", "3");
        }

        public void GameLoop()
        {
            //_ = GameLoopService.GetBoardDefinitionForLevel(1);
            _ = GameLoopService.BoardLoopBuild(0);
        }
    }
}
