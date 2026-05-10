using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames.UI.LimitedTimeEvent
{
    public class EventTabButtonView : MonoBehaviour
    {
        [SerializeField] private Button          _button;
        [SerializeField] private TextMeshProUGUI _label;

        public void Setup(string label, Action onClick)
        {
            if (_label  != null) _label.text = label;
            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
                _button.onClick.AddListener(() => onClick?.Invoke());
            }
            SetSelected(false);
        }

        public void SetSelected(bool selected)
        {
            if (_button != null) _button.interactable = !selected;
        }
    }
}
