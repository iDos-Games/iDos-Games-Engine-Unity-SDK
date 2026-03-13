using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using IDosGames.TitlePublicConfiguration;

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
        }

        public void Initialize(int slotIndex)
        {
            SlotIndex = slotIndex;
        }

        public void UpdateView(BuildingDefinition definition, int currentLevel, bool isDamaged)
        {
            int previousLevel = _currentLevel;
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
                return;
            }

            SetDamaged(_isDamaged);

            if (previousLevel > 0 && currentLevel > previousLevel)
                StartCoroutine(PlayUpgradeEffect());
        }

        private static string ResolveImageUrl(BuildingDefinition definition, int currentLevel)
        {
            if (definition?.ImageUrls == null || definition.ImageUrls.Count == 0)
                return null;

            if (definition.ImageUrls.Count == 1)
                return definition.ImageUrls[0];

            int levelIndex = Mathf.Max(0, currentLevel - 1);
            levelIndex = Mathf.Clamp(levelIndex, 0, definition.ImageUrls.Count - 1);

            return definition.ImageUrls[levelIndex];
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
            if (upgradeEffect == null)
                yield break;

            upgradeEffect.SetActive(true);
            yield return new WaitForSecondsRealtime(upgradeEffectDuration);

            if (upgradeEffect != null)
                upgradeEffect.SetActive(false);
        }

        private async void LoadImage(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return;

            var sprite = await ImageLoader.GetSpriteAsync(imageUrl);

            if (sprite != null && this != null && buildingImage != null)
                buildingImage.sprite = sprite;
        }
    }
}
