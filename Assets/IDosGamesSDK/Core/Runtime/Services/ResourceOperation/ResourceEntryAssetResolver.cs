using System.Collections.Generic;

namespace IDosGames
{
    public static class ResourceEntryAssetResolver
    {
        private const string PreferredAssetKey = "icon";
        private const string UnknownDisplayName = "Unknown";

        public static (string AssetPath, string DisplayName) Resolve(ResourceEntry entry)
        {
            if (entry == null)
            {
                return (null, UnknownDisplayName);
            }

            var config = IDosGamesData.Config?.TitlePublicConfiguration;

            switch (entry.Type)
            {
                case ResourceEntryType.Item:
                    return ResolveItem(config, entry);

                case ResourceEntryType.VirtualCurrency:
                    return ResolveVirtualCurrency(config, entry);

                case ResourceEntryType.CryptoCurrency:
                    return ResolveCryptoCurrency(config, entry);

                default:
                    return (null, FallbackName(entry));
            }
        }

        private static (string AssetPath, string DisplayName) ResolveItem(TitlePublicConfigurationModel config, ResourceEntry entry)
        {
            var catalogs = config?.Item?.Catalogs;

            if (catalogs == null || string.IsNullOrEmpty(entry.CatalogID) || string.IsNullOrEmpty(entry.ItemID))
            {
                return (null, FallbackName(entry));
            }

            if (!catalogs.TryGetValue(entry.CatalogID, out var catalog) || catalog?.Items == null)
            {
                return (null, FallbackName(entry));
            }

            if (!catalog.Items.TryGetValue(entry.ItemID, out var definition) || definition == null)
            {
                return (null, FallbackName(entry));
            }

            var name = !string.IsNullOrWhiteSpace(definition.DisplayName) ? definition.DisplayName : FallbackName(entry);
            return (PickAssetPath(definition.AssetPaths), name);
        }

        private static (string AssetPath, string DisplayName) ResolveVirtualCurrency(TitlePublicConfigurationModel config, ResourceEntry entry)
        {
            var currencies = config?.Currency?.VirtualCurrencies;

            if (currencies == null || string.IsNullOrEmpty(entry.CurrencyID))
            {
                return (null, FallbackName(entry));
            }

            if (!currencies.TryGetValue(entry.CurrencyID, out var definition) || definition == null)
            {
                return (null, FallbackName(entry));
            }

            var name = !string.IsNullOrWhiteSpace(definition.DisplayName) ? definition.DisplayName : FallbackName(entry);
            return (PickAssetPath(definition.AssetPaths), name);
        }

        private static (string AssetPath, string DisplayName) ResolveCryptoCurrency(TitlePublicConfigurationModel config, ResourceEntry entry)
        {
            var currencies = config?.Currency?.CryptoCurrencies;

            if (currencies == null || string.IsNullOrEmpty(entry.CurrencyID))
            {
                return (null, FallbackName(entry));
            }

            if (!currencies.TryGetValue(entry.CurrencyID, out var definition) || definition == null)
            {
                return (null, FallbackName(entry));
            }

            var name = !string.IsNullOrWhiteSpace(definition.DisplayName) ? definition.DisplayName : FallbackName(entry);
            return (PickAssetPath(definition.AssetPaths), name);
        }

        private static string PickAssetPath(Dictionary<string, string> assetPaths)
        {
            if (assetPaths == null || assetPaths.Count == 0)
            {
                return null;
            }

            if (assetPaths.TryGetValue(PreferredAssetKey, out var preferred) && !string.IsNullOrWhiteSpace(preferred))
            {
                return preferred;
            }

            foreach (var value in assetPaths.Values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return null;
        }

        private static string FallbackName(ResourceEntry entry)
        {
            if (!string.IsNullOrWhiteSpace(entry.ItemID)) return entry.ItemID;
            if (!string.IsNullOrWhiteSpace(entry.CurrencyID)) return entry.CurrencyID;
            return UnknownDisplayName;
        }
    }
}
