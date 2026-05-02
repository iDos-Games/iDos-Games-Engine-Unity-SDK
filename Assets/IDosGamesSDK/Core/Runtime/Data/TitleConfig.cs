using System;
using System.Collections.Generic;
using IDosGames.ClientModels;
using IDosGames.TitlePublicConfiguration;

namespace IDosGames
{
    public class TitleConfig
    {
        public TitlePublicConfigurationModel TitlePublicConfiguration { get; private set; } = new ();
        public PlatformSettingsModel PlatformSettings { get; private set; }
        public Dictionary<string, object> TitlePublicData { get; private set; }
        public Currencies Currencies { get; private set; }
        public Dictionary<string, List<CatalogItem>> Catalogs { get; private set; } = new();
        public LeaderboardDefinitions LeaderboardDefinitions => TitlePublicConfiguration.Leaderboard;

        public event Action OnTitlePublicConfigurationUpdated;
        public event Action OnTitlePublicDataUpdated;
        public event Action<string> OnCatalogUpdated; // string - CatalogVersion
        public event Action OnPlatformSettingsUpdated;
        public event Action OnCurrencyDataUpdated;
        public event Action OnAnyUpdated;

        internal TitleConfig() { }

        internal void ApplyTitlePublicConfiguration(TitlePublicConfigurationModel data)
        {
            TitlePublicConfiguration = data;
            OnTitlePublicConfigurationUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplyTitlePublicData(Dictionary<string, object> data)
        {
            TitlePublicData = data;
            OnTitlePublicDataUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplyCatalog(string catalogVersion, GetCatalogItemsResult data)
        {
            Catalogs[catalogVersion] = data?.Catalog;
            OnCatalogUpdated?.Invoke(catalogVersion);
            OnAnyUpdated?.Invoke();
        }

        public List<CatalogItem> GetCatalog(string version)
        {
            Catalogs.TryGetValue(version, out var items);
            return items;
        }

        internal void ApplyPlatformSettings(PlatformSettingsModel data)
        {
            PlatformSettings = data;
            OnPlatformSettingsUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplyCurrencies(Currencies data)
        {
            Currencies = data;
            OnCurrencyDataUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplyCraftDefinitions(List<CraftDefinition> data)
        {
            TitlePublicConfiguration.Craft = data;
            OnTitlePublicConfigurationUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplyGameLoops(GameLoopsDefinition data)
        {
            TitlePublicConfiguration.GameLoop = data;
            OnTitlePublicConfigurationUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplyDailyRewardsDefinitions(List<DailyRewardsDefinition> data)
        {
            TitlePublicConfiguration.DailyRewardsDefinitions = data;
            OnTitlePublicConfigurationUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplyLootboxDefinitions(List<LootboxDefinition> data)
        {
            TitlePublicConfiguration.Lootbox = data;
            OnTitlePublicConfigurationUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchBoardDefinition(BoardLoopDefinition data)
        {
            TitlePublicConfiguration.GameLoop ??= new GameLoopsDefinition();
            TitlePublicConfiguration.GameLoop.Board = data;
            OnTitlePublicConfigurationUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchCharacterDefinitions(CharacterDefinitions data)
        {
            TitlePublicConfiguration.Character = data;
            OnTitlePublicConfigurationUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchQuestDefinitions(QuestDefinitions data)
        {
            TitlePublicConfiguration.Quest = data;
            OnTitlePublicConfigurationUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchLimitedTimeEventsDefinition(LimitedTimeEventsDefinition data)
        {
            TitlePublicConfiguration.LimitedTimeEvent = data;
            OnTitlePublicConfigurationUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchPremiumDefinitions(List<PremiumDefinition> data)
        {
            TitlePublicConfiguration.Premium = data;
            OnTitlePublicConfigurationUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchDealOfferDefinitions(DealOffersDefinition data)
        {
            TitlePublicConfiguration.DealOffer = data;
            OnTitlePublicConfigurationUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchLeaderboardDefinitions(LeaderboardDefinitions data)
        {
            TitlePublicConfiguration.Leaderboard = data;
            OnTitlePublicConfigurationUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchReferralDefinitions(ReferralDefinitions data)
        {
            TitlePublicConfiguration.Referral = data;
            OnTitlePublicConfigurationUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchStoreDefinitions(StoreDefinitions data)
        {
            TitlePublicConfiguration.Store = data;
            OnTitlePublicConfigurationUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }
    }
}
