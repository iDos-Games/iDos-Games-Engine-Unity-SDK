using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	[RequireComponent(typeof(Button))]
	public class ConnectWalletButton : MonoBehaviour
	{
		[SerializeField] private GameObject _loading;

		private Button _button;

#if IDOSGAMES_CRYPTO_WALLET
		private void Awake()
		{
			_button = GetComponent<Button>();
			ResetListener();

		}

		private void OnEnable()
		{
			SetInteractable(true);
		}

		private void ResetListener()
		{
			_button.onClick.RemoveAllListeners();
		}

		private void SetInteractable(bool interactable)
		{
			_button.interactable = interactable;
			_loading.gameObject.SetActive(!interactable);
		}
#endif

    }
}
