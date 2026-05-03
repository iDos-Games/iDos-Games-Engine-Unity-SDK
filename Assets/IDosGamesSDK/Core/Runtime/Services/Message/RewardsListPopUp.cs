using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace IDosGames
{
    public class RewardsListPopUp : PopUp, IPointerClickHandler
    {
        [SerializeField] private Transform _contentRoot;
        [SerializeField] private RewardListItemView _itemPrefab;
        [SerializeField] private float _spawnDelay = 0.5f;
        [SerializeField] private Button _backgroundButton;

        private readonly List<RewardListItemView> _spawnedItems = new();

        private Coroutine _showRoutine;

        private bool _isShowingRewards;
        private bool _canClose;
        private bool _skipCurrentDelayRequested;

        private void Awake()
        {
            if (_backgroundButton != null)
            {
                _backgroundButton.onClick.AddListener(HandleTap);
            }
        }

        private void OnDestroy()
        {
            if (_backgroundButton != null)
            {
                _backgroundButton.onClick.RemoveListener(HandleTap);
            }
        }

        public void Set(IReadOnlyList<ItemOrCurrency> rewards)
        {
            StopShowRoutine();
            Clear();

            _isShowingRewards = false;
            _canClose = false;
            _skipCurrentDelayRequested = false;

            if (rewards == null || rewards.Count == 0)
            {
                _canClose = true;
                return;
            }

            _showRoutine = StartCoroutine(ShowRewardsSequentially(rewards));
        }

        private IEnumerator ShowRewardsSequentially(IReadOnlyList<ItemOrCurrency> rewards)
        {
            _isShowingRewards = true;

            for (int i = 0; i < rewards.Count; i++)
            {
                yield return WaitForNextReward();

                if (_contentRoot == null || _itemPrefab == null)
                {
                    _showRoutine = null;
                    _isShowingRewards = false;
                    yield break;
                }

                var itemView = Instantiate(_itemPrefab, _contentRoot);
                itemView.Set(rewards[i]);
                _spawnedItems.Add(itemView);
            }

            _showRoutine = null;
            _isShowingRewards = false;
            _canClose = true;
        }

        private IEnumerator WaitForNextReward()
        {
            if (_spawnDelay <= 0f)
            {
                _skipCurrentDelayRequested = false;
                yield break;
            }

            float elapsed = 0f;
            _skipCurrentDelayRequested = false;

            while (elapsed < _spawnDelay)
            {
                if (_skipCurrentDelayRequested)
                {
                    _skipCurrentDelayRequested = false;
                    yield break;
                }

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            _skipCurrentDelayRequested = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            HandleTap();
        }

        private void HandleTap()
        {
            if (_isShowingRewards)
            {
                _skipCurrentDelayRequested = true;
                return;
            }

            if (_canClose)
            {
                Close();
            }
        }

        public void Close()
        {
            StopShowRoutine();
            Clear();

            _isShowingRewards = false;
            _canClose = false;
            _skipCurrentDelayRequested = false;

            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            StopShowRoutine();
            Clear();

            _isShowingRewards = false;
            _canClose = false;
            _skipCurrentDelayRequested = false;
        }

        private void StopShowRoutine()
        {
            if (_showRoutine != null)
            {
                StopCoroutine(_showRoutine);
                _showRoutine = null;
            }
        }

        private void Clear()
        {
            for (int i = 0; i < _spawnedItems.Count; i++)
            {
                if (_spawnedItems[i] != null)
                {
                    Destroy(_spawnedItems[i].gameObject);
                }
            }

            _spawnedItems.Clear();
        }
    }
}
