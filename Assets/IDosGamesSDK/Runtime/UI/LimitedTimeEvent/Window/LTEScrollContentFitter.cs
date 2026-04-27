using UnityEngine;
using UnityEngine.UI;

namespace IDosGames.UI.LimitedTimeEvent
{
    /// <summary>
    /// Resizes the ScrollRect Content RectTransform so it exactly wraps both
    /// milestone columns.  Call Refresh() after any milestone show/hide pass.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class LTEScrollContentFitter : MonoBehaviour
    {
        [Tooltip("Left milestone column (Group_BubblaFrame05_Left)")]
        [SerializeField] private RectTransform _leftColumn;

        [Tooltip("Right milestone column (Group_BubblaFrame05_Right)")]
        [SerializeField] private RectTransform _rightColumn;

        [Tooltip("Extra space below the last milestone row")]
        [SerializeField] private float _bottomPadding = 40f;

        private RectTransform _rt;

        private void Awake() => _rt = (RectTransform)transform;

        /// <summary>
        /// Rebuilds layout for both columns and adjusts Content height to fit.
        /// Call this after every render pass that may change visible milestone count.
        /// </summary>
        public void Refresh()
        {
            if (_rt == null) _rt = (RectTransform)transform;

            float maxBottom = 0f; // furthest Y (in Content-space, measuring downward)

            MeasureColumn(_leftColumn,  ref maxBottom);
            MeasureColumn(_rightColumn, ref maxBottom);

            if (maxBottom > 0f)
                _rt.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Vertical,
                    maxBottom + _bottomPadding);
        }

        // columnTop + preferredHeight gives the furthest Y used by this column.
        private static void MeasureColumn(RectTransform col, ref float maxBottom)
        {
            if (col == null) return;
            LayoutRebuilder.ForceRebuildLayoutImmediate(col);
            float topOffset = -col.anchoredPosition.y;          // anchoredPos.y is negative
            float preferred = LayoutUtility.GetPreferredHeight(col);
            float bottom    = topOffset + preferred;
            if (bottom > maxBottom) maxBottom = bottom;
        }
    }
}
