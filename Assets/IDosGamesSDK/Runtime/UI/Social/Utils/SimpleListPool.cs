// File: Assets/IDosGamesSDK/Runtime/UI/Social/Utils/SimpleListPool.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace IDosGames.UI.Social
{
    /// <summary>
    /// Simple object pool for ScrollRect item views.
    /// Reuses existing children of a container, creating new ones only when needed.
    /// </summary>
    public class SimpleListPool<T> where T : MonoBehaviour
    {
        private readonly Transform _container;
        private readonly T _prefab;
        private readonly List<T> _activeItems = new List<T>();

        public IReadOnlyList<T> ActiveItems => _activeItems;

        public SimpleListPool(Transform container, T prefab)
        {
            _container = container;
            _prefab = prefab;
        }

        /// <summary>
        /// Adjusts the number of active items to match count.
        /// Returns the list of active items ready to be populated.
        /// </summary>
        public List<T> Resize(int count)
        {
            // Deactivate excess
            for (int i = count; i < _activeItems.Count; i++)
            {
                _activeItems[i].gameObject.SetActive(false);
            }

            // Activate or create needed
            for (int i = 0; i < count; i++)
            {
                if (i < _activeItems.Count)
                {
                    _activeItems[i].gameObject.SetActive(true);
                }
                else
                {
                    var item = UnityEngine.Object.Instantiate(_prefab, _container);
                    _activeItems.Add(item);
                }
            }

            return _activeItems.GetRange(0, count);
        }

        public void Clear()
        {
            foreach (var item in _activeItems)
            {
                item.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Removes and deactivates a single item by reference.
        /// </summary>
        public void Remove(T item)
        {
            if (item == null) return;
            item.gameObject.SetActive(false);
            // Item stays in list for reuse; just hidden
        }
    }
}
