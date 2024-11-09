using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace IDosGames
{
    [RequireComponent(typeof(Button))]
    public class GroupedOfferItem : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _name;
        [SerializeField] private Image _rarityBackground;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _amount;

        public virtual async void Fill(Action action, string itemID, int amount)
        {
            ResetButton(action);

            SkinCatalogItem item = UserDataService.GetCachedSkinItem(itemID);
            if (item == null)
            {
                item = UserDataService.GetAvatarSkinItem(itemID);
            }

            if (item != null)
            {
                _icon.sprite = await ImageLoader.GetSpriteAsync(item.ImagePath);
                _rarityBackground.color = Rarity.GetColor(item.Rarity);
                _name.text = item.DisplayName;
            }
            _amount.text = amount.ToString();
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
