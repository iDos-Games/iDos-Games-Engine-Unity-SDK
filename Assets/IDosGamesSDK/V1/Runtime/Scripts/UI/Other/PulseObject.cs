using UnityEngine;

namespace IDosGames
{
    public class PulseObject : MonoBehaviour
    {
        public float minScale = 0.9f;
        public float maxScale = 1.1f;
        public float pulseSpeed = 2f;

        private Vector3 initialScale;

        void Start()
        {
            initialScale = transform.localScale;
        }

        void Update()
        {
            float scale = Mathf.Lerp(minScale, maxScale, (Mathf.PingPong(Time.time * pulseSpeed, 1)));
            transform.localScale = initialScale * scale;
        }
    }
}
