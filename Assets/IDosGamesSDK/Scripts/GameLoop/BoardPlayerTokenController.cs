using System.Collections;
using UnityEngine;
using IDosGames.ClientModels;
using System;
using System.Threading.Tasks;

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
        [SerializeField] private int rewardsPopupDelayMs = 500;

        private bool _isRolling;
        private Coroutine _moveRoutine;

        private void OnEnable()
        {
            if (GameLoopData.Instance != null)
                GameLoopData.Instance.OnBoardReady += SnapToCurrentPosition;
        }

        private void OnDisable()
        {
            if (GameLoopData.Instance != null)
                GameLoopData.Instance.OnBoardReady -= SnapToCurrentPosition;
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

        public void RollWithMultiplierFromUI(int multiplicator)
        {
            _ = RollWithMultiplier(multiplicator);
        }

        private async Task RollWithMultiplier(int multiplicator)
        {
            if (_isRolling)
                return;

            if (multiplicator <= 0)
                multiplicator = 1;

            if (ringRenderer == null || playerToken == null)
                return;

            _isRolling = true;

            try
            {
                var result = await GameLoopService.BoardLoopRoll(multiplicator);

                if (result == null || result.Data == null)
                {
                    Debug.LogWarning("[BoardPlayerTokenController] Roll result is null");
                    return;
                }

                await PlayRollAnimationAsync(result.Data);

                if (rewardsPopupDelayMs > 0)
                    await Task.Delay(rewardsPopupDelayMs);

                var rewards = result.Data.GrantedRewards;
                if (rewards != null && rewards.Count > 0)
                    Message.ShowRewards(rewards);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BoardPlayerTokenController] RollWithMultiplier failed: {ex}");
            }
            finally
            {
                _isRolling = false;
            }
        }

        private Task PlayRollAnimationAsync(BoardRollResponse response)
        {
            var tcs = new TaskCompletionSource<bool>();

            if (_moveRoutine != null)
                StopCoroutine(_moveRoutine);

            _moveRoutine = StartCoroutine(AnimateRoll(response, () => tcs.TrySetResult(true)));
            return tcs.Task;
        }

        private IEnumerator AnimateRoll(BoardRollResponse response, Action onComplete)
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
                    _moveRoutine = null;
                    onComplete?.Invoke();
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
            onComplete?.Invoke();
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
