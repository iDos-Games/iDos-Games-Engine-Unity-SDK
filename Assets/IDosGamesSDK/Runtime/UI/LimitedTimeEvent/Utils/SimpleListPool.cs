using System.Collections.Generic;
using UnityEngine;

namespace IDosGames.UI.LimitedTimeEvent
{
    /// <summary>
    /// Minimal pool that reuses child GameObjects of a given prefab type.
    /// Avoids allocating new objects every refresh.
    /// </summary>
    public class SimpleListPool<T> where T : MonoBehaviour
    {
        private readonly T         _prefab;
        private readonly Transform _parent;
        private readonly List<T>   _all    = new List<T>();
        private          int       _activeCount;

        public SimpleListPool(T prefab, Transform parent, int preWarm = 0)
        {
            _prefab = prefab;
            _parent = parent;

            for (int i = 0; i < preWarm; i++)
            {
                var obj = Object.Instantiate(_prefab, _parent);
                obj.gameObject.SetActive(false);
                _all.Add(obj);
            }
        }

        /// <summary>Get (or create) the next item and activate it.</summary>
        public T Get()
        {
            T item;
            if (_activeCount < _all.Count)
            {
                item = _all[_activeCount];
            }
            else
            {
                item = Object.Instantiate(_prefab, _parent);
                _all.Add(item);
            }
            item.gameObject.SetActive(true);
            item.transform.SetSiblingIndex(_activeCount);
            _activeCount++;
            return item;
        }

        /// <summary>Deactivate all items; next Get() will reuse from the top.</summary>
        public void ReturnAll()
        {
            for (int i = 0; i < _activeCount; i++)
                _all[i].gameObject.SetActive(false);
            _activeCount = 0;
        }

        public int ActiveCount => _activeCount;
    }
}
