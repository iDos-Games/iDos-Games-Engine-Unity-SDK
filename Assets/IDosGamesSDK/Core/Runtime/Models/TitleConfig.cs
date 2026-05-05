using System;
using System.Collections.Generic;

namespace IDosGames
{
    public class TitleConfig
    {
        public TitlePublicConfigurationModel TitlePublicConfiguration { get; private set; } = new ();
        public Dictionary<string, TitlePublicData> PublicCustomTitleData { get; private set; } = new ();
        public CurrencyDefinitions Currency { get; private set; } = new();
        public ItemDefinitions Item { get; private set; } = new ();

        public event Action OnTitlePublicConfigurationUpdated;
        public event Action OnPublicCustomTitleDataUpdated;
        public event Action OnCurrencyDefinitionsUpdated;
        public event Action OnItemDefinitionsUpdated;
        public event Action OnAnyUpdated;

        internal TitleConfig() { }

        internal void ApplyTitlePublicConfiguration(TitlePublicConfigurationModel data)
        {
            TitlePublicConfiguration = data;
            OnTitlePublicConfigurationUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplyPublicCustomTitleData(Dictionary<string, TitlePublicData> data)
        {
            PublicCustomTitleData = data;
            OnPublicCustomTitleDataUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplyCurrencyDefinitions(CurrencyDefinitions data)
        {
            Currency = data;
            OnCurrencyDefinitionsUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        internal void ApplyItemDefinitions(ItemDefinitions data)
        {
            Item = data;
            OnItemDefinitionsUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }

        //internal void PatchStoreDefinitions(StoreDefinitions data)
        //{
        //    TitlePublicConfiguration.Store = data;
        //    OnTitlePublicConfigurationUpdated?.Invoke();
        //    OnAnyUpdated?.Invoke();
        //}
    }
}
