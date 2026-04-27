using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames.UI.LimitedTimeEvent
{
    public enum StageIconState { Completed, Current, Upcoming }

    public class StageIconView : MonoBehaviour
    {
        [SerializeField] private Image           _innerImage;
        [SerializeField] private TextMeshProUGUI _numText;

        [Header("State Colors")]
        [SerializeField] private Color _colorCompleted = new Color(0.27f, 0.55f, 1.00f, 1f);
        [SerializeField] private Color _colorCurrent   = new Color(1.00f, 0.80f, 0.15f, 1f);
        [SerializeField] private Color _colorUpcoming  = new Color(0.55f, 0.55f, 0.55f, 1f);

        public void Render(int stageNumber, StageIconState state)
        {
            if (_numText != null)
                _numText.text = stageNumber.ToString();

            if (_innerImage == null) return;
            _innerImage.color = state == StageIconState.Completed ? _colorCompleted
                              : state == StageIconState.Current   ? _colorCurrent
                              : _colorUpcoming;
        }
    }
}
