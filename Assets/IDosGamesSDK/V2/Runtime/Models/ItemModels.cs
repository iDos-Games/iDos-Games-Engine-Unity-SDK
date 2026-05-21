using System;

namespace IDosGames
{
    /// <summary>
    /// Request for the Item module.
    /// Required fields per action:
    /// - UpgradeLevel: <see cref="ItemInstanceID"/>.
    /// </summary>
    [Serializable]
    public class ItemRequest : BaseRequest
    {
        /// <summary>
        /// Unstackable item instance ID — key in <see cref="UserInventoryState.UnstackableItems"/>.
        /// Must not contain '.' or '$' (would break the MongoDB path on the server).
        /// </summary>
        public string ItemInstanceID { get; set; }
    }

    /// <summary>
    /// Response for <see cref="ItemAction.UpgradeLevel"/>. On idempotent replay with the same
    /// RelatedEntityID the server returns the same payload as the original successful call.
    /// </summary>
    [Serializable]
    public class UpgradeItemLevelResponse
    {
        /// <summary>Server execution time (UTC).</summary>
        public DateTime ServerTimeUtc { get; set; }

        /// <summary>ID of the upgraded instance (key in <see cref="UserInventoryState.UnstackableItems"/>).</summary>
        public string ItemInstanceID { get; set; }

        /// <summary>Catalog item ID (<see cref="ItemDefinition.ItemID"/>).</summary>
        public string ItemID { get; set; }

        /// <summary>CatalogID the item belongs to (<see cref="ItemDefinition.CatalogID"/>).</summary>
        public string CatalogID { get; set; }

        /// <summary>New instance level after the upgrade. Mirrors <see cref="UnstackableItemInstanceState.Level"/>.</summary>
        public int Level { get; set; }

        /// <summary>
        /// Consumed resources (cost) of the upgrade. Listed under
        /// <c>Resources.Consume.Standard.Entries</c> / <c>Resources.Consume.Standard.EventTokens</c>.
        /// </summary>
        public ResourceOperation Resources { get; set; } = new();
    }

    /// <summary>
    /// Item module actions.
    /// </summary>
    public enum ItemAction
    {
        /// <summary>Upgrade an unstackable instance level by +1, atomically charging the scaled cost.</summary>
        UpgradeLevel,
    }
}
