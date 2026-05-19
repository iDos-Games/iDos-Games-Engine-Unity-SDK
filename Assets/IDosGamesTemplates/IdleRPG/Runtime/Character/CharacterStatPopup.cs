using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
    /// <summary>
    /// Modal popup spawned from <see cref="CharacterDetailPanel"/>. Lists every
    /// <see cref="StatDefinition"/> of the current character with an upgrade button.
    /// </summary>
    public class CharacterStatPopup : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField] private GameObject root;

        [Header("List")]
        [SerializeField] private Transform content;
        [SerializeField] private CharacterStatPopupItem itemPrefab;

        [Header("Close")]
        [SerializeField] private Button closeButton;

        private readonly List<CharacterStatPopupItem> _spawned = new();
        private string _characterID;

        private void Awake()
        {
            if (root == null) root = gameObject;
            root.SetActive(false);

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(Hide);
            }
        }

        private void OnEnable()
        {
            CharacterService.OnStatLevelUpgraded += HandleStatLevelUpgraded;
            if (IDosGamesData.User != null)
                IDosGamesData.User.OnCharacterUpdated += Refresh;
        }

        private void OnDisable()
        {
            CharacterService.OnStatLevelUpgraded -= HandleStatLevelUpgraded;
            if (IDosGamesData.User != null)
                IDosGamesData.User.OnCharacterUpdated -= Refresh;
        }

        public void Show(string characterID)
        {
            if (string.IsNullOrEmpty(characterID)) return;

            _characterID = characterID;
            EnsureRoot().SetActive(true);
            Refresh();
        }

        public void Hide()
        {
            EnsureRoot().SetActive(false);
            _characterID = null;
        }

        private GameObject EnsureRoot()
        {
            if (root == null) root = gameObject;
            return root;
        }

        private void Refresh()
        {
            if (string.IsNullOrEmpty(_characterID) || content == null || itemPrefab == null) return;

            var characters = IDosGamesData.User?.State?.Character?.Characters;
            if (characters == null || !characters.TryGetValue(_characterID, out var model) || model == null)
            {
                HideAll();
                return;
            }

            CharacterDefinition def = null;
            var defs = IDosGamesData.Config?.TitlePublicConfiguration?.Character?.Definitions;
            if (defs != null) defs.TryGetValue(_characterID, out def);

            if (def?.Stats == null || def.Stats.Count == 0)
            {
                HideAll();
                return;
            }

            int i = 0;
            foreach (var kv in def.Stats)
            {
                if (kv.Value == null) continue;

                var item = GetOrSpawn(i);
                item.gameObject.SetActive(true);
                item.Bind(kv.Key, kv.Value, model, def, _characterID);
                i++;
            }

            for (int j = i; j < _spawned.Count; j++)
                if (_spawned[j] != null)
                    _spawned[j].gameObject.SetActive(false);
        }

        private CharacterStatPopupItem GetOrSpawn(int index)
        {
            while (_spawned.Count <= index)
                _spawned.Add(Instantiate(itemPrefab, content));
            return _spawned[index];
        }

        private void HideAll()
        {
            for (int i = 0; i < _spawned.Count; i++)
                if (_spawned[i] != null)
                    _spawned[i].gameObject.SetActive(false);
        }

        private void HandleStatLevelUpgraded(UpgradeStatLevelResponse data)
        {
            if (data != null && data.CharacterID == _characterID)
                Refresh();
        }
    }
}
