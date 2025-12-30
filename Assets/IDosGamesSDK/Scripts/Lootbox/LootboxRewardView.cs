using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IDosGames.TitlePublicConfiguration;

namespace IDosGames.UI
{
    public class LootboxRewardView : MonoBehaviour
    {
        [Header("UI References")]
        public Image IconImage;
        public TextMeshProUGUI AmountText;
        public TextMeshProUGUI NameText;

        public void Setup(ItemOrCurrency item)
        {
            int amount = item.Amount ?? 1;
            if (AmountText) AmountText.text = $"x{amount}";

            string displayName = !string.IsNullOrEmpty(item.Name) ? item.Name :
                                 (!string.IsNullOrEmpty(item.CurrencyID) ? item.CurrencyID : item.ItemID);

            if (NameText) NameText.text = displayName;

            // TODO: Реализуй тут загрузку картинки
            // if (!string.IsNullOrEmpty(item.ImagePath)) 
            //    WebSDK.DownloadImage(item.ImagePath, IconImage);
        }
    }
}
