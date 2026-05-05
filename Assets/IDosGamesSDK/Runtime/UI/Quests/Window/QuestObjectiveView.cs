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

        public void Setup(ObjectiveUIItem objective)
        {
            if (_labelText != null)
                _labelText.text = objective.Label;

            long display = objective.Completed ? objective.Target : objective.Current;
            if (_progressText != null)
                _progressText.text = $"{display}/{objective.Target}";

            if (_progressSlider != null && objective.Target > 0)
                _progressSlider.value = objective.Completed
                    ? 1f
                    : Mathf.Clamp01((float)objective.Current / objective.Target);
        }
    }
}
