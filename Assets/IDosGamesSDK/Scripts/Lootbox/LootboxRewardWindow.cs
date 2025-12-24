using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using IDosGames.ServerModels;
using IDosGames.TitlePublicConfiguration;

namespace IDosGames
{
    public class LootboxRewardWindow : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject _windowRoot;
        [SerializeField] private Transform _itemsContainer;
        [SerializeField] private GameObject _rewardItemPrefab; // Префаб с иконкой и текстом
        [SerializeField] private Button _closeButton;

        private void Awake()
        {
            if (_closeButton) _closeButton.onClick.AddListener(Close);
            if (_windowRoot) _windowRoot.SetActive(false);
        }

        public void Show(LootboxOpenResponse result)
        {
            // Очищаем старые иконки
            foreach (Transform child in _itemsContainer)
                Destroy(child.gameObject);

            // Агрегируем награды (суммируем одинаковые)
            Dictionary<string, int> aggregatedRewards = new Dictionary<string, int>();

            if (result.Results != null)
            {
                foreach (var boxContent in result.Results)
                {
                    foreach (var reward in boxContent)
                    {
                        string id = (reward.Type == ItemType.VirtualCurrency) ? reward.CurrencyID : reward.ItemID;
                        int amount = reward.Amount ?? 1;

                        if (aggregatedRewards.ContainsKey(id))
                            aggregatedRewards[id] += amount;
                        else
                            aggregatedRewards[id] = amount;
                    }
                }
            }

            // Создаем визуальные элементы
            foreach (var kvp in aggregatedRewards)
            {
                CreateRewardIcon(kvp.Key, kvp.Value);
            }

            _windowRoot.SetActive(true);
        }

        private void CreateRewardIcon(string id, int amount)
        {
            var go = Instantiate(_rewardItemPrefab, _itemsContainer);

            // Находим текст в префабе (предполагаем, что он там есть)
            var text = go.GetComponentInChildren<TextMeshProUGUI>();
            if (text)
                text.text = $"{id}\nx{amount}";

            // Здесь можно добавить логику загрузки иконки по ID
        }

        private void Close()
        {
            _windowRoot.SetActive(false);
        }
    }
}
