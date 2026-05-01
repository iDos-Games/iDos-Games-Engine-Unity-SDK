using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	public class ChestRewardCard : MonoBehaviour
	{
		[SerializeField] private Image _frame;
		[SerializeField] private Image _icon;
		[SerializeField] private TMP_Text _name;
		[SerializeField] private CardMoveAnimation _moveAnimation;

		public CardMoveAnimation Animation => _moveAnimation;

		public void Set(RarityType rarity, Sprite icon, string name)
		{
			_frame.color = Rarity.GetColor(rarity);
			_icon.sprite = icon;
			_name.text = name;
		}
	}
}