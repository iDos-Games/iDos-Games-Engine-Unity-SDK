using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IDosGames
{
    public class SecondarySpinPopupClose : MonoBehaviour
    {
        [SerializeField] private GameObject _popupSpin;
        public SpinWheel _spinWheel;

        private void OnEnable()
        {
            _spinWheel.SpinEnded += OnSpinEnded;
        }

        private void OnDisable()
        {
            _spinWheel.SpinEnded -= OnSpinEnded;
        }

        private void OnSpinEnded(int currentSectorIndex)
        {
            _popupSpin.SetActive(false);
        }
    }
}
