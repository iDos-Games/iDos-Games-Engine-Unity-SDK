#if IDOSGAMES_CRYPTO_WALLET
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
	[RequireComponent(typeof(Button))]
	public class ConnectWalletButton : MonoBehaviour
	{
		//[SerializeField] private WalletConnectV2 _walletConnectV2;
		[SerializeField] private GameObject _loading;

		private Button _button;

		private void Awake()
		{
			_button = GetComponent<Button>();
			ResetListener();

			//WalletConnectV2.ConnectingWalletStarted += () => SetInteractable(false);
		}

		private void OnEnable()
		{
			SetInteractable(true);
		}

		private void ResetListener()
		{
			_button.onClick.RemoveAllListeners();
			//_button.onClick.AddListener(() => _walletConnectV2.Connect());
		}

		private void SetInteractable(bool interactable)
		{
			_button.interactable = interactable;
			_loading.gameObject.SetActive(!interactable);
		}
	}
}
#endif