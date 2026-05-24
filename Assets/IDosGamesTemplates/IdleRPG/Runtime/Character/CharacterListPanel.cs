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

            // Build the union: every character from the catalog is shown.
            //   - Owned in state          → real model, isLocked=false
            //   - UnlockedByDefault       → placeholder model (Level=0), isLocked=false
            //   - Locked                  → placeholder model, isLocked=true (overlay blocks clicks)
            var ordered = new List<(CharacterModel model, CharacterDefinition def, bool isLocked)>();

            if (defs != null)
            {
                foreach (var kv in defs)
                {
                    var def = kv.Value;
                    if (def == null) continue;

                    CharacterModel model = null;
                    characters?.TryGetValue(kv.Key, out model);

                    bool unlockedByDefault = def.Unlock?.UnlockedByDefault ?? false;
                    bool isLocked = model == null && !unlockedByDefault;

                    if (model == null)
                        model = CreatePlaceholderModel(kv.Key, def);

                    ordered.Add((model, def, isLocked));
                }
            }

            // Defensive: any state character without a matching definition.
            if (characters != null)
            {
                foreach (var kv in characters)
                {
                    if (kv.Value == null) continue;
                    if (defs != null && defs.ContainsKey(kv.Key)) continue;
                    ordered.Add((kv.Value, null, false));
                }
            }

            if (ordered.Count == 0)
            {
                HideAll();
                return;
            }

            ordered.Sort((a, b) =>
            {
                int sa = a.def?.Identity?.SortOrder ?? 0;
                int sb = b.def?.Identity?.SortOrder ?? 0;
                int cmp = sa.CompareTo(sb);
                return cmp != 0 ? cmp : string.CompareOrdinal(a.model.CharacterID, b.model.CharacterID);
            });

            for (int i = 0; i < ordered.Count; i++)
            {
                var item = GetOrSpawn(i);
                item.gameObject.SetActive(true);
                item.Bind(ordered[i].model, ordered[i].def, ordered[i].isLocked, OnItemSelected);
            }

            for (int i = ordered.Count; i < _spawned.Count; i++)
            {
                if (_spawned[i] != null)
                    _spawned[i].gameObject.SetActive(false);
            }
        }

        internal static CharacterModel CreatePlaceholderModel(string characterID, CharacterDefinition def)
        {
            return new CharacterModel
            {
                CharacterID = characterID,
                Class = def?.Classification?.ClassID,
                Name = def?.Identity?.DisplayName,
                Level = 0,
                Experience = 0,
                Power = 0,
                StatLevels = new Dictionary<string, int>(),
                Equipment = new Dictionary<string, EquippedItem>(),
            };
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
