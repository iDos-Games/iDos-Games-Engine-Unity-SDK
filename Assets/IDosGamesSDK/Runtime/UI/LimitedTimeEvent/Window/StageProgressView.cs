using UnityEngine;
using UnityEngine.UI;

namespace IDosGames.UI.LimitedTimeEvent
{
    public class StageProgressView : MonoBehaviour
    {
        [SerializeField] private Image         _fillImage;
        [SerializeField] private StageIconView _stageIconTemplate;
        [SerializeField] private RectTransform _stageIconsContainer;

        public void Render(int completedStages, int totalStages)
        {
            if (_fillImage != null)
                _fillImage.fillAmount = totalStages > 0
                    ? Mathf.Clamp01((float)completedStages / totalStages)
                    : 0f;

            if (_stageIconTemplate == null || _stageIconsContainer == null) return;

            ClearContainer();
            _stageIconTemplate.gameObject.SetActive(false);

            for (int i = 0; i < totalStages; i++)
            {
                var icon  = Instantiate(_stageIconTemplate, _stageIconsContainer);
                var state = i < completedStages  ? StageIconState.Completed
                          : i == completedStages ? StageIconState.Current
                          :                        StageIconState.Upcoming;
                icon.Render(i + 1, state);
                icon.gameObject.SetActive(true);
            }
        }

        private void ClearContainer()
        {
            for (int i = _stageIconsContainer.childCount - 1; i >= 0; i--)
            {
                var child = _stageIconsContainer.GetChild(i).gameObject;
                if (child == _stageIconTemplate.gameObject) continue;
#if UNITY_EDITOR
                if (!Application.isPlaying) { DestroyImmediate(child); continue; }
#endif
                Destroy(child);
            }
        }
    }
}
