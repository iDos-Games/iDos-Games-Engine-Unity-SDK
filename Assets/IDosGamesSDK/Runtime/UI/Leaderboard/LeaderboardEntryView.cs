using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames.UI.Leaderboard
{
    public class LeaderboardEntryView : MonoBehaviour
    {
        [Header("Content")]
        [SerializeField] private TextMeshProUGUI _rankText;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private Image           _avatarImage;

        [Header("Badges")]
        [SerializeField] private GameObject _premiumBadge;
        [SerializeField] private GameObject _currentUserIndicator;

        // ─────────────────────────────────────────────────────────────────

        public void Setup(LeaderboardEntryUIItem item, string scoreDisplayName = "")
        {
            if (_rankText  != null) _rankText.text  = item.Rank > 0 ? $"#{item.Rank}" : "—";
            if (_nameText  != null) _nameText.text  = item.Name;
            if (_scoreText != null) _scoreText.text = FormatScore(item.Score, scoreDisplayName);
            if (_levelText != null) _levelText.text = item.Level > 0 ? $"Lv.{item.Level}" : "";

            if (_avatarImage != null && !string.IsNullOrEmpty(item.AvatarUrl))
                LoadAvatarAsync(item.AvatarUrl);

            if (_premiumBadge        != null) _premiumBadge.SetActive(item.IsPremium);
            if (_currentUserIndicator != null) _currentUserIndicator.SetActive(item.IsCurrentUser);
        }
        
        private static string FormatScore(long score, string displayName)
        {
            string formatted;
            if      (score >= 1_000_000) formatted = $"{score / 1_000_000f:0.#}M";
            else if (score >= 1_000)     formatted = $"{score / 1_000f:0.#}K";
            else                         formatted = score.ToString("N0");

            return string.IsNullOrEmpty(displayName) ? formatted : $"{formatted} {displayName}";
        }

        private async void LoadAvatarAsync(string url)
        {
            try
            {
                var sprite = await ImageLoader.GetSpriteAsync(url);
                if (this != null && _avatarImage != null && sprite != null)
                    _avatarImage.sprite = sprite;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[LeaderboardEntryView] Avatar load failed: {ex.Message}");
            }
        }
    }
}
