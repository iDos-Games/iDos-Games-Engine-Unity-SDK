using UnityEngine;
namespace IDosGames.UserProfile
{
    public class AvatarItemMenu : MonoBehaviour
    {
        [SerializeField] private AvatarItemMenuPopUp _popUp;

        public void ShowPopUp(Transform transform, AvatarSkinCatalogItem item)
        {
            _popUp.SetButtons(item);
            _popUp.SetPosition(transform);
            gameObject.SetActive(true);
        }
    }
}