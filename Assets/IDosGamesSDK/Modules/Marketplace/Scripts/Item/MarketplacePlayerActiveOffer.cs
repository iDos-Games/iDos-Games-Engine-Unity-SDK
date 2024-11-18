using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace IDosGames
{
	[RequireComponent(typeof(Button))]
	public class MarketplacePlayerActiveOffer : MonoBehaviour
	{
		[SerializeField] private Button _button;
		[SerializeField] private TMP_Text _name;
		[SerializeField] private Image _rarityBackground;
		[SerializeField] private Image _icon;
		[SerializeField] private TMP_Text _price;
		[SerializeField] private Image _currencyIcon;

		public virtual async void Fill(Action action, MarketplaceActiveOffer offer, Sprite currencyIcon)
		{
			ResetButton(action);

			SkinCatalogItem item = UserDataService.GetCachedSkinItem(offer?.ItemID);
            if (item == null)
            {
                item = UserDataService.GetAvatarSkinItem(offer?.ItemID);
            }
            _icon.sprite = await ImageLoader.GetSpriteAsync(item.ImagePath);
            _rarityBackground.color = Rarity.GetColor(item.Rarity);
			_name.text = item.DisplayName;

			_price.text = ((int)offer.Price).ToString();
			_currencyIcon.sprite = currencyIcon;
		}

		private void ResetButton(Action action)
		{
			if (action == null)
			{
				return;
			}

			_button.onClick.RemoveAllListeners();
			_button.onClick.AddListener(new UnityAction(action));
		}
	}
}
