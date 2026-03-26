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
        public Currencies CurrencyData { get; private set; }
        public Dictionary<string, List<CatalogItem>> Catalogs { get; private set; } = new();
        
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

        internal void ApplyCurrencyData(Currencies data)
        {
            CurrencyData = data;
            OnCurrencyDataUpdated?.Invoke();
            OnAnyUpdated?.Invoke();
        }
    }
}
