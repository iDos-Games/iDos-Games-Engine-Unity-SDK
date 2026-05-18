using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace IDosGames
{
    public class StageBuilding : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image buildingImage;

        [Header("Effects")]
        [SerializeField] private Material grayscaleMaterial;
        [SerializeField] private GameObject damagedEffect;
        [SerializeField] private GameObject upgradeEffect;
        [SerializeField] private float upgradeEffectDuration = 2f;

        [SerializeField] private float upgradeScaleDown = 0.1f;
        [SerializeField] private float upgradeScaleUp = 1.08f;
        [SerializeField] private float upgradeScaleDownDuration = 0.8f;
        [SerializeField] private float upgradeScaleUpDuration = 0.8f;
        [SerializeField] private float upgradeScaleSettleDuration = 0.08f;

        private Coroutine _upgradeRoutine;
        private Vector3 _defaultScale = Vector3.one;
        private bool _hasViewData;

        public int SlotIndex { get; private set; }

        private int _currentLevel;
        private bool _isDamaged;
        private string _loadedImageUrl;
        private Material _defaultMaterial;

        private void Awake()
        {
            if (buildingImage != null)
                _defaultMaterial = buildingImage.material;

            if (damagedEffect != null)
                damagedEffect.SetActive(false);

            if (upgradeEffect != null)
                upgradeEffect.SetActive(false);

            if (buildingImage != null)
                _defaultScale = buildingImage.rectTransform.localScale;
        }

        public void Initialize(int slotIndex)
        {
            SlotIndex = slotIndex;
        }

        public void UpdateView(BuildingDefinition definition, int currentLevel, bool isDamaged)
        {
            int previousLevel = _currentLevel;
            bool shouldPlayBuildOrUpgradeEffect = _hasViewData && currentLevel > previousLevel;

            _currentLevel = currentLevel;
            _isDamaged = isDamaged;

            string imageUrl = ResolveImageUrl(definition, _currentLevel);
            if (!string.IsNullOrEmpty(imageUrl) && imageUrl != _loadedImageUrl)
            {
                _loadedImageUrl = imageUrl;
                LoadImage(imageUrl);
            }

            bool isEmpty = _currentLevel <= 0;

            if (buildingImage != null)
                buildingImage.gameObject.SetActive(!isEmpty);

            if (isEmpty)
            {
                SetDamaged(false);
                _hasViewData = true;
                return;
            }

            SetDamaged(_isDamaged);

            if (shouldPlayBuildOrUpgradeEffect)
            {
                if (_upgradeRoutine != null)
                    StopCoroutine(_upgradeRoutine);

                _upgradeRoutine = StartCoroutine(PlayUpgradeEffect());
            }

            _hasViewData = true;
        }

        private static string ResolveImageUrl(BuildingDefinition definition, int currentLevel)
        {
            var assetPaths = definition?.AssetPaths;
            if (assetPaths == null || assetPaths.Count == 0)
                return null;

            if (assetPaths.TryGetValue($"level_{currentLevel}", out var perLevel) && !string.IsNullOrWhiteSpace(perLevel))
                return perLevel;

            if (assetPaths.TryGetValue("icon", out var icon) && !string.IsNullOrWhiteSpace(icon))
                return icon;

            foreach (var value in assetPaths.Values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }

            return null;
        }

        private void SetDamaged(bool damaged)
        {
            if (buildingImage != null)
            {
                if (damaged && grayscaleMaterial != null)
                {
                    buildingImage.material = grayscaleMaterial;
                    buildingImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                }
                else
                {
                    buildingImage.material = _defaultMaterial;
                    buildingImage.color = Color.white;
                }
            }

            if (damagedEffect != null)
                damagedEffect.SetActive(damaged);
        }

        private IEnumerator PlayUpgradeEffect()
        {
            RectTransform rect = buildingImage != null ? buildingImage.rectTransform : null;

            if (upgradeEffect != null)
                upgradeEffect.SetActive(true);

            if (rect != null)
            {
                rect.localScale = _defaultScale;
                yield return ScaleTo(rect, _defaultScale * upgradeScaleDown, upgradeScaleDownDuration);
                yield return ScaleTo(rect, _defaultScale * upgradeScaleUp, upgradeScaleUpDuration);
                yield return ScaleTo(rect, _defaultScale, upgradeScaleSettleDuration);
            }

            float remainingTime = Mathf.Max(
                0f,
                upgradeEffectDuration - upgradeScaleDownDuration - upgradeScaleUpDuration - upgradeScaleSettleDuration
            );

            if (remainingTime > 0f)
                yield return new WaitForSecondsRealtime(remainingTime);

            if (upgradeEffect != null)
                upgradeEffect.SetActive(false);

            if (rect != null)
                rect.localScale = _defaultScale;

            _upgradeRoutine = null;
        }

        private async void LoadImage(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return;

            var sprite = await ImageLoader.GetSpriteAsync(imageUrl);

            if (sprite != null && this != null && buildingImage != null)
                buildingImage.sprite = sprite;
        }

        private IEnumerator ScaleTo(RectTransform target, Vector3 endScale, float duration)
        {
            if (target == null)
                yield break;

            if (duration <= 0f)
            {
                target.localScale = endScale;
                yield break;
            }

            Vector3 startScale = target.localScale;
            float time = 0f;

            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(time / duration);

                // ���� ��������, ��� ������� Lerp
                float eased = 1f - Mathf.Pow(1f - t, 3f);

                target.localScale = Vector3.Lerp(startScale, endScale, eased);
                yield return null;
            }

            target.localScale = endScale;
        }
    }
}
