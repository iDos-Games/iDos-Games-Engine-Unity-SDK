using System;
using UnityEngine;

namespace IDosGames
{
    /// <summary>
    /// Per-rarity label visuals used by <see cref="CharacterDetailPanel"/> to display the
    /// rarity tag (e.g. "RARE" / "LEGENDARY") above the character. Independent from
    /// <see cref="CharacterRarityVisual"/> — only label-specific fields.
    /// </summary>
    [Serializable]
    public class CharacterRarityLabel
    {
        public string RarityID;
        public string DisplayName;   // shown text, e.g. "RARE"
        public Sprite LabelSprite;   // background sprite of the label
    }
}
