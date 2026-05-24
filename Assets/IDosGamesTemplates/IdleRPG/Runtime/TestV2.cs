using UnityEngine;

namespace IDosGames
{
    public class TestV2 : MonoBehaviour
    {
        public void Lootbox()
        {
            _ = LootboxService.Open("Test", 1, 1);
        }

        public void Lootbox2()
        {
            _ = LootboxService.Open("Test2", 1, 1);
        }
    }
}
