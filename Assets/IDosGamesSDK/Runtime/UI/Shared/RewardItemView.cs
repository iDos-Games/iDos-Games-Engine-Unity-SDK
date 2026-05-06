using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames.UI
{
    public class RewardItemView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _amountText;

        public Image Icon => _icon;
        public TextMeshProUGUI AmountText => _amountText;

        public void Setup(string text, Sprite iconSprite = null)
        {
            if (_amountText != null) _amountText.text = text;
            if (_icon != null && iconSprite != null) _icon.sprite = iconSprite;
            gameObject.SetActive(true);
        }
    }
}
