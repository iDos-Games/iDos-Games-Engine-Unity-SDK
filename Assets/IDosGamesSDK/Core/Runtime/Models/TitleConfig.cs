using System;

namespace IDosGames
{
    public class TitleConfig
    {
        public TitlePublicConfigurationModel TitlePublicConfiguration { get; private set; } = new ();

        public event Action OnAnyUpdated;
        public event Action OnTitlePublicConfigurationUpdated;
        public event Action OnTitleCustomDataUpdated;
        public event Action OnCurrencyDefinitionsUpdated;
        public event Action OnItemDefinitionsUpdated;

        public event Action OnCharacterDefinitionsUpdated;
        public event Action OnTimedEventDefinitionsUpdated;
        public event Action OnQuestDefinitionsUpdated;
        public event Action OnSeasonDefinitionsUpdated;
        public event Action OnLeaderboardDefinitionsUpdated;
        public event Action OnCollectionDefinitionsUpdated;
        public event Action OnCoopEventDefinitionsUpdated;
        public event Action OnCraftDefinitionsUpdated;
        public event Action OnDealOfferDefinitionsUpdated;
        public event Action OnGameLoopDefinitionsUpdated;
        public event Action OnLootboxDefinitionsUpdated;
        public event Action OnPremiumDefinitionsUpdated;
        public event Action OnReferralDefinitionsUpdated;
        public event Action OnRewardDefinitionsUpdated;
        public event Action OnStoreDefinitionsUpdated;
        public event Action OnTimedBoostDefinitionsUpdated;
        public event Action OnUserCustomDataDefinitionsUpdated;

        internal TitleConfig() { }

        internal void PatchCharacter(CharacterDefinitions data)
        {
            TitlePublicConfiguration ??= new ();
            TitlePublicConfiguration.Character = data;
            OnCharacterDefinitionsUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplyTitlePublicConfiguration(TitlePublicConfigurationModel data)
        {
            TitlePublicConfiguration = data;
            OnTitlePublicConfigurationUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchTitleCustomData(TitleCustomDataResponse data)
        {
            TitlePublicConfiguration.TitleCustomData = data;
            OnTitleCustomDataUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchItemDefinitions(ItemDefinitions data)
        {
            TitlePublicConfiguration.Item = data;
            OnItemDefinitionsUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchCurrencyDefinitions(CurrencyDefinitions data)
        {
            TitlePublicConfiguration.Currency = data;
            OnCurrencyDefinitionsUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchTimedEvent(TimedEventDefinitions data)
        {
            TitlePublicConfiguration ??= new();
            TitlePublicConfiguration.TimedEvent = data;
            OnTimedEventDefinitionsUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchQuest(QuestDefinitions data)
        {
            TitlePublicConfiguration ??= new();
            TitlePublicConfiguration.Quest = data;
            OnQuestDefinitionsUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchSeason(SeasonDefinitions data)
        {
            TitlePublicConfiguration ??= new();
            TitlePublicConfiguration.Season = data;
            OnSeasonDefinitionsUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchLeaderboard(LeaderboardDefinitions data)
        {
            TitlePublicConfiguration ??= new();
            TitlePublicConfiguration.Leaderboard = data;
            OnLeaderboardDefinitionsUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchCollection(CollectionDefinitions data)
        {
            TitlePublicConfiguration ??= new();
            TitlePublicConfiguration.Collection = data;
            OnCollectionDefinitionsUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchCoopEvent(CoopEventDefinitions data)
        {
            TitlePublicConfiguration ??= new();
            TitlePublicConfiguration.CoopEvent = data;
            OnCoopEventDefinitionsUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchCraft(CraftDefinitions data)
        {
            TitlePublicConfiguration ??= new();
            TitlePublicConfiguration.Craft = data;
            OnCraftDefinitionsUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchDealOffer(DealOfferDefinitions data)
        {
            TitlePublicConfiguration ??= new();
            TitlePublicConfiguration.DealOffer = data;
            OnDealOfferDefinitionsUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchGameLoop(GameLoopDefinitions data)
        {
            TitlePublicConfiguration ??= new();
            TitlePublicConfiguration.GameLoop = data;
            OnGameLoopDefinitionsUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchBoardDefinition(BoardLoopDefinition data)
        {
            TitlePublicConfiguration ??= new();
            TitlePublicConfiguration.GameLoop ??= new GameLoopDefinitions();
            TitlePublicConfiguration.GameLoop.Board = data;
            OnGameLoopDefinitionsUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchLootbox(LootboxDefinitions data)
        {
            TitlePublicConfiguration ??= new();
            TitlePublicConfiguration.Lootbox = data;
            OnLootboxDefinitionsUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchPremium(PremiumDefinitions data)
        {
            TitlePublicConfiguration ??= new();
            TitlePublicConfiguration.Premium = data;
            OnPremiumDefinitionsUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchReferral(ReferralDefinitions data)
        {
            TitlePublicConfiguration ??= new();
            TitlePublicConfiguration.Referral = data;
            OnReferralDefinitionsUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchReward(RewardDefinitions data)
        {
            TitlePublicConfiguration ??= new TitlePublicConfigurationModel();
            TitlePublicConfiguration.Reward = data;
            OnRewardDefinitionsUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchStore(StoreDefinitions data)
        {
            TitlePublicConfiguration ??= new();
            TitlePublicConfiguration.Store = data;
            OnStoreDefinitionsUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchTimedBoost(TimedBoostDefinitions data)
        {
            TitlePublicConfiguration ??= new();
            TitlePublicConfiguration.TimedBoost = data;
            OnTimedBoostDefinitionsUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchUserCustomData(UserCustomDataDefinitions data)
        {
            TitlePublicConfiguration ??= new();
            TitlePublicConfiguration.UserCustomData = data;
            OnUserCustomDataDefinitionsUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }
    }
}
