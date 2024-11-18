using TMPro;
using UnityEngine;

namespace IDosGames
{
    public class TokenTickerText : MonoBehaviour
    {
        [SerializeField] private TMP_Text _customTokenTicker;

        private void OnEnable()
        {
#if IDOSGAMES_CRYPTO_WALLET
            _customTokenTicker.text = BlockchainSettings.HardTokenTicker;
#endif
        }
    }
}
