using System.Collections.Generic;
using UnityEngine;

namespace IDosGames
{
    public class CharacterListPanel : MonoBehaviour
    {
        [Header("List")]
        [SerializeField] private Transform content;
        [SerializeField] private CharacterListItem itemPrefab;

        [Header("Detail")]
        [SerializeField] private CharacterDetailPanel detailPanel;

        private readonly List<CharacterListItem> _spawned = new();

        private void OnEnable()
        {
            CharacterService.OnUserCharactersLoaded       += HandleCharactersLoaded;
            CharacterService.OnCharacterDefinitionsLoaded += HandleDefinitionsLoaded;
            CharacterService.OnCharacterLevelUpgraded     += HandleCharacterLevelUpgraded;
            CharacterService.OnStatLevelUpgraded          += HandleStatLevelUpgraded;

            if (IDosGamesData.User != null)
                IDosGamesData.User.OnCharacterUpdated += Refresh;

            Refresh();
            EnsureDataLoaded();
        }

        private void OnDisable()
        {
            CharacterService.OnUserCharactersLoaded       -= HandleCharactersLoaded;
            CharacterService.OnCharacterDefinitionsLoaded -= HandleDefinitionsLoaded;
            CharacterService.OnCharacterLevelUpgraded     -= HandleCharacterLevelUpgraded;
            CharacterService.OnStatLevelUpgraded          -= HandleStatLevelUpgraded;

            if (IDosGamesData.User != null)
                IDosGamesData.User.OnCharacterUpdated -= Refresh;
        }

        private async void EnsureDataLoaded()
        {
            if (IDosGamesData.Config?.TitlePublicConfiguration?.Character?.Definitions == null)
                await CharacterService.GetCharacterDefinitions();

            if (IDosGamesData.User?.State?.Character?.Characters == null
                || IDosGamesData.User.State.Character.Characters.Count == 0)
            {
                await CharacterService.GetUserCharacters();
            }
        }

        private void Refresh()
        {
            if (content == null || itemPrefab == null) return;

            var characters = IDosGamesData.User?.State?.Character?.Characters;
            var defs       = IDosGamesData.Config?.TitlePublicConfiguration?.Character?.Definitions;

            if (characters == null || characters.Count == 0)
            {
                HideAll();
                return;
            }

            var ordered = new List<CharacterModel>(characters.Values);
            ordered.Sort((a, b) =>
            {
                int sa = (defs != null && defs.TryGetValue(a.CharacterID, out var da) && da?.Identity != null) ? da.Identity.SortOrder : 0;
                int sb = (defs != null && defs.TryGetValue(b.CharacterID, out var db) && db?.Identity != null) ? db.Identity.SortOrder : 0;
                int cmp = sa.CompareTo(sb);
                return cmp != 0 ? cmp : string.CompareOrdinal(a.CharacterID, b.CharacterID);
            });

            for (int i = 0; i < ordered.Count; i++)
            {
                var model = ordered[i];
                CharacterDefinition def = null;
                if (defs != null && !string.IsNullOrEmpty(model?.CharacterID))
                    defs.TryGetValue(model.CharacterID, out def);

                var item = GetOrSpawn(i);
                item.gameObject.SetActive(true);
                item.Bind(model, def, OnItemSelected);
            }

            for (int i = ordered.Count; i < _spawned.Count; i++)
            {
                if (_spawned[i] != null)
                    _spawned[i].gameObject.SetActive(false);
            }
        }

        private CharacterListItem GetOrSpawn(int index)
        {
            while (_spawned.Count <= index)
            {
                var spawned = Instantiate(itemPrefab, content);
                _spawned.Add(spawned);
            }
            return _spawned[index];
        }

        private void HideAll()
        {
            for (int i = 0; i < _spawned.Count; i++)
                if (_spawned[i] != null)
                    _spawned[i].gameObject.SetActive(false);
        }

        private void OnItemSelected(string characterID)
        {
            if (detailPanel != null)
                detailPanel.Show(characterID);
        }

        private void HandleCharactersLoaded(Dictionary<string, CharacterModel> _) => Refresh();
        private void HandleDefinitionsLoaded(CharacterDefinitions _) => Refresh();
        private void HandleCharacterLevelUpgraded(UpgradeCharacterLevelResponse _) => Refresh();
        private void HandleStatLevelUpgraded(UpgradeStatLevelResponse _) => Refresh();
    }
}
