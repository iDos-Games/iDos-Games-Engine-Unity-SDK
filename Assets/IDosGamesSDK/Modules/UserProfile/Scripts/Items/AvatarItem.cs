using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace IDosGames.UserProfile
{
    public class AvatarItem : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _icon;
        [SerializeField] private Image _rarityBackground;
        [SerializeField] private TMP_Text _name;
        [SerializeField] private Image _checkMark;

        [SerializeField] private TMP_Text _amount;

        [SerializeField] private GameObject _amountObject;

        public AvatarSkinCatalogItem AvatarData { get; private set; }
        public async void Fill(Action action, AvatarSkinCatalogItem item)
        {
            ResetButton(action);
            AvatarData = item;
            _icon.sprite = await ImageLoader.GetSpriteAsync(item.ImagePath);
            _rarityBackground.color = Rarity.GetColor(item.Rarity);
            var amount = UserInventory.GetItemAmount(item.ItemID);
            UpdateAmount(amount);
            _name.text = item.DisplayName;
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

        public void UpdateAmount(int amount)
        {
            _amount.text = amount.ToString();

            if (_amountObject != null)
            {
                _amountObject.SetActive(amount > 0);
            }
        }

        public void SetActivateCheckMark(bool active)
        {
            _checkMark.gameObject.SetActive(active);
        }
    }
}