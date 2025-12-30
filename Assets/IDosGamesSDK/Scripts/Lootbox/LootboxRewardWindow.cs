using UnityEngine;
using UnityEngine.UI;
using IDosGames.ServerModels;

namespace IDosGames.UI
{
    public class LootboxRewardWindow : MonoBehaviour
    {
        [Header("References")]
        public GameObject WindowRoot;
        public Transform GridContainer;
        public LootboxRewardView RewardItemPrefab;
        public Button CloseButton;

        private void Awake()
        {
            LootboxService.OnLootboxOpened += ShowRewards;

            if (CloseButton) CloseButton.onClick.AddListener(CloseWindow);
            if (WindowRoot) WindowRoot.SetActive(false);
        }

        private void OnDestroy()
        {
            LootboxService.OnLootboxOpened -= ShowRewards;
        }

        private void ShowRewards(LootboxOpenResponse response)
        {
            if (response == null || response.Results == null) return;

            foreach (Transform child in GridContainer) Destroy(child.gameObject);

            foreach (var sessionList in response.Results)
            {
                foreach (var reward in sessionList)
                {
                    var item = Instantiate(RewardItemPrefab, GridContainer);
                    item.Setup(reward);
                }
            }

            if (WindowRoot) WindowRoot.SetActive(true);
        }

        private void CloseWindow()
        {
            if (WindowRoot) WindowRoot.SetActive(false);
        }
    }
}
