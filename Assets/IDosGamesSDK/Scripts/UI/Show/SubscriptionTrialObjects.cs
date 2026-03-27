using System.Linq;
using UnityEngine;

namespace IDosGames
{
    public class SubscriptionTrialObjects : MonoBehaviour
    {
        [SerializeField] private GameObject[] _onFreeTrial;
        [SerializeField] private GameObject[] _onNotFreeTrial;

        private void Start()
        {
            bool freeEnabled = GetFreeTrialState();
            _onFreeTrial.ToList().ForEach(x => x.SetActive(freeEnabled));
            _onNotFreeTrial.ToList().ForEach(x => x.SetActive(!freeEnabled));
        }

        private bool GetFreeTrialState()
        {
            var vipFreeTrial = IDosGamesData.Config.TitlePublicConfiguration?.SystemState?.VipFreeTrial;
            if (vipFreeTrial == null) return false;

#if UNITY_IOS
            return vipFreeTrial.Ios;
#elif UNITY_ANDROID
            return vipFreeTrial.Android;
#else
            return false;
#endif
        }
    }
}
