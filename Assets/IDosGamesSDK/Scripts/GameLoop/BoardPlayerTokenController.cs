using System.Collections;
using UnityEngine;
using IDosGames.ClientModels;

namespace IDosGames
{
    public class BoardPlayerTokenController : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private BoardHexRingRenderer ringRenderer;
        [SerializeField] private RectTransform playerToken;

        [Header("Move")]
        [SerializeField] private float stepDuration = 0.18f;
        [SerializeField] private float hopHeight = 24f;

        private Coroutine _moveRoutine;

        private void OnEnable()
        {
            if (GameLoopData.Instance != null)
                GameLoopData.Instance.OnBoardReady += SnapToCurrentPosition;

            GameLoopService.OnBoardRollSuccess += HandleRollSuccess;
        }

        private void OnDisable()
        {
            if (GameLoopData.Instance != null)
                GameLoopData.Instance.OnBoardReady -= SnapToCurrentPosition;

            GameLoopService.OnBoardRollSuccess -= HandleRollSuccess;
        }

        private void Start()
        {
            SnapToCurrentPosition();
        }

        public void SnapToCurrentPosition()
        {
            var data = GameLoopData.Instance;
            if (data == null || data.BoardState == null || ringRenderer == null || playerToken == null)
                return;

            int tileIndex = data.BoardState.Position;

            if (ringRenderer.TryGetPositionByTileIndex(tileIndex, out var pos))
            {
                playerToken.anchoredPosition = pos;
                ringRenderer.SetHighlightedTileIndex(tileIndex);
            }
            else
            {
                Debug.LogWarning($"[BoardPlayerTokenController] Tile position not found for tileIndex={tileIndex}");
            }
        }

        private void HandleRollSuccess(BoardRollResponse response)
        {
            if (response == null || ringRenderer == null || playerToken == null)
                return;

            if (_moveRoutine != null)
                StopCoroutine(_moveRoutine);

            _moveRoutine = StartCoroutine(AnimateRoll(response));
        }

        private IEnumerator AnimateRoll(BoardRollResponse response)
        {
            int totalSteps = Mathf.Max(0, response.Steps);
            int currentTileIndex = response.OldPosition;

            if (ringRenderer.TryGetPositionByTileIndex(currentTileIndex, out var startPos))
            {
                playerToken.anchoredPosition = startPos;
                ringRenderer.SetHighlightedTileIndex(currentTileIndex);
            }

            for (int i = 0; i < totalSteps; i++)
            {
                int nextTileIndex = ringRenderer.NormalizeTileIndex(currentTileIndex + 1);

                if (!ringRenderer.TryGetPositionByTileIndex(nextTileIndex, out var nextPos))
                {
                    Debug.LogWarning($"[BoardPlayerTokenController] Next tile not found: {nextTileIndex}");
                    yield break;
                }

                yield return MoveToken(playerToken.anchoredPosition, nextPos, stepDuration);

                currentTileIndex = nextTileIndex;
                ringRenderer.SetHighlightedTileIndex(currentTileIndex);
            }

            if (ringRenderer.TryGetPositionByTileIndex(response.NewPosition, out var finalPos))
            {
                playerToken.anchoredPosition = finalPos;
                ringRenderer.SetHighlightedTileIndex(response.NewPosition);
            }

            _moveRoutine = null;
        }

        private IEnumerator MoveToken(Vector2 from, Vector2 to, float duration)
        {
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = Mathf.Clamp01(time / duration);

                Vector2 pos = Vector2.Lerp(from, to, t);
                pos.y += Mathf.Sin(t * Mathf.PI) * hopHeight;

                playerToken.anchoredPosition = pos;
                yield return null;
            }

            playerToken.anchoredPosition = to;
        }
    }
}
