using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames.UI.LimitedTimeEvent
{
    public enum StageIconState { Completed, Current, Upcoming }

    public class StageIconView : MonoBehaviour
    {
        [SerializeField] private Image           _backgroundImage;
        [SerializeField] private Image           _innerImage;
        [SerializeField] private TextMeshProUGUI _numText;

        [Header("Inner Colors")]
        [SerializeField] private Color _colorCompleted = new Color(0.27f, 0.55f, 1.00f, 1f);
        [SerializeField] private Color _colorCurrent   = new Color(1.00f, 0.80f, 0.15f, 1f);
        [SerializeField] private Color _colorUpcoming  = new Color(0.55f, 0.55f, 0.55f, 1f);

        [Header("Background Colors")]
        [SerializeField] private Color _bgColorCompleted = new Color(0.10f, 0.20f, 0.40f, 1f);
        [SerializeField] private Color _bgColorCurrent   = new Color(0.40f, 0.30f, 0.05f, 1f);
        [SerializeField] private Color _bgColorUpcoming  = new Color(0.20f, 0.20f, 0.20f, 1f);

        public void Render(int stageNumber, StageIconState state)
        {
            if (_numText != null)
                _numText.text = stageNumber.ToString();

            Color inner = state == StageIconState.Completed ? _colorCompleted
                        : state == StageIconState.Current   ? _colorCurrent
                        : _colorUpcoming;

            Color bg    = state == StageIconState.Completed ? _bgColorCompleted
                        : state == StageIconState.Current   ? _bgColorCurrent
                        : _bgColorUpcoming;

            if (_innerImage      != null) _innerImage.color      = inner;
            if (_backgroundImage != null) _backgroundImage.color = bg;
        }
    }
}
