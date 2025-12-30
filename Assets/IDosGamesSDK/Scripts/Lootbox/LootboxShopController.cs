using UnityEngine;
using IDosGames.ServerModels;
using IDosGames.TitlePublicConfiguration;

namespace IDosGames
{
    public class LootboxShopController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private Transform _lootboxListContainer;
        [SerializeField] private LootboxItemView _lootboxPrefab;
        [SerializeField] private LootboxRewardWindow _rewardWindow;
        [SerializeField] private GameObject _loadingIndicator;

        private void OnEnable()
        {
            // Так как мы в одном namespace, LootboxService доступен напрямую
            LootboxService.OnDefinitionsReceived += OnDefinitionsLoaded;
            LootboxService.OnLootboxOpened += OnLootboxOpened;
        }

        private void OnDisable()
        {
            LootboxService.OnDefinitionsReceived -= OnDefinitionsLoaded;
            LootboxService.OnLootboxOpened -= OnLootboxOpened;
        }

        private async void Start()
        {
            SetLoading(true);
            var result = await LootboxService.GetDefinitions();
            SetLoading(false);

            if (!result.Success)
            {
                // Debug.LogError($"[IDosGames] Shop load failed: {result.ErrorMessage}");
            }
        }

        private void OnDefinitionsLoaded(LootboxDefinitionsResponse data)
        {
            // Очистка
            foreach (Transform child in _lootboxListContainer) Destroy(child.gameObject);

            // Создание списка
            if (data.LootboxDefinitions != null)
            {
                foreach (var def in data.LootboxDefinitions)
                {
                    var newItem = Instantiate(_lootboxPrefab, _lootboxListContainer);
                    newItem.Setup(def, OnPurchaseRequested);
                }
            }
        }

        private void OnLootboxOpened(LootboxOpenResponse result)
        {
            SetLoading(false);
            // Показываем окно наград
            if (_rewardWindow) _rewardWindow.Show(result);
        }

        private async void OnPurchaseRequested(LootboxDefinition def, int optionId)
        {
            SetLoading(true);
            Debug.Log($"[IDosGames] Purchasing {def.LootboxID}...");

            var result = await LootboxService.Open(def.LootboxID, optionId, 1);

            if (!result.Success)
            {
                SetLoading(false);
                // Debug.LogError($"[IDosGames] Purchase failed: {result.ErrorMessage}");
                // Здесь можно добавить вызов Popup с ошибкой
            }
        }

        private void SetLoading(bool isLoading)
        {
            if (_loadingIndicator) _loadingIndicator.SetActive(isLoading);
        }
    }
}
