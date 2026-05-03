using System;
using System.Collections.Generic;
using IDosGames.ClientModels;

namespace IDosGames
{
    public class TitleConfig
    {
        public TitlePublicConfigurationModel TitlePublicConfiguration { get; private set; } = new ();

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

        internal void ApplyCraftDefinitions(List<CraftDefinition> data)
        {
            //TitlePublicConfiguration.Craft = data;
            OnTitlePublicConfigurationUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchLimitedTimeEventsDefinition(LimitedTimeEventsDefinition data)
        {
            //TitlePublicConfiguration.LimitedTimeEvent = data;
            OnTitlePublicConfigurationUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchPremiumDefinitions(List<PremiumDefinition> data)
        {
            //TitlePublicConfiguration.Premium = data;
            OnTitlePublicConfigurationUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void PatchDealOfferDefinitions(DealOffersDefinition data)
        {
            //TitlePublicConfiguration.DealOffer = data;
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
