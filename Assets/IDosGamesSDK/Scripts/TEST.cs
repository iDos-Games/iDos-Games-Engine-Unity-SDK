using IDosGames.ClientModels;
using UnityEngine;

namespace IDosGames
{
    public class TEST : MonoBehaviour
    {
        private void Start()
        {
            Test();
        }

        public void Test()
        {
            
        }

        public void Btn1()
        {
            AuthService.Instance.LoginWithDeviceID();
        }

        public void Btn2()
        {
            AuthService.Instance.AddUsernamePassword("aido_92@mail.ru", "123456aA@");
        }

    }
}