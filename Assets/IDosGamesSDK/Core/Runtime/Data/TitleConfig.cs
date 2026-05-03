using System;

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

        //internal void PatchStoreDefinitions(StoreDefinitions data)
        //{
        //    TitlePublicConfiguration.Store = data;
        //    OnTitlePublicConfigurationUpdated?.Invoke();
        //    OnAnyUpdated?.Invoke();
        //}
    }
}
