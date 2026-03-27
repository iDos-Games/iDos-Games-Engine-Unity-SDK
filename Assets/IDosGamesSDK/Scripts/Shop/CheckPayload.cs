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

        private async void OnDisable()
        {
            if (!string.IsNullOrEmpty(ShopSystem._payload))
            {
                ShopSystem._payload = null;
                await UserService.GetClientState();
            }
        }
    }
}
