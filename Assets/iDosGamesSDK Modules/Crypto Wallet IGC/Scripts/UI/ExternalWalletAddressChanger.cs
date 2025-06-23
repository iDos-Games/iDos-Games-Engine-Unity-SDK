using TMPro;
using UnityEngine;

namespace IDosGames
{
    public class ExternalWalletAddressChanger : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _addressInputField;

        public void ChangeExternalWalletAddress()
        {
            WalletManager.ToAddress = _addressInputField.text;
        }
    }
}
