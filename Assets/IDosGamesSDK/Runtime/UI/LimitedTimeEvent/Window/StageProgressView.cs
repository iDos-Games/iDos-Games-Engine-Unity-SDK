using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames.UI.LimitedTimeEvent
{
    public class StageProgressView : MonoBehaviour
    {
        [SerializeField] private Image               _fillImage;
        [SerializeField] private List<StageIconView> _stageIcons;

        // completedStages = \u043a\u043e\u043b-\u0432\u043e \u043f\u0440\u043e\u0439\u0434\u0435\u043d\u043d\u044b\u0445 \u044d\u0442\u0430\u043f\u043e\u0432 (claimed milestones)
        // totalStages     = \u043e\u0431\u0449\u0435\u0435 \u043a\u043e\u043b-\u0432\u043e \u044d\u0442\u0430\u043f\u043e\u0432 (milestones.Count)
        public void Render(int completedStages, int totalStages)
        {
            if (_fillImage != null)
                _fillImage.fillAmount = totalStages > 0
                    ? Mathf.Clamp01((float)completedStages / totalStages)
                    : 0f;

            if (_stageIcons == null) return;
            for (int i = 0; i < _stageIcons.Count; i++)
            {
                if (_stageIcons[i] == null) continue;
                var state = i < completedStages  ? StageIconState.Completed
                          : i == completedStages ? StageIconState.Current
                          : StageIconState.Upcoming;
                _stageIcons[i].Render(i + 1, state);
            }
        }
    }
}
