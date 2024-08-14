using TMPro;
using UnityEngine;

namespace IDosGames
{
    public class CoinTickerText : MonoBehaviour
    {
        [SerializeField] private TMP_Text _customTokenTicker;

        private void OnEnable()
        {
#if IDOSGAMES_CRYPTO_WALLET
            if(IDosGamesSDKSettings.Instance.CustomContractsEnabled)
            {
                _customTokenTicker.text = IDosGamesSDKSettings.Instance.FirstTokenTicker;
            }
            else
            {
                _customTokenTicker.text = "MEM";
            }
#endif
        }
    }
}
