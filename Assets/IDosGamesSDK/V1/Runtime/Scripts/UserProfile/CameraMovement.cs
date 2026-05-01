using UnityEngine;

namespace IDosGames.UserProfile
{
    public class CameraMovement : MonoBehaviour
    {
        [SerializeField] private Transform avatar;
        private bool isMoving = false;
        public float speed = 2.0f;
        private Vector3 _position;

        private void Update()
        {
            if (isMoving)
            {
                float step = speed * Time.deltaTime;
                transform.localPosition = Vector3.Lerp(transform.localPosition, _position, step);

                if (Vector3.Distance(transform.localPosition, _position) < 0.01f)
                {
                    isMoving = false;
                }
            }

        }



        public void SetTarget(Vector3 position)
        {
            _position = position;
            isMoving = true;
        }
        public void SetPosition(Vector3 position)
        {
            transform.localPosition = position;
        }

    }
}