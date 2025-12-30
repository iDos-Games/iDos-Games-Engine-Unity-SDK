using UnityEngine;

namespace IDosGames.UI
{
    public class LootboxManager : MonoBehaviour
    {
        public Transform ContentRoot;
        public LootboxItemView LootboxPrefab;
        public GameObject LoadingSpinner;

        private async void Start()
        {
            if (LoadingSpinner) LoadingSpinner.SetActive(true);

            var result = await LootboxService.GetDefinitions();

            if (LoadingSpinner) LoadingSpinner.SetActive(false);

            if (result.Success && result.Data.LootboxDefinitions != null)
            {
                foreach (Transform child in ContentRoot) Destroy(child.gameObject);

                foreach (var def in result.Data.LootboxDefinitions)
                {
                    var view = Instantiate(LootboxPrefab, ContentRoot);
                    view.Setup(def);
                }
            }
            else
            {
                Debug.LogError("Failed definitions: " + result.Error);
            }
        }
    }
}
