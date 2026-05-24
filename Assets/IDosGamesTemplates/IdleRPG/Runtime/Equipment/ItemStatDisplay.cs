using System;
using UnityEngine;

namespace IDosGames
{
    /// <summary>
    /// Inspector mapping from an <see cref="ItemStats.FlatBonuses"/> / <see cref="ItemStats.PercentBonuses"/>
    /// key (e.g. <c>"AttackDamage"</c>, <c>"HP"</c>) to a display label and icon for
    /// <see cref="EquipmentInfoPopup"/>'s stat rows.
    /// </summary>
    [Serializable]
    public class ItemStatDisplay
    {
        public string Key;
        public string DisplayName;
        public Sprite Icon;
    }
}
