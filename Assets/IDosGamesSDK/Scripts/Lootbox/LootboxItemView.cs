using UnityEngine;
using TMPro;
using IDosGames.TitlePublicConfiguration;

namespace IDosGames.UI
{
    public class LootboxItemView : MonoBehaviour
    {
        public TextMeshProUGUI TitleText;
        public Transform PricesContainer;
        public LootboxPriceButton PriceButtonPrefab;

        public void Setup(LootboxDefinition def)
        {
            TitleText.text = def.LootboxID;

            foreach (Transform child in PricesContainer) Destroy(child.gameObject);

            if (def.PriceOptions != null)
            {
                foreach (var option in def.PriceOptions)
                {
                    var btn = Instantiate(PriceButtonPrefab, PricesContainer);
                    btn.Setup(def.LootboxID, option);
                }
            }
        }
    }
}
