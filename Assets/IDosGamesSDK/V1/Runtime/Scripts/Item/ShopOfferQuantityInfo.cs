using TMPro;
using UnityEngine;

namespace IDosGames
{
	public class ShopOfferQuantityInfo : MonoBehaviour
	{
		[SerializeField] private TMP_Text _text;

		public void Set(string quantityLeft)
		{
			_text.text = quantityLeft;
		}
	}
}