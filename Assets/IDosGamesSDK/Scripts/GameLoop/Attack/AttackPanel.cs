using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using IDosGames.ClientModels;
using System.Threading.Tasks;

namespace IDosGames
{
    public class AttackPanel : MonoBehaviour
    {
        public static AttackPanel Instance { get; private set; }

        [Header("Root")]
        [SerializeField] private GameObject root;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Target Info")]
        [SerializeField] private TMP_Text targetNameText;
        [SerializeField] private Image targetAvatarImage;

        [Header("Building Slots (кнопки для атаки)")]
        [SerializeField] private List<AttackBuildingSlot> buildingSlots; // по числу зданий на стейдже

        [Header("Result Overlay")]
        [SerializeField] private GameObject resultOverlay;
        [SerializeField] private TMP_Text resultTitleText;   // "HIT!" / "BLOCKED!"
        [SerializeField] private TMP_Text resultRewardText;  // "+ 12 500 coins"
        [SerializeField] private Image resultIcon;           // иконка молнии / щита
        [SerializeField] private Sprite hitSprite;
        [SerializeField] private Sprite blockedSprite;

        [Header("Animation")]
        [SerializeField] private float panelFadeInDuration = 0.3f;
        [SerializeField] private float resultShowDuration = 2.0f;
        [SerializeField] private float shakeAmplitude = 12f;
        [SerializeField] private float shakeDuration = 0.4f;

        private RollActionData _currentTarget;
        private bool _waitingForResult;

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

        private void OnEnable() => GameLoopService.OnBoardAttackSuccess += HandleAttackResult;
        private void OnDisable() => GameLoopService.OnBoardAttackSuccess -= HandleAttackResult;

        // -----------------------------------------------------------------------
        public void Show(RollActionData target)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            root.SetActive(true);

            _currentTarget = target;
            _waitingForResult = false;

            // Заполняем данные о цели
            targetNameText.text = target?.PublicData?.Username ?? "???";
            // Аватар подгружаем async (пример — адаптируй под свой ImageLoader)
            _ = LoadAvatar(target?.PublicData?.AvatarUrl);

            // Здания берём из текущего стейджа
            RefreshBuildingSlots();

            resultOverlay.SetActive(false);
            StartCoroutine(FadeIn());
        }

        private void RefreshBuildingSlots()
        {
            var stage = GameLoopData.Instance?.CurrentStage;
            var targetStates = _currentTarget?.TargetBuildingStates;

            for (int i = 0; i < buildingSlots.Count; i++)
            {
                var slot = buildingSlots[i];
                if (slot == null) continue;

                var def = stage?.Buildings?.Find(x => x?.SlotIndex == i);

                bool isValid = def != null;
                slot.gameObject.SetActive(isValid);

                if (!isValid) continue;

                var bldState = targetStates?.Find(x => x?.SlotIndex == i)
                               ?? new BuildingState { SlotIndex = i, Level = 0 };

                slot.Bind(i, def, bldState, OnBuildingSelected);
            }
        }

        private void OnBuildingSelected(int slotIndex)
        {
            if (_waitingForResult) return;
            _waitingForResult = true;

            // Блокируем все кнопки
            buildingSlots.ForEach(s => s?.SetInteractable(false));

            // Анимация выбора слота
            StartCoroutine(AnimateSelection(slotIndex));

            // Отправляем запрос
            _ = SendAttackRequest(slotIndex);
        }

        private async Task SendAttackRequest(int slotIndex)
        {
            try
            {
                var result = await GameLoopService.BoardLoopAttack(slotIndex);

                // Если запрос провалился — разблокируем панель
                if (result == null || !result.Success)
                {
                    Debug.LogWarning("[AttackPanel] Attack request failed, unlocking panel.");
                    UnlockPanel();
                }
                // Успех — придёт через HandleAttackResult via event
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AttackPanel] Attack request exception: {ex}");
                UnlockPanel();
            }
        }

        private void UnlockPanel()
        {
            _waitingForResult = false;
            buildingSlots.ForEach(s => s?.SetInteractable(true));
        }

        private void HandleAttackResult(AttackResponse response)
        {
            StartCoroutine(ShowResult(response));
        }

        // -----------------------------------------------------------------------
        // COROUTINES
        // -----------------------------------------------------------------------

        private IEnumerator AnimateSelection(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < buildingSlots.Count)
            {
                var slot = buildingSlots[slotIndex];
                if (slot != null)
                    yield return StartCoroutine(ShakeRoutine(slot.transform));
            }
        }

        private IEnumerator ShowResult(AttackResponse response)
        {
            bool isHit = response?.Status == "HIT";

            resultTitleText.text = isHit ? "HIT!" : "BLOCKED!";
            //resultRewardText.text = isHit ? $"+ {response.Reward:N0} coins" : "";
            resultIcon.sprite = isHit ? hitSprite : blockedSprite;

            resultOverlay.SetActive(true);

            // Анимация появления результата
            yield return StartCoroutine(PunchScale(resultOverlay.transform, 0.25f));
            yield return new WaitForSeconds(resultShowDuration);

            // Скрываем панель
            yield return StartCoroutine(FadeOut());
            root.SetActive(false);
        }

        private IEnumerator FadeIn()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;

            float t = 0f;
            while (t < panelFadeInDuration)
            {
                t += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Clamp01(t / panelFadeInDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        private IEnumerator FadeOut()
        {
            float t = panelFadeInDuration;
            while (t > 0f)
            {
                t -= Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Clamp01(t / panelFadeInDuration);
                yield return null;
            }
            canvasGroup.alpha = 0f;

            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        private IEnumerator ShakeRoutine(Transform target)
        {
            Vector3 origin = target.localPosition;
            float t = 0f;

            while (t < shakeDuration)
            {
                t += Time.unscaledDeltaTime;
                float pct = t / shakeDuration;
                float decay = 1f - pct;
                float offsetX = Mathf.Sin(pct * Mathf.PI * 8f) * shakeAmplitude * decay;
                target.localPosition = origin + new Vector3(offsetX, 0f, 0f);
                yield return null;
            }
            target.localPosition = origin;
        }

        private IEnumerator PunchScale(Transform target, float duration)
        {
            Vector3 original = target.localScale;
            float t = 0f;

            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float pct = t / duration;
                float scale = 1f + Mathf.Sin(pct * Mathf.PI) * 0.25f;
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
