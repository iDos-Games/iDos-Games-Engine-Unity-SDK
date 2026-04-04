using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using IDosGames.ClientModels;
using System;
using System.Threading.Tasks;
using IDosGames.TitlePublicConfiguration;

namespace IDosGames
{
    public class RaidPanel : MonoBehaviour
    {
        public static RaidPanel Instance { get; private set; }

        [Header("Root")]
        [SerializeField] private GameObject root;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Target")]
        [SerializeField] private TMP_Text targetNameText;
        [SerializeField] private Image targetAvatarImage;

        [Header("Raid Grid")]
        [SerializeField] private List<RaidCell> cells; // ровно 9

        [Header("HUD")]
        [SerializeField] private TMP_Text attemptsText;   // "Попыток: 3"
        [SerializeField] private TMP_Text totalStolenText; // "Украдено: 4 200"

        [Header("Result")]
        [SerializeField] private GameObject resultOverlay;
        [SerializeField] private TMP_Text resultTitleText;
        [SerializeField] private TMP_Text resultAmountText;
        [SerializeField] private float resultShowDuration = 2.5f;

        [Header("Symbol Sprites")]
        [SerializeField] private Sprite symbolNoneSprite;
        [SerializeField] private Sprite symbolSmallSprite;
        [SerializeField] private Sprite symbolMediumSprite;
        [SerializeField] private Sprite symbolBigSprite;

        [Header("Animation")]
        [SerializeField] private float fadeInDuration = 0.3f;

        private bool _interactable = true;
        private RollActionData _target;

        // ==================== Fast mode state ====================
        private bool _isFastMode;
        private List<HeistSymbol> _localLayout;
        private List<int> _localOpenedIndices;
        private int _localSmallCount;
        private int _localMediumCount;
        private int _localBigCount;

        // -----------------------------------------------------------------------
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            root.SetActive(false);
        }

        private void OnEnable()
        {
            GameLoopService.OnBoardRaidSuccess += HandleRaidResult;
        }

        private void OnDisable()
        {
            GameLoopService.OnBoardRaidSuccess -= HandleRaidResult;
        }

        // -----------------------------------------------------------------------
        public void Show(RollActionData target)
        {
            ResetState();

            _target = target;

            root.SetActive(true);
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = true;

            targetNameText.text = target?.PublicData?.Username ?? "???";
            _ = LoadAvatar(target?.PublicData?.AvatarUrl);

            resultOverlay.SetActive(false);

            // Определяем режим
            var boardDef = GameLoopData.Instance?.BoardDefinition;
            bool configIsSequential = boardDef != null && boardDef.RaidMode == RaidMode.Sequential;

            if (configIsSequential)
            {
                // Sequential — используем уже имеющийся pending, без доп. запроса
                InitSequentialAndShow();
            }
            else
            {
                // Fast — сначала получаем свежий стейт с layout, потом показываем
                StartCoroutine(InitFastAndShowCoroutine());
            }
        }

        // -----------------------------------------------------------------------
        // Sequential — мгновенная инициализация (как было раньше)
        // -----------------------------------------------------------------------
        private void InitSequentialAndShow()
        {
            _isFastMode = false;

            var pending = GameLoopData.Instance?.BoardState?.Pending;
            int attempts = pending != null ? (12 - (pending.OpenedIndices?.Count ?? 0)) : 3;

            UpdateHUD(attempts, null);
            InitCells(pending);

            _interactable = true;
            StartCoroutine(FadeIn());
        }

        // -----------------------------------------------------------------------
        // Fast — запрашиваем GetUserBoardState, получаем layout, потом показываем
        // -----------------------------------------------------------------------
        private IEnumerator InitFastAndShowCoroutine()
        {
            // Показываем панель, но без интерактивности — пока загружаем данные
            // (canvasGroup.alpha = 0, blocksRaycasts = true, чтобы заблокировать клики под панелью)

            bool requestDone = false;
            BoardLoopState freshState = null;
            bool requestFailed = false;

            _ = FetchBoardStateAsync(
                (state) => { freshState = state; requestDone = true; },
                () => { requestFailed = true; requestDone = true; }
            );

            // Ждём ответ сервера
            while (!requestDone)
                yield return null;

            if (requestFailed || freshState == null)
            {
                Debug.LogWarning("[RaidPanel] Failed to fetch board state for Fast raid. Falling back to Sequential.");
                // Фоллбэк на Sequential с тем что есть
                InitSequentialAndShow();
                yield break;
            }

            var pending = freshState.Pending;
            bool hasLayout = pending?.RaidLayout != null && pending.RaidLayout.Count > 0;

            if (!hasLayout)
            {
                Debug.LogWarning("[RaidPanel] Server returned no RaidLayout. Falling back to Sequential.");
                InitSequentialAndShow();
                yield break;
            }

            // Инициализируем Fast-режим с актуальными данными
            _isFastMode = true;
            InitFastMode(pending);

            int attempts = 12 - (_localOpenedIndices?.Count ?? 0);
            UpdateHUD(attempts, null);
            InitCells(pending);

            _interactable = true;
            StartCoroutine(FadeIn());
        }

        private async Task FetchBoardStateAsync(Action<BoardLoopState> onSuccess, Action onFail)
        {
            try
            {
                var result = await GameLoopService.GetUserBoardState();

                if (result != null && result.Success && result.Data != null)
                {
                    onSuccess?.Invoke(result.Data);
                }
                else
                {
                    Debug.LogWarning($"[RaidPanel] GetUserBoardState failed: {result?.Error}");
                    onFail?.Invoke();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RaidPanel] GetUserBoardState exception: {ex}");
                onFail?.Invoke();
            }
        }

        // -----------------------------------------------------------------------
        private void ResetState()
        {
            _isFastMode = false;
            _localLayout = null;
            _localOpenedIndices = null;
            _localSmallCount = 0;
            _localMediumCount = 0;
            _localBigCount = 0;
            _target = null;
            _interactable = false;
        }

        // -----------------------------------------------------------------------
        private void InitFastMode(BoardPendingInteraction pending)
        {
            _localLayout = new List<HeistSymbol>(pending.RaidLayout);
            _localOpenedIndices = pending.OpenedIndices != null
                ? new List<int>(pending.OpenedIndices)
                : new List<int>();

            _localSmallCount = 0;
            _localMediumCount = 0;
            _localBigCount = 0;

            foreach (var idx in _localOpenedIndices)
            {
                if (idx >= 0 && idx < _localLayout.Count)
                    IncrementSymbolCount(_localLayout[idx]);
            }
        }

        private void IncrementSymbolCount(HeistSymbol symbol)
        {
            switch (symbol)
            {
                case HeistSymbol.Small: _localSmallCount++; break;
                case HeistSymbol.Medium: _localMediumCount++; break;
                case HeistSymbol.Big: _localBigCount++; break;
            }
        }

        private string CheckLocalFinish()
        {
            if (_localSmallCount >= 3) return "SMALL";
            if (_localMediumCount >= 3) return "MEDIUM";
            if (_localBigCount >= 3) return "BIG";
            return null;
        }

        // -----------------------------------------------------------------------
        private void InitCells(BoardPendingInteraction pending)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                if (cell == null) continue;

                bool alreadyOpened = pending?.OpenedIndices != null &&
                                     pending.OpenedIndices.Contains(i);

                HeistSymbol symbol = HeistSymbol.None;

                if (alreadyOpened)
                {
                    if (_isFastMode)
                    {
                        if (_localLayout != null && i < _localLayout.Count)
                            symbol = _localLayout[i];
                    }
                    else
                    {
                        if (pending?.RaidLayout != null && i < pending.RaidLayout.Count)
                            symbol = pending.RaidLayout[i];
                    }
                }

                cell.Bind(
                    digIndex: i,
                    alreadyOpened: alreadyOpened,
                    symbol: symbol,
                    symbolSprite: GetSymbolSprite(symbol),
                    hiddenSprite: symbolNoneSprite,
                    onSelected: OnCellSelected
                );
            }
        }

        // -----------------------------------------------------------------------
        private void OnCellSelected(int digIndex)
        {
            if (!_interactable) return;
            _interactable = false;

            cells.ForEach(c => c?.SetInteractable(false));

            if (_isFastMode)
            {
                ProcessFastDig(digIndex);
            }
            else
            {
                _ = SendRaidRequest(digIndex);
            }
        }

        // -----------------------------------------------------------------------
        // === FAST MODE ===
        // -----------------------------------------------------------------------
        private void ProcessFastDig(int digIndex)
        {
            if (_localLayout == null || digIndex < 0 || digIndex >= _localLayout.Count)
            {
                Debug.LogWarning("[RaidPanel] Fast mode but no layout, falling back to server.");
                _ = SendRaidRequest(digIndex);
                return;
            }

            if (_localOpenedIndices.Contains(digIndex))
            {
                UnlockPanel();
                return;
            }

            HeistSymbol foundSymbol = _localLayout[digIndex];
            _localOpenedIndices.Add(digIndex);
            IncrementSymbolCount(foundSymbol);

            int attemptsLeft = 12 - _localOpenedIndices.Count;
            string tier = CheckLocalFinish();

            StartCoroutine(ProcessFastDigCoroutine(digIndex, foundSymbol, attemptsLeft, tier));
        }

        private IEnumerator ProcessFastDigCoroutine(int digIndex, HeistSymbol foundSymbol, int attemptsLeft, string tier)
        {
            var cell = GetCell(digIndex);
            if (cell != null)
            {
                Sprite foundSprite = GetSymbolSprite(foundSymbol);
                yield return StartCoroutine(cell.RevealRoutine(foundSprite));
            }

            UpdateHUD(attemptsLeft, null);

            if (tier != null)
            {
                RevealAllCells(_localLayout);
                yield return new WaitForSeconds(0.8f);
                yield return StartCoroutine(SendFastRaidAndShowResult(tier));
            }
            else
            {
                UnlockPanel();
            }
        }

        private IEnumerator SendFastRaidAndShowResult(string tier)
        {
            bool requestDone = false;
            RaidResponse serverResponse = null;
            bool requestFailed = false;

            _ = SendFastRaidRequestAsync(
                new List<int>(_localOpenedIndices),
                (response) => { serverResponse = response; requestDone = true; },
                () => { requestFailed = true; requestDone = true; }
            );

            while (!requestDone)
                yield return null;

            if (requestFailed || serverResponse == null)
            {
                Debug.LogWarning("[RaidPanel] Fast raid server request failed.");
                yield return StartCoroutine(FadeOut());
                root.SetActive(false);
                yield break;
            }

            yield return StartCoroutine(ShowFastRaidResult(tier, serverResponse.StolenResource));
        }

        private async Task SendFastRaidRequestAsync(
            List<int> digIndices,
            Action<RaidResponse> onSuccess,
            Action onFail)
        {
            try
            {
                var result = await GameLoopService.BoardLoopRaidFast(digIndices);

                if (result != null && result.Success)
                {
                    onSuccess?.Invoke(result.Data);
                }
                else
                {
                    Debug.LogWarning($"[RaidPanel] Fast raid failed: {result?.Error}");
                    onFail?.Invoke();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RaidPanel] Fast raid exception: {ex}");
                onFail?.Invoke();
            }
        }

        private IEnumerator ShowFastRaidResult(string tier, ItemOrCurrency stolenResource)
        {
            string title = tier switch
            {
                "SMALL" => "Небольшой куш!",
                "MEDIUM" => "Неплохо!",
                "BIG" => "ДЖЕКПОТ!",
                _ => "Ограбление завершено"
            };

            resultTitleText.text = title;
            resultAmountText.text = stolenResource != null
                ? $"+ {stolenResource.Amount:N0} {stolenResource.Name ?? stolenResource.CurrencyID}"
                : "Ничего не украдено";

            resultOverlay.SetActive(true);
            yield return StartCoroutine(PunchScale(resultOverlay.transform, 0.3f));
            yield return new WaitForSeconds(resultShowDuration);
            yield return StartCoroutine(FadeOut());
            root.SetActive(false);
        }

        // -----------------------------------------------------------------------
        // === SEQUENTIAL MODE ===
        // -----------------------------------------------------------------------
        private async Task SendRaidRequest(int digIndex)
        {
            try
            {
                var result = await GameLoopService.BoardLoopRaid(digIndex);

                if (result == null || !result.Success)
                {
                    Debug.LogWarning("[RaidPanel] Raid request failed, unlocking panel.");
                    UnlockPanel();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RaidPanel] Raid request exception: {ex}");
                UnlockPanel();
            }
        }

        private void UnlockPanel()
        {
            _interactable = true;
            cells.ForEach(c =>
            {
                if (c != null && !c.IsOpened)
                    c.SetInteractable(true);
            });
        }

        private void HandleRaidResult(RaidResponse response)
        {
            if (_isFastMode) return;

            StartCoroutine(ProcessRaidResult(response));
        }

        private IEnumerator ProcessRaidResult(RaidResponse response)
        {
            var cell = GetCell(response.OpenedIndex);
            if (cell != null)
            {
                Sprite foundSprite = GetSymbolSprite(response.FoundSymbol);
                yield return StartCoroutine(cell.RevealRoutine(foundSprite));
            }

            UpdateHUD(response.AttemptsLeft, response.StolenResource);

            bool finished = response.Status != "CONTINUE";

            if (finished)
            {
                if (response.RaidLayout != null)
                    RevealAllCells(response.RaidLayout);

                yield return new WaitForSeconds(0.8f);
                yield return StartCoroutine(ShowRaidResult(response));
            }
            else
            {
                _interactable = true;
                cells.ForEach(c => c?.SetInteractable(true));

                for (int i = 0; i < cells.Count; i++)
                {
                    if (cells[i] != null && cells[i].IsOpened)
                        cells[i].SetInteractable(false);
                }
            }
        }

        private void RevealAllCells(List<HeistSymbol> layout)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                if (cells[i] == null || cells[i].IsOpened) continue;
                if (i < layout.Count)
                    cells[i].ForceReveal(GetSymbolSprite(layout[i]));
            }
        }

        private IEnumerator ShowRaidResult(RaidResponse response)
        {
            string title = response.Status switch
            {
                "FINISHED_SMALL" => "Небольшой куш!",
                "FINISHED_MEDIUM" => "Неплохо!",
                "FINISHED_BIG" => "ДЖЕКПОТ!",
                _ => "Ограбление завершено"
            };

            resultTitleText.text = title;
            resultAmountText.text = response.StolenResource != null
                ? $"+ {response.StolenResource.Amount:N0} {response.StolenResource.Name ?? response.StolenResource.CurrencyID}"
                : "Nothing was stolen";

            resultOverlay.SetActive(true);
            yield return StartCoroutine(PunchScale(resultOverlay.transform, 0.3f));
            yield return new WaitForSeconds(resultShowDuration);
            yield return StartCoroutine(FadeOut());
            root.SetActive(false);
        }

        // -----------------------------------------------------------------------
        private void UpdateHUD(int attemptsLeft, ItemOrCurrency stolenResource)
        {
            if (attemptsText != null) attemptsText.text = $"Attempts: {attemptsLeft}";

            if (totalStolenText != null) totalStolenText.text = stolenResource != null
                    ? $"Stolen: {stolenResource.Amount:N0}"
                    : "Stolen: 0";
        }

        private RaidCell GetCell(int digIndex)
        {
            return (digIndex >= 0 && digIndex < cells.Count) ? cells[digIndex] : null;
        }

        private Sprite GetSymbolSprite(HeistSymbol symbol) => symbol switch
        {
            HeistSymbol.Small => symbolSmallSprite,
            HeistSymbol.Medium => symbolMediumSprite,
            HeistSymbol.Big => symbolBigSprite,
            _ => symbolNoneSprite
        };

        // -----------------------------------------------------------------------
        // Анимации
        // -----------------------------------------------------------------------

        private IEnumerator FadeIn()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;

            float t = 0f;
            while (t < fadeInDuration)
            {
                t += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Clamp01(t / fadeInDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        private IEnumerator FadeOut()
        {
            float t = fadeInDuration;
            while (t > 0f)
            {
                t -= Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Clamp01(t / fadeInDuration);
                yield return null;
            }
            canvasGroup.alpha = 0f;

            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        private IEnumerator PunchScale(Transform target, float duration)
        {
            Vector3 original = target.localScale;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float scale = 1f + Mathf.Sin((t / duration) * Mathf.PI) * 0.3f;
                target.localScale = original * scale;
                yield return null;
            }
            target.localScale = original;
        }

        private async System.Threading.Tasks.Task LoadAvatar(string url)
        {
            if (string.IsNullOrEmpty(url) || targetAvatarImage == null) return;
            var sprite = await ImageLoader.GetSpriteAsync(url);
            if (sprite != null && targetAvatarImage != null)
                targetAvatarImage.sprite = sprite;
        }
    }
}
