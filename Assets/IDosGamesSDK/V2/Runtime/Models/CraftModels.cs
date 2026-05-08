using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace IDosGames
{
    /// <summary>
    /// Request for all Craft actions.
    ///
    /// <para>GetDefinitions — no extra fields required.</para>
    /// <para>Craft — requires: <see cref="CraftID"/>, <see cref="InputItemIDs"/> (exactly RequiredItemCount entries).
    /// Optional: <see cref="Count"/> (1–20, default 1), <see cref="SelectedOptionID"/> (if null, first option is used),
    /// <see cref="RelatedEntityID"/> (idempotency key; auto-generated if omitted).</para>
    /// </summary>
    [Serializable]
    public class CraftRequest : BaseRequest
    {
        /// <summary>ID of the craft recipe to execute.</summary>
        public string CraftID;

        /// <summary>How many crafts to perform in one call (1–20, clamped server-side).</summary>
        public int Count = 1;

        /// <summary>
        /// Selected price option ID. If null/empty, the server picks the first available option.
        /// Ignored when the craft has no price options (free craft).
        /// </summary>
        public string SelectedOptionID;

        /// <summary>
        /// Template of item instance IDs to consume per craft.
        /// Must contain exactly <c>RequiredItemCount</c> entries.
        /// The server repeats this template <see cref="Count"/> times.
        /// </summary>
        public List<string> InputItemIDs = new();
    }

    // ── Responses ──────────────────────────────────────────────────────────

    /// <summary>Response for <see cref="CraftAction.GetDefinitions"/>.</summary>
    [Serializable]
    public class CraftDefinitionsResponse
    {
        public DateTime ServerTimeUtc;
        public CraftDefinitions CraftDefinitions;
    }

    /// <summary>Response for <see cref="CraftAction.Craft"/>.</summary>
    [Serializable]
    public class CraftResponse
    {
        public DateTime ServerTimeUtc;

        [JsonConverter(typeof(StringEnumConverter))]
        public CraftType Type;

        public string CraftID;

        /// <summary>How many crafts were actually performed.</summary>
        public int CraftedCount;

        /// <summary>Which price option was used (null = free craft).</summary>
        public string SelectedOptionID;

        public string InputRarity;
        public string OutputRarity;

        /// <summary>
        /// All resource changes for this operation.
        /// <c>Consume.Standard.Entries</c> — items and currency spent.
        /// <c>Grant.Standard.Entries</c> — items received.
        /// Use <see cref="CraftService.OnCraftCompleted"/> to react to applied tokens.
        /// </summary>
        public ResourceOperation Resources;

        /// <summary>Per-craft breakdown for UI/animations (what was burned, what dropped).</summary>
        public List<CraftSingleResult> Results = new();
    }

    /// <summary>Result of a single craft within a batch.</summary>
    [Serializable]
    public class CraftSingleResult
    {
        /// <summary>Zero-based index within the batch (0 .. CraftedCount-1).</summary>
        public int Index;

        /// <summary>Item instance IDs that were consumed in this craft.</summary>
        public List<string> BurnedItemIDs;

        /// <summary>Only for <see cref="CraftType.TradeUpCollection"/>: the collection that was traded up.</summary>
        public string RolledCollectionID;

        /// <summary>Only for <see cref="CraftType.TradeUpCollection"/>: how many items per collection were consumed.</summary>
        public Dictionary<string, int> UsedCollections;

        /// <summary>The item that dropped from this craft.</summary>
        public ResourceEntry Output;
    }

    // ── Action enum ─────────────────────────────────────────────────────────

    /// <summary>Available actions for the Craft endpoint.</summary>
    public enum CraftAction
    {
        GetDefinitions,
        Craft,
    }

    // ── Config models ───────────────────────────────────────────────────────

    /// <summary>Craft type — determines which matching/validation logic the server uses.</summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CraftType
    {
        /// <summary>Items from any collection, matched only by rarity.</summary>
        TradeUpRarity,

        /// <summary>Items from the same CollectionID, matched by collection and rarity.</summary>
        TradeUpCollection,
    }

    /// <summary>Root config container for all craft recipes in a title.</summary>
    [Serializable]
    public class CraftDefinitions
    {
        /// <summary>Key = CraftID.</summary>
        public Dictionary<string, CraftDefinition> Definitions = new();
    }

    /// <summary>Definition of a single craft recipe.</summary>
    [Serializable]
    public class CraftDefinition
    {
        public string CraftID;

        [JsonConverter(typeof(StringEnumConverter))]
        public CraftType Type;

        /// <summary>
        /// Catalog to search for input/output items.
        /// Null/empty = search all catalogs.
        /// </summary>
        public string CatalogID;

        /// <summary>
        /// For <see cref="CraftType.TradeUpCollection"/> only:
        /// Metadata.CollectionID that inputs and outputs must share.
        /// Ignored for <see cref="CraftType.TradeUpRarity"/>.
        /// </summary>
        public string CollectionID;

        /// <summary>RarityID of items the player must provide.</summary>
        public string InputRarityID;

        /// <summary>RarityID of items the player may receive.</summary>
        public string OutputRarityID;

        /// <summary>How many item IDs the client must send per craft (typically 10).</summary>
        public int RequiredItemCount = 10;

        /// <summary>
        /// Payment options keyed by OptionID.
        /// Empty = craft is free (no currency or item cost besides input items).
        /// </summary>
        public Dictionary<string, CraftPriceOption> PriceOptions = new();
    }

    /// <summary>One payment option for a craft recipe.</summary>
    [Serializable]
    public class CraftPriceOption
    {
        /// <summary>Stable key matching <see cref="CraftRequest.SelectedOptionID"/>.</summary>
        public string OptionID;

        /// <summary>
        /// Cost per single craft (Standard items/currencies + optional PremiumDiscounts).
        /// The server multiplies amounts by <see cref="CraftRequest.Count"/> automatically.
        /// Null = free option.
        /// </summary>
        public ResourceConsume RequiredResources;
    }
}
