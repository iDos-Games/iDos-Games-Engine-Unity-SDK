using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using IDosGames.TitlePublicConfiguration;

namespace IDosGames.UI
{
    public class LootboxPriceButton : MonoBehaviour
    {
        public Button ActionButton;
        public TextMeshProUGUI PriceText;

        private string _lootboxId;
        private int _optionId;

        public void Setup(string lootboxId, LootboxPriceOption option)
        {
            _lootboxId = lootboxId;
            _optionId = option.OptionID;

            StringBuilder sb = new StringBuilder();
            foreach (var res in option.RequiredResources)
            {
                string name = !string.IsNullOrEmpty(res.CurrencyID) ? res.CurrencyID : res.ItemID;
                sb.Append($"{res.Amount} {name} ");
            }
            PriceText.text = sb.ToString();

            ActionButton.onClick.RemoveAllListeners();
            ActionButton.onClick.AddListener(OnClick);
        }

        private async void OnClick()
        {
            ActionButton.interactable = false;

            var result = await LootboxService.Open(_lootboxId, _optionId, 1);

            if (this != null) ActionButton.interactable = true;

            if (!result.Success)
            {
                Debug.LogError($"Error opening lootbox: {result.Error}");
            }
            // If successful -> an event will be fired in RewardWindow
        }
    }
}
