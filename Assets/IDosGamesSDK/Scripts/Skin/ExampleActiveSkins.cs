using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	public class ExampleActiveSkins : MonoBehaviour
	{
		[SerializeField] private Image _circle;
		[SerializeField] private Image _triangle;
		[SerializeField] private Image _square;

		[Header("DefaultSkins")]
		[Space(5)]
		[SerializeField] private Sprite _circleDefaultSkin;
		[SerializeField] private Sprite _triangleDefaultSkin;
		[SerializeField] private Sprite _squareDefaultSkin;

		private const string CIRCLE_OBJECT_TYPE = "circle";
		private const string TRIANGLE_OBJECT_TYPE = "triangle";
		private const string SQUARE_OBJECT_TYPE = "square";

		private void OnEnable()
		{
			UserDataService.SkinCatalogItemsUpdated += UpdateSkinsView;
			UserDataService.EquippedSkinsUpdated += UpdateSkinsView;
		}

		private void OnDisable()
		{
			UserDataService.SkinCatalogItemsUpdated -= UpdateSkinsView;
			UserDataService.EquippedSkinsUpdated -= UpdateSkinsView;
		}

		private void UpdateSkinsView()
		{
			_circle.sprite = _circleDefaultSkin;
			_triangle.sprite = _triangleDefaultSkin;
			_square.sprite = _squareDefaultSkin;

			foreach (var item in UserDataService.EquippedSkins)
			{
				var skinItem = UserDataService.GetSkinItem(item);

				if (skinItem == null)
				{
					continue;
				}

				Sprite icon = Resources.Load<Sprite>(skinItem.ImagePath);

				switch (skinItem.ObjectType)
				{
					case CIRCLE_OBJECT_TYPE:
						{
							_circle.sprite = icon;
							break;
						}
					case TRIANGLE_OBJECT_TYPE:
						{
							_triangle.sprite = icon;
							break;
						}
					case SQUARE_OBJECT_TYPE:
						{
							_square.sprite = icon;
							break;
						}
				}
			}
		}
	}
}
