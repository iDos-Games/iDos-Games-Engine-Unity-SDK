using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames.UI.Leaderboard
{
    public class LeaderboardCycleRewardPopup : MonoBehaviour
    {
        [SerializeField] private Button          _closeButton;
        [SerializeField] private TextMeshProUGUI _rankText;
        [SerializeField] private RectTransform   _rewardContainer;
        [SerializeField] private RewardItemView  _rewardItemTemplate;

        private static readonly Queue<ClaimCycleRewardResponse> _queue = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterHook()
        {
            LeaderboardService.OnStartupRewardsClaimed += Show;
        }

        private void OnEnable()
        {
            if (_closeButton != null)
                _closeButton.onClick.AddListener(Hide);
        }

        private void OnDisable()
        {
            if (_closeButton != null)
                _closeButton.onClick.RemoveListener(Hide);
        }

        // ── Static entry point called by LeaderboardService ───────────────

        public static void Show(List<ClaimCycleRewardResponse> rewards)
        {
            var instance = FindAnyObjectByType<LeaderboardCycleRewardPopup>(FindObjectsInactive.Include);
            if (instance == null) return;

            _queue.Clear();
            foreach (var r in rewards)
                _queue.Enqueue(r);

            instance.Display(_queue.Dequeue());
        }

        public void Hide()
        {
            gameObject.SetActive(false);

            if (_queue.Count > 0)
                Display(_queue.Dequeue());
        }

        // ── Display logic ─────────────────────────────────────────────────

        private void Display(ClaimCycleRewardResponse response)
        {
            var entries = response.Resources?.Grant?.Standard?.Entries;

            if (_rankText != null)
                _rankText.text = response.Rank > 0 ? $"#{response.Rank}" : "";

            if (_rewardContainer != null)
            {
                for (int i = _rewardContainer.childCount - 1; i >= 0; i--)
                {
                    var child = _rewardContainer.GetChild(i);
                    if (child.gameObject == _rewardItemTemplate.gameObject) continue;
                    Destroy(child.gameObject);
                }
            }

            if (entries != null && _rewardContainer != null && _rewardItemTemplate != null)
            {
                foreach (var reward in entries)
                {
                    var item = Instantiate(_rewardItemTemplate, _rewardContainer);
                    item.Setup(reward);
                }
            }

            gameObject.SetActive(true);

            if (_queue.Count == 0)
                LeaderboardService.ConsumeStartupRewards();
        }

        // ── Editor test ───────────────────────────────────────────────────

#if UNITY_EDITOR
        [ContextMenu("Test / Show Sample Rewards")]
        private void TestShow()
        {
            Show(new List<ClaimCycleRewardResponse>
            {
                new()
                {
                    LeaderboardID = "weekly_pvp",
                    Rank          = 3,
                    Resources     = new ResourceOperation
                    {
                        Grant = new ResourceGrant
                        {
                            Standard = new ResourceBundle
                            {
                                Entries = new List<ResourceEntry>
                                {
                                    new() { Type = ResourceEntryType.VirtualCurrency, CurrencyID = "Coins", Amount = 500 },
                                    new() { Type = ResourceEntryType.VirtualCurrency, CurrencyID = "Gems",  Amount = 50  },
                                }
                            }
                        }
                    }
                },
                new()
                {
                    LeaderboardID = "monthly_solo",
                    Rank          = 7,
                    Resources     = new ResourceOperation
                    {
                        Grant = new ResourceGrant
                        {
                            Standard = new ResourceBundle
                            {
                                Entries = new List<ResourceEntry>
                                {
                                    new() { Type = ResourceEntryType.VirtualCurrency, CurrencyID = "Gems", Amount = 100 },
                                }
                            }
                        }
                    }
                }
            });
        }
#endif
    }
}
