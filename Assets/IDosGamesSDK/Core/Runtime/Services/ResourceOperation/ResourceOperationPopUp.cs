using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace IDosGames
{
    public class ResourceOperationPopUp : PopUp, IPointerClickHandler
    {
        [SerializeField] private Transform _consumesRoot;
        [SerializeField] private Transform _grantsRoot;
        [SerializeField] private ResourceOperationItemView _itemPrefab;
        [SerializeField] private GameObject _consumesSectionRoot;
        [SerializeField] private GameObject _grantsSectionRoot;
        [SerializeField] private float _spawnDelay = 0.5f;
        [SerializeField] private Button _backgroundButton;
        [SerializeField] private bool _showConsumeSection = true;

        private readonly List<ResourceOperationItemView> _spawnedItems = new();

        private Coroutine _showRoutine;

        private bool _isShowingItems;
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

        public bool WouldDisplay(ResourceOperation operation)
        {
            if (operation == null) return false;

            var grants = FlattenGrant(operation.Grant);
            if (grants.Count > 0) return true;

            if (!_showConsumeSection) return false;

            var consumes = FlattenConsume(operation.Consume);
            return consumes.Count > 0;
        }

        public void Set(ResourceOperation operation)
        {
            StopShowRoutine();
            Clear();

            _isShowingItems = false;
            _canClose = false;
            _skipCurrentDelayRequested = false;

            var consumes = _showConsumeSection ? FlattenConsume(operation?.Consume) : new List<ResourceEntry>();
            var grants = FlattenGrant(operation?.Grant);

            if (_consumesSectionRoot != null)
            {
                _consumesSectionRoot.SetActive(_showConsumeSection && consumes.Count > 0);
            }

            if (_grantsSectionRoot != null)
            {
                _grantsSectionRoot.SetActive(grants.Count > 0);
            }

            if (consumes.Count == 0 && grants.Count == 0)
            {
                _canClose = true;
                return;
            }

            _showRoutine = StartCoroutine(ShowEntriesSequentially(consumes, grants));
        }

        private IEnumerator ShowEntriesSequentially(IReadOnlyList<ResourceEntry> consumes, IReadOnlyList<ResourceEntry> grants)
        {
            _isShowingItems = true;

            for (int i = 0; i < consumes.Count; i++)
            {
                yield return WaitForNextEntry();

                if (_itemPrefab == null || _consumesRoot == null)
                {
                    _showRoutine = null;
                    _isShowingItems = false;
                    yield break;
                }

                SpawnEntry(consumes[i], _consumesRoot);
            }

            for (int i = 0; i < grants.Count; i++)
            {
                yield return WaitForNextEntry();

                if (_itemPrefab == null || _grantsRoot == null)
                {
                    _showRoutine = null;
                    _isShowingItems = false;
                    yield break;
                }

                SpawnEntry(grants[i], _grantsRoot);
            }

            _showRoutine = null;
            _isShowingItems = false;
            _canClose = true;
        }

        private void SpawnEntry(ResourceEntry entry, Transform parent)
        {
            var itemView = Instantiate(_itemPrefab, parent);
            itemView.Set(entry);
            _spawnedItems.Add(itemView);
        }

        private IEnumerator WaitForNextEntry()
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
            if (_isShowingItems)
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

            _isShowingItems = false;
            _canClose = false;
            _skipCurrentDelayRequested = false;

            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            StopShowRoutine();
            Clear();

            _isShowingItems = false;
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

        private static List<ResourceEntry> FlattenGrant(ResourceGrant grant)
        {
            var result = new List<ResourceEntry>();

            if (grant == null)
            {
                return result;
            }

            AppendEntries(result, grant.Standard);

            if (grant.PremiumTiers != null)
            {
                for (int i = 0; i < grant.PremiumTiers.Count; i++)
                {
                    AppendEntries(result, grant.PremiumTiers[i]?.Resources);
                }
            }

            return result;
        }

        private static List<ResourceEntry> FlattenConsume(ResourceConsume consume)
        {
            var result = new List<ResourceEntry>();

            if (consume == null)
            {
                return result;
            }

            AppendEntries(result, consume.Standard);

            if (consume.PremiumTiers != null)
            {
                for (int i = 0; i < consume.PremiumTiers.Count; i++)
                {
                    AppendEntries(result, consume.PremiumTiers[i]?.Resources);
                }
            }

            return result;
        }

        private static void AppendEntries(List<ResourceEntry> destination, ResourceBundle bundle)
        {
            var entries = bundle?.Entries;
            if (entries == null) return;

            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry == null) continue;
                destination.Add(entry);
            }
        }
    }
}
