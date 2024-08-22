using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IDosGames
{
    public class CheckPayload : MonoBehaviour
    {
        private void OnEnable()
        {
            
        }

        private void OnDisable()
        {
            if (!string.IsNullOrEmpty(ShopSystem._payload))
            {
                ShopSystem._payload = null;
                UserDataService.RequestUserAllData();
            }
        }
    }
}
