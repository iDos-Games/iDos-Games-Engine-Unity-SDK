// File: Assets/IDosGamesSDK/Runtime/UI/Social/Views/LoadingView.cs
using UnityEngine;

namespace IDosGames.UI.Social
{
    /// <summary>
    /// Simple loading indicator. Attach to a GameObject with a spinner/animation.
    /// </summary>
    public class LoadingView : MonoBehaviour
    {
        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
