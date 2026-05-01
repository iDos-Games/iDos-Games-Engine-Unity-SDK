using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IDosGames
{
    public class ArrowMultiplicator : MonoBehaviour
    {
        public RewardMultiplicator rewardMultiplicator;
        public float speed = 5.0f;
        public float minX = -455f;
        public float maxX = 455f;

        private RectTransform rectTransform;
        private bool isMovingRight = true;

        void Start()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        void Update()
        {
            Vector3 currentPosition = rectTransform.anchoredPosition;

            if (rewardMultiplicator._shouldMove)
            {
                float newX = currentPosition.x + (isMovingRight ? speed : -speed) * Time.unscaledDeltaTime;

                newX = Mathf.Clamp(newX, minX, maxX);

                rectTransform.anchoredPosition = new Vector3(newX, currentPosition.y, currentPosition.z);

                if (newX == minX || newX == maxX)
                {
                    isMovingRight = !isMovingRight;
                }
            }

            if(currentPosition.x >= -455 && currentPosition.x <= -340 )
            {
                rewardMultiplicator._multiplicator = 1.5f;
                rewardMultiplicator._multiplicatorIndex = 0;
            }
            else if (currentPosition.x >= -339.999 && currentPosition.x <= -203 )
            {
                rewardMultiplicator._multiplicator = 2f;
                rewardMultiplicator._multiplicatorIndex = 1;
            }
            else if (currentPosition.x >= -202.999 && currentPosition.x <= -67 )
            {
                rewardMultiplicator._multiplicator = 2.5f;
                rewardMultiplicator._multiplicatorIndex = 2;
            }
            else if (currentPosition.x >= -66.999 && currentPosition.x <= 67 )
            {
                rewardMultiplicator._multiplicator = 3f;
                rewardMultiplicator._multiplicatorIndex = 3;
            }
            else if (currentPosition.x >= 67.01 && currentPosition.x <= 203)
            {
                rewardMultiplicator._multiplicator = 2.5f;
                rewardMultiplicator._multiplicatorIndex = 4;
            }
            else if (currentPosition.x >= 203.01 && currentPosition.x <= 340)
            {
                rewardMultiplicator._multiplicator = 2f;
                rewardMultiplicator._multiplicatorIndex = 5;
            }
            else if(currentPosition.x >= 340.01 && currentPosition.x <= 455)
            {
                rewardMultiplicator._multiplicator = 1.5f;
                rewardMultiplicator._multiplicatorIndex = 6;
            }
        }
    }
}