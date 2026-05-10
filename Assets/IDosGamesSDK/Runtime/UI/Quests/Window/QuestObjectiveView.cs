using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames.UI.Quest
{
    /// <summary>
    /// Displays a single quest objective: label + progress slider + progress text.
    /// Spawned at runtime by QuestItemView for each objective.
    /// </summary>
    public class QuestObjectiveView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _labelText;
        [SerializeField] private Slider          _progressSlider;
        [SerializeField] private TextMeshProUGUI _progressText;

        private static readonly Color ColorDone    = new Color(0.102f, 0.910f, 0.447f); // #1AE872 green
        private static readonly Color ColorLabel   = new Color(1f, 1f, 1f, 0.65f); // readable white
        private static readonly Color ColorProgress= new Color(0.55f,  0.8f,   1f);     // #8CCCFF light blue
        private static readonly Color ColorFill    = new Color(0.2f,   0.8f,   1f);     // #33CCFF vivid cyan

        public void Setup(ObjectiveUIItem objective)
        {
            bool done = objective.Completed;

            if (_labelText != null)
            {
                _labelText.text  = objective.Label;
                _labelText.color = done ? ColorDone : ColorLabel;
            }

            long display = done ? objective.Target : objective.Current;
            if (_progressText != null)
            {
                _progressText.text  = $"{display}/{objective.Target}";
                _progressText.color = done ? ColorDone : ColorProgress;
            }

            if (_progressSlider != null && objective.Target > 0)
            {
                _progressSlider.value = done ? 1f : Mathf.Clamp01((float)objective.Current / objective.Target);

                var fillImg = _progressSlider.fillRect?.GetComponent<Image>();
                if (fillImg != null)
                {
                    Color fc = done ? ColorDone : ColorFill;
                    fillImg.color = fc;

                    var glow = _progressSlider.fillRect.GetComponent<Outline>();
                    if (glow == null)
                        glow = _progressSlider.fillRect.gameObject.AddComponent<Outline>();
                    glow.effectColor    = new Color(fc.r, fc.g, fc.b, done ? 0.55f : 0.70f);
                    glow.effectDistance = new Vector2(0f, done ? 4f : 6f);
                    glow.useGraphicAlpha = false;
                }
            }
        }
    }
}
