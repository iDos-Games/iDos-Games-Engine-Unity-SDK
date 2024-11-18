using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	public class ExampleActiveSkins : MonoBehaviour
	{
		[SerializeField] private Image _ak47;
		[SerializeField] private Image _awm;
		[SerializeField] private Image _deagle;
        [SerializeField] private Image _m4;
        [SerializeField] private Image _usp9;

        [Header("DefaultSkins")]
		[Space(5)]
		[SerializeField] private Sprite _ak47DefaultSkin;
		[SerializeField] private Sprite _awmDefaultSkin;
		[SerializeField] private Sprite _deagleDefaultSkin;
        [SerializeField] private Sprite _m4DefaultSkin;
        [SerializeField] private Sprite _usp9DefaultSkin;

        private const string AK47_OBJECT_TYPE = "ak47";
		private const string AWM_OBJECT_TYPE = "awm";
		private const string DEAGLE_OBJECT_TYPE = "deagle";
        private const string M4_OBJECT_TYPE = "m4";
        private const string USP9_OBJECT_TYPE = "usp9";

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

		private async void UpdateSkinsView()
		{
			_ak47.sprite = _ak47DefaultSkin;
			_awm.sprite = _awmDefaultSkin;
			_deagle.sprite = _deagleDefaultSkin;
			_m4.sprite = _m4DefaultSkin;
            _usp9.sprite = _usp9DefaultSkin;

            foreach (var item in UserDataService.EquippedSkins)
			{
				var skinItem = UserDataService.GetCachedSkinItem(item);

				if (skinItem == null)
				{
					continue;
				}

				Sprite icon = await ImageLoader.GetSpriteAsync(skinItem.ImagePath);

				switch (skinItem.ObjectType)
				{
					case AK47_OBJECT_TYPE:
						{
							_ak47.sprite = icon;
							break;
						}
					case AWM_OBJECT_TYPE:
						{
							_awm.sprite = icon;
							break;
						}
					case DEAGLE_OBJECT_TYPE:
						{
							_deagle.sprite = icon;
							break;
						}
                    case M4_OBJECT_TYPE:
                        {
                            _m4.sprite = icon;
                            break;
                        }
                    case USP9_OBJECT_TYPE:
                        {
                            _usp9.sprite = icon;
                            break;
                        }
                }
			}
		}
	}
}
