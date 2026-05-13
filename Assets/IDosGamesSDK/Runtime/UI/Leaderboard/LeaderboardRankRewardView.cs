using TMPro;
using UnityEngine;
using IDosGames.UI;

namespace IDosGames.UI.Leaderboard
{
    public class LeaderboardRankRewardView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _rankText;
        [SerializeField] private RewardItemView  _rewardTemplate;
        [SerializeField] private RectTransform   _rewardContainer;

        public void Setup(LeaderboardRankRewardUIItem data)
        {
            if (_rankText != null)
                _rankText.text = data.RankRange;

            // clear old rows
            for (int i = _rewardContainer.childCount - 1; i >= 0; i--)
            {
                var child = _rewardContainer.GetChild(i);
                if (child == _rewardTemplate.transform) continue;
                Destroy(child.gameObject);
            }

            if (data.Rewards == null) return;

            foreach (var reward in data.Rewards)
            {
                var row = Instantiate(_rewardTemplate, _rewardContainer);
                row.Setup(reward);
                row.gameObject.SetActive(true);
            }
            
            if (_rewardTemplate != null)
                _rewardTemplate.gameObject.SetActive(false);
        }
    }
}
