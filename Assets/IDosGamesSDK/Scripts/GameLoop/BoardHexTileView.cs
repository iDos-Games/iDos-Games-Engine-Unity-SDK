using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using IDosGames.TitlePublicConfiguration;
using BoardTileTypeConfig = IDosGames.TitlePublicConfiguration.BoardTileType;
using System.Collections.Generic;

namespace IDosGames
{
    public class BoardHexTileView : MonoBehaviour
    {
        [SerializeField] private Image background;
        [SerializeField] private GameObject highlight;
        [SerializeField] private TMP_Text indexText;
        [SerializeField] private TMP_Text typeText;
        [SerializeField] private TMP_Text rewardsText;

        [System.Serializable]
        private class TileSpriteMapping
        {
            public BoardTileTypeConfig Type;
            public Sprite Sprite;
        }

        [Header("Tile Sprites")]
        [SerializeField] private Sprite defaultSprite;
        [SerializeField] private List<TileSpriteMapping> tileSprites = new();

        public RectTransform RectTransform => (RectTransform)transform;
        public int TileIndex { get; private set; }

        private void Awake()
        {
            SetHighlighted(false);
        }

        public void Bind(BoardTileDefinition tile)
        {
            if (tile == null)
                return;

            TileIndex = tile.Index;
            indexText.text = tile.Index.ToString();
            typeText.text = tile.Type.ToString();
            rewardsText.text = BuildRewardsText(tile);

            if (background != null)
            {
                background.sprite = GetSprite(tile.Type);
                background.color = Color.white;
            }

            SetHighlighted(false);
        }

        public void SetHighlighted(bool value)
        {
            if (highlight != null)
                highlight.SetActive(value);
        }

        private string BuildRewardsText(BoardTileDefinition tile)
        {
            if (tile.TileRewards == null || tile.TileRewards.Count == 0)
                return "No rewards";

            var sb = new StringBuilder();

            for (int i = 0; i < tile.TileRewards.Count; i++)
            {
                var reward = tile.TileRewards[i];
                if (reward == null)
                    continue;

                string name =
                    !string.IsNullOrEmpty(reward.Name) ? reward.Name :
                    !string.IsNullOrEmpty(reward.CurrencyID) ? reward.CurrencyID :
                    !string.IsNullOrEmpty(reward.ItemID) ? reward.ItemID :
                    reward.Type?.ToString() ?? "Unknown";

                string amount = reward.Amount.HasValue ? $" x{reward.Amount.Value}" : "";

                if (sb.Length > 0)
                    sb.Append('\n');

                sb.Append(name).Append(amount);
            }

            return sb.ToString();
        }

        private Sprite GetSprite(BoardTileTypeConfig type)
        {
            if (tileSprites != null)
            {
                for (int i = 0; i < tileSprites.Count; i++)
                {
                    var entry = tileSprites[i];
                    if (entry != null && entry.Type == type && entry.Sprite != null)
                        return entry.Sprite;
                }
            }

            return defaultSprite;
        }
    }
}
