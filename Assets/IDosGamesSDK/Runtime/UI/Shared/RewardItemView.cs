using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames.UI
{
    public class RewardItemView : MonoBehaviour
    {
        [SerializeField] private Image           _icon;
        [SerializeField] private TextMeshProUGUI _amountText;

        // ── Standard setup — resolves label and icon automatically ────────

        public void Setup(ResourceEntry reward)
        {
            bool isCurrency = reward.Type == null || reward.Type == ResourceEntryType.VirtualCurrency;
            long amount     = reward.Amount ?? 0;

            string label = isCurrency
                ? (amount > 0 ? $"+{amount}" : string.Empty)
                : (amount > 1 ? $"x{amount}"  : string.Empty);

            ApplyLabel(label);
            LoadIconAsync(ResolveIconPath(reward));
        }

        // ─────────────────────────────────────────────────────────────────

        private void ApplyLabel(string label)
        {
            if (_amountText != null) _amountText.text = label;
            gameObject.SetActive(true);
        }

        private async void LoadIconAsync(string path)
        {
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                var sprite = await ImageLoader.GetSpriteAsync(path);
                if (this != null && _icon != null && sprite != null)
                    _icon.sprite = sprite;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[RewardItemView] Icon load failed: {ex.Message}");
            }
        }

        private static string ResolveIconPath(ResourceEntry reward)
        {
            var config = IDosGamesData.Config?.TitlePublicConfiguration;
            if (config == null) return null;

            if (reward.Type == ResourceEntryType.VirtualCurrency || reward.Type == null)
            {
                var currencies = config.Currency?.VirtualCurrencies;
                if (currencies != null &&
                    !string.IsNullOrEmpty(reward.CurrencyID) &&
                    currencies.TryGetValue(reward.CurrencyID, out var def))
                    return def.AssetPaths?.GetValueOrDefault("icon");
            }
            else if (reward.Type == ResourceEntryType.Item)
            {
                var catalogs = config.Item?.Catalogs;
                if (catalogs != null &&
                    !string.IsNullOrEmpty(reward.CatalogID) &&
                    catalogs.TryGetValue(reward.CatalogID, out var catalog) &&
                    catalog.Items != null &&
                    !string.IsNullOrEmpty(reward.ItemID) &&
                    catalog.Items.TryGetValue(reward.ItemID, out var itemDef))
                    return itemDef.AssetPaths?.GetValueOrDefault("icon");
            }

            return null;
        }
    }
}
