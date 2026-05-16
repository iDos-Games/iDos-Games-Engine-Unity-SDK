using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using IDosGames.TitlePublicConfiguration;

namespace IDosGames
{
    public class BoardHexRingRenderer : MonoBehaviour
    {
        [Header("Spawn Root")]
        [SerializeField] private RectTransform tilesRoot;
        [SerializeField] private BoardHexTileView tilePrefab;

        [Header("Tile Size")]
        [SerializeField] private float hexRadius = 70f;

        [Header("Pseudo Hex Ring")]
        [SerializeField] private float ringRadius = 320f;
        [SerializeField] private float angleOffset = 90f; // 90 = первый тайл сверху
        [SerializeField] private bool rotateTilesAlongPath = false;
        [SerializeField] private bool autoRenderOnBoardReady = true;

        private readonly List<BoardHexTileView> _views = new();
        private readonly Dictionary<int, BoardHexTileView> _viewByTileIndex = new();
        private List<BoardTileDefinition> _orderedTiles = new();

        private int? _highlightedTileIndex;

        public int TileCount => _views.Count;

        private void OnEnable()
        {
            if (BoardGameManager.Instance != null && autoRenderOnBoardReady)
                BoardGameManager.Instance.OnBoardReady += RenderFromCurrentTemplate;

            if (autoRenderOnBoardReady && BoardGameManager.Instance != null && BoardGameManager.Instance.IsBoardReady)
                RenderFromCurrentTemplate();
        }

        private void OnDisable()
        {
            if (BoardGameManager.Instance != null && autoRenderOnBoardReady)
                BoardGameManager.Instance.OnBoardReady -= RenderFromCurrentTemplate;
        }

        [ContextMenu("Render From Current Template")]
        public void RenderFromCurrentTemplate()
        {
            ClearSpawnedTiles();

            var template = BoardGameManager.Instance?.CurrentTemplate;
            if (template?.Tiles == null || template.Tiles.Count == 0)
            {
                Debug.LogWarning("[BoardHexRingRenderer] CurrentTemplate or Tiles is empty");
                return;
            }

            _orderedTiles = template.Tiles
                .Where(t => t != null)
                .OrderBy(t => t.Index)
                .ToList();

            int count = _orderedTiles.Count;
            if (count == 0)
                return;

            float width = Mathf.Sqrt(3f) * hexRadius;
            float height = 2f * hexRadius;

            for (int i = 0; i < count; i++)
            {
                var tile = _orderedTiles[i];
                var view = Instantiate(tilePrefab, tilesRoot);
                var rect = view.RectTransform;

                rect.sizeDelta = new Vector2(width, height);
                rect.anchoredPosition = GetPseudoHexRingPosition(i, count);

                if (rotateTilesAlongPath)
                {
                    float z = GetPseudoHexRingRotation(i, count);
                    rect.localRotation = Quaternion.Euler(0f, 0f, z);
                }
                else
                {
                    rect.localRotation = Quaternion.identity;
                }

                view.Bind(tile);

                _views.Add(view);
                _viewByTileIndex[tile.Index] = view;
            }

            if (BoardGameManager.Instance?.BoardState != null)
                SetHighlightedTileIndex(BoardGameManager.Instance.BoardState.Position);
        }

        private Vector2 GetPseudoHexRingPosition(int index, int totalCount)
        {
            if (totalCount <= 1)
                return Vector2.zero;

            Vector2[] corners = GetHexCorners();

            // распредел€ем тайлы по 6 сторонам равномерно
            float u = ((float)index / totalCount) * 6f;
            int segment = Mathf.FloorToInt(u) % 6;
            float localT = u - Mathf.Floor(u);

            Vector2 a = corners[segment];
            Vector2 b = corners[(segment + 1) % 6];

            return Vector2.Lerp(a, b, localT);
        }

        private float GetPseudoHexRingRotation(int index, int totalCount)
        {
            if (totalCount <= 1)
                return 0f;

            Vector2 prev = GetPseudoHexRingPosition(WrapIndex(index - 1, totalCount), totalCount);
            Vector2 next = GetPseudoHexRingPosition(WrapIndex(index + 1, totalCount), totalCount);

            Vector2 dir = (next - prev).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            // если визуально надо довернуть тайлы, мен€й +30 / -30 / -90
            return angle - 90f;
        }

        private Vector2[] GetHexCorners()
        {
            // pointy-top hex: top -> top-right -> bottom-right -> bottom -> bottom-left -> top-left
            Vector2[] result = new Vector2[6];

            for (int i = 0; i < 6; i++)
            {
                float angleDeg = angleOffset - i * 60f;
                float angleRad = angleDeg * Mathf.Deg2Rad;

                float x = Mathf.Cos(angleRad) * ringRadius;
                float y = Mathf.Sin(angleRad) * ringRadius;

                result[i] = new Vector2(x, y);
            }

            return result;
        }

        public int WrapIndex(int index)
        {
            return WrapIndex(index, _views.Count);
        }

        private int WrapIndex(int index, int count)
        {
            if (count <= 0)
                return 0;

            return ((index % count) + count) % count;
        }

        public int NormalizeTileIndex(int tileIndex)
        {
            if (_orderedTiles == null || _orderedTiles.Count == 0)
                return 0;

            return ((tileIndex % _orderedTiles.Count) + _orderedTiles.Count) % _orderedTiles.Count;
        }

        public bool TryGetPositionByTileIndex(int tileIndex, out Vector2 position)
        {
            if (_viewByTileIndex.TryGetValue(tileIndex, out var view) && view != null)
            {
                position = view.RectTransform.anchoredPosition;
                return true;
            }

            position = Vector2.zero;
            return false;
        }

        public bool TryGetTileViewByTileIndex(int tileIndex, out BoardHexTileView view)
        {
            return _viewByTileIndex.TryGetValue(tileIndex, out view) && view != null;
        }

        public void SetHighlightedTileIndex(int tileIndex)
        {
            ClearHighlight();

            if (_viewByTileIndex.TryGetValue(tileIndex, out var view) && view != null)
            {
                view.SetHighlighted(true);
                _highlightedTileIndex = tileIndex;
            }
        }

        public void ClearHighlight()
        {
            if (_highlightedTileIndex.HasValue &&
                _viewByTileIndex.TryGetValue(_highlightedTileIndex.Value, out var oldView) &&
                oldView != null)
            {
                oldView.SetHighlighted(false);
            }

            _highlightedTileIndex = null;
        }

        private void ClearSpawnedTiles()
        {
            for (int i = 0; i < _views.Count; i++)
            {
                if (_views[i] == null)
                    continue;

                if (Application.isPlaying)
                    Destroy(_views[i].gameObject);
                else
                    DestroyImmediate(_views[i].gameObject);
            }

            _views.Clear();
            _viewByTileIndex.Clear();
            _orderedTiles.Clear();
            _highlightedTileIndex = null;
        }
    }
}