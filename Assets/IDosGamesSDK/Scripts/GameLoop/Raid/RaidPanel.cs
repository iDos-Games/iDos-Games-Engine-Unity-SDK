using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using IDosGames.ClientModels;
using System;
using System.Threading.Tasks;

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

        [Header("Raid Grid (9 ячеек, 0..8)")]
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

        private void OnEnable() => GameLoopService.OnBoardRaidSuccess += HandleRaidResult;
        private void OnDisable() => GameLoopService.OnBoardRaidSuccess -= HandleRaidResult;

        // -----------------------------------------------------------------------
        public void Show(RollActionData target)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            root.SetActive(true);

            _target = target;
            _interactable = true;

            targetNameText.text = target?.PublicData?.Username ?? "???";
            _ = LoadAvatar(target?.PublicData?.AvatarUrl);

            var pending = GameLoopData.Instance?.BoardState?.Pending;
            int attempts = pending != null ? (9 - (pending.OpenedIndices?.Count ?? 0)) : 3;
            long stolen = pending?.CurrentTotalStolen ?? 0;

            UpdateHUD(attempts, stolen);
            InitCells(pending);

            resultOverlay.SetActive(false);
            StartCoroutine(FadeIn());
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

                if (alreadyOpened && pending?.RaidLayout != null && i < pending.RaidLayout.Count)
                    symbol = pending.RaidLayout[i];

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

        private void OnCellSelected(int digIndex)
        {
            if (!_interactable) return;
            _interactable = false;

            cells.ForEach(c => c?.SetInteractable(false));

            _ = SendRaidRequest(digIndex);
        }

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
            StartCoroutine(ProcessRaidResult(response));
        }

        private IEnumerator ProcessRaidResult(RaidResponse response)
        {
            // Анимируем открытие ячейки
            var cell = GetCell(response.OpenedIndex);
            if (cell != null)
            {
                Sprite foundSprite = GetSymbolSprite(response.FoundSymbol);
                yield return StartCoroutine(cell.RevealRoutine(foundSprite));
            }

            // Обновляем HUD
            UpdateHUD(response.AttemptsLeft, response.TotalStolen);

            bool finished = response.Status != "CONTINUE";

            if (finished)
            {
                // Показываем остаток раскладки
                if (response.RaidLayout != null)
                    RevealAllCells(response.RaidLayout);

                yield return new WaitForSeconds(0.8f);
                yield return StartCoroutine(ShowRaidResult(response));
            }
            else
            {
                // Ещё ходы есть — снова даём выбирать
                _interactable = true;
                cells.ForEach(c => c?.SetInteractable(true));

                // Уже открытые повторно блокируем
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
            resultAmountText.text = $"+ {response.TotalStolen:N0} монет";

            resultOverlay.SetActive(true);
            yield return StartCoroutine(PunchScale(resultOverlay.transform, 0.3f));
            yield return new WaitForSeconds(resultShowDuration);
            yield return StartCoroutine(FadeOut());
            root.SetActive(false);
        }

        // -----------------------------------------------------------------------
        private void UpdateHUD(int attemptsLeft, long totalStolen)
        {
            if (attemptsText != null) attemptsText.text = $"Попыток: {attemptsLeft}";
            if (totalStolenText != null) totalStolenText.text = $"Украдено: {totalStolen:N0}";
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
        // Общие анимации
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
