using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IDosGames
{
    public class BackgroundScroll : MonoBehaviour
    {
        public RectTransform _mosaique;

        [Range(0.001f, 0.03f)]
        public float _speed = 0.001f;
        public float _lerpDuration = 1;
        float _timeElapsed;
        Vector2 _destination;
        Vector2 _startPos;
        
        void Start()
        {
            _startPos = _mosaique.localPosition;
            _destination = new Vector2((_startPos.x - 100f), (_startPos.y - 100f));
        }

        void Update()
        {
            if (_timeElapsed < _lerpDuration)
            {
                _mosaique.localPosition = Vector2.Lerp(_startPos, _destination, _timeElapsed / _lerpDuration);
                _timeElapsed += 0.001f;
            }
            else
            {
                _timeElapsed = 0f;
            }
        }
    }
}
