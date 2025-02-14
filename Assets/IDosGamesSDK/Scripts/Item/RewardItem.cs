using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	public class RewardItem : Item
	{
		[SerializeField] private Image _icon;
		[SerializeField] private TMP_Text _amount;

		public int Amount
		{
			get => GetAmount();
			protected set => SetAmount(value);
		}

        public void Set(string imagePath, int amount)
        {
            SetIcon(imagePath);
            SetAmount(amount);
        }

        private async void SetIcon(string imagePath)
        {
            string iconPath = (imagePath == JsonProperty.TOKEN_IMAGE_PATH) ? IGSUserData.Currency.CurrencyData.Find(c => c.CurrencyCode == "IG")?.ImageUrl ?? JsonProperty.TOKEN_IMAGE_PATH : imagePath;
            var sprite = await ImageLoader.GetSpriteAsync(iconPath);
            if (sprite != null)
            {
                _icon.sprite = sprite;
            }
        }

        private void SetAmount(int amount)
		{
			amount = amount < 0 ? 0 : amount;
			_amount.text = "x" + amount.ToString();
		}

		private int GetAmount()
		{
			int.TryParse(_amount.text, out int amount);
			return amount;
		}
	}
}