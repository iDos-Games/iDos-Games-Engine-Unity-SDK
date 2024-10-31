using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IDosGames
{
    public class RewardAnimations : MonoBehaviour
    {
        private static RewardAnimations instance;

        public RewardAnimationManager igcAnimation;
        public RewardAnimationManager eventPointAnimation;
        public RewardAnimationManager igtAnimation;

        public static RewardAnimations Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Object.FindAnyObjectByType<RewardAnimations>();
                }
                return instance;
            }
        }

        public static void ShowIgcAnimation()
        {
            Instance.igcAnimation.AddReward(0);
        }

        public static void ShowEventPointAnimation()
        {
            Instance.eventPointAnimation.AddReward(0);
        }

        public static void ShowIgtAnimation()
        {
            Instance.igtAnimation.AddReward(0);
        }
    }
}
