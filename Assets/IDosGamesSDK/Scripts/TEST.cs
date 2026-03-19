using IDosGames.TitlePublicConfiguration;
using System.Collections.Generic;
using UnityEngine;

namespace IDosGames
{
    public class TEST : MonoBehaviour
    {
        public void Test()
        {
            
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

        public async void GameLoop()
        {
            //_ = await GameLoopService.GetBoardDefinitionForLevel(1);

            var result = await GameLoopService.BoardLoopRoll(1);
            var rewards = result.Data.GrantedRewards;
            Message.ShowRewards(rewards);
        }

        public void ShowTestRewards()
        {
            var rewards = new List<ItemOrCurrency>
            {
                new ItemOrCurrency
                {
                    Type = ItemType.VirtualCurrency,
                    CurrencyID = "Gold",
                    Name = "Gold",
                    Amount = 1000,
                    ImagePath = "Rewards/gold"
                },
                new ItemOrCurrency
                {
                    Type = ItemType.VirtualCurrency,
                    CurrencyID = "Gems",
                    Name = "Gems",
                    Amount = 250,
                    ImagePath = "Rewards/gems"
                },
                new ItemOrCurrency
                {
                    Type = ItemType.VirtualCurrency,
                    CurrencyID = "Energy",
                    Name = "Energy",
                    Amount = 50,
                    ImagePath = "Rewards/energy"
                },
                new ItemOrCurrency
                {
                    Type = ItemType.Item,
                    ItemID = "Hammer",
                    Name = "Hammer",
                    Amount = 3,
                    ImagePath = "Rewards/hammer"
                },
                new ItemOrCurrency
                {
                    Type = ItemType.Item,
                    ItemID = "DiceRoll",
                    Name = "Dice Roll",
                    Amount = 5,
                    ImagePath = "Rewards/dice"
                },
                new ItemOrCurrency
                {
                    Type = ItemType.Item,
                    ItemID = "Shield",
                    Name = "Shield",
                    Amount = 2,
                    ImagePath = "Rewards/shield"
                },
                new ItemOrCurrency
                {
                    Type = ItemType.Item,
                    ItemID = "Rocket",
                    Name = "Rocket",
                    Amount = 1,
                    ImagePath = "Rewards/rocket"
                },
                new ItemOrCurrency
                {
                    Type = ItemType.Item,
                    ItemID = "Chest",
                    Name = "Epic Chest",
                    Amount = 1,
                    ImagePath = "Rewards/chest_epic"
                },
                new ItemOrCurrency
                {
                    Type = ItemType.Item,
                    ItemID = "CardPack",
                    Name = "Card Pack",
                    Amount = 4,
                    ImagePath = "Rewards/card_pack"
                },
                new ItemOrCurrency
                {
                    Type = ItemType.Item,
                    ItemID = "Booster",
                    Name = "Score Booster",
                    Amount = 2,
                    ImagePath = "Rewards/booster"
                }
            };

            Message.ShowRewards(rewards);
        }
    }
}
