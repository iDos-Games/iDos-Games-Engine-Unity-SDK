using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	public class MarketplacePlayerHistoryItem : MonoBehaviour
	{
		[SerializeField] private TMP_Text _name;
		[SerializeField] private Image _rarityBackground;
		[SerializeField] private Image _icon;
		[SerializeField] private TMP_Text _price;
		[SerializeField] private Image _currencyIcon;
		[SerializeField] private TMP_Text _soldText;
		[SerializeField] private TMP_Text _boughtText;

		public virtual async void Fill(MarketplaceActiveOffer offer, Sprite currencyIcon)
		{
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

			_soldText.gameObject.SetActive(offer.SellerID == AuthService.UserID);
			_boughtText.gameObject.SetActive(offer.SellerID != AuthService.UserID);
		}
	}
}
