using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
    /// <summary>
    /// Root of <c>Canvas/Equipment</c>. Driven externally by <c>Show(characterID)</c>
    /// (typically from <see cref="CharacterListPanel"/> or <see cref="CharacterDetailPanel"/>).
    /// Pattern mirrors <see cref="CharacterDetailPanel"/> + <see cref="CharacterListPanel"/>.
    /// </summary>
    public class EquipmentPanel : MonoBehaviour
    {
        [Serializable]
        public class StatReadout
        {
            [Tooltip("StatID from CharacterDefinition.Stats and matching key in ItemStats.FlatBonuses.")]
            public string StatKey;
            public GameObject Root;
            public Image Icon;
            public TextMeshProUGUI ValueText;
        }

        [Header("Root")]
        [SerializeField] private GameObject root;

        [Header("Header")]
        [SerializeField] private Image characterImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private List<StatReadout> statReadouts;

        [Header("Slots")]
        [SerializeField] private List<EquipmentSlotView> slots;

        [Header("Tabs / inventory grid")]
        [SerializeField] private List<EquipmentCategoryTab> tabs;
        [SerializeField] private Transform content;
        [SerializeField] private EquipmentItem itemPrefab;

        [Header("Buttons")]
        [SerializeField] private Button buttonUnequipAll;
        [SerializeField] private Button buttonEquipAll;
        [SerializeField] private Button buttonBack;
        [SerializeField] private Button buttonProperty;

        [Header("Popup")]
        [SerializeField] private EquipmentInfoPopup popup;

        [Header("Visuals")]
        [SerializeField] private EquipmentVisualConfig visuals;

        private readonly List<EquipmentItem> _spawned = new();
        private string _characterID;
        private string _activeClass;
        private string _loadedImagePath;

        // -------------------- Lifecycle --------------------

        private void Awake()
        {
            if (root == null) root = gameObject;

            if (buttonUnequipAll != null)
            {
                buttonUnequipAll.onClick.RemoveAllListeners();
                buttonUnequipAll.onClick.AddListener(OnUnequipAllClicked);
            }
            if (buttonBack != null)
            {
                buttonBack.onClick.RemoveAllListeners();
                buttonBack.onClick.AddListener(Hide);
            }
            if (buttonEquipAll != null)
            {
                buttonEquipAll.onClick.RemoveAllListeners();
                buttonEquipAll.onClick.AddListener(OnEquipAllClicked);
            }
            if (buttonProperty != null)
            {
                buttonProperty.onClick.RemoveAllListeners();
                buttonProperty.onClick.AddListener(OnPropertyClicked);
            }

            BindTabs();
        }

        private void OnEnable()
        {
            CharacterService.OnItemsEquipped           += HandleItemsEquipped;
            CharacterService.OnItemsUnequipped         += HandleItemsUnequipped;
            CharacterService.OnAllCharactersUnequipped += HandleAllUnequipped;

            if (IDosGamesData.User != null)
            {
                IDosGamesData.User.OnCharacterUpdated += Refresh;
                IDosGamesData.User.OnInventoryUpdated += Refresh;
            }

            EnsureDataLoaded();
            Refresh();
        }

        private void OnDisable()
        {
            CharacterService.OnItemsEquipped           -= HandleItemsEquipped;
            CharacterService.OnItemsUnequipped         -= HandleItemsUnequipped;
            CharacterService.OnAllCharactersUnequipped -= HandleAllUnequipped;

            if (IDosGamesData.User != null)
            {
                IDosGamesData.User.OnCharacterUpdated -= Refresh;
                IDosGamesData.User.OnInventoryUpdated -= Refresh;
            }
        }

        // -------------------- Public API --------------------

        public void Show(string characterID)
        {
            if (string.IsNullOrWhiteSpace(characterID)) return;
            _characterID = characterID;
            _loadedImagePath = null;
            EnsureRoot().SetActive(true);
            Refresh();
        }

        public void Hide()
        {
            EnsureRoot().SetActive(false);
            _characterID = null;
            _loadedImagePath = null;
        }

        private GameObject EnsureRoot() => root != null ? root : (root = gameObject);

        // -------------------- Data load --------------------

        private async void EnsureDataLoaded()
        {
            if (IDosGamesData.Config?.TitlePublicConfiguration?.Character?.Definitions == null)
                await CharacterService.GetCharacterDefinitions();

            if (IDosGamesData.User?.State?.Character?.Characters == null
                || IDosGamesData.User.State.Character.Characters.Count == 0)
            {
                await CharacterService.GetUserCharacters();
            }

            if (IDosGamesData.User?.State?.InventoryV2?.UnstackableItems == null)
                await UserService.GetUserInventory();
        }

        // -------------------- Tabs --------------------

        private void BindTabs()
        {
            if (tabs == null) return;

            for (int i = 0; i < tabs.Count; i++)
            {
                var tab = tabs[i];
                if (tab == null) continue;
                var icon = ResolveClassIcon(tab.ItemClass);
                bool isFirst = i == 0;
                tab.Bind(icon, isFirst, OnTabSelected);
                if (isFirst) _activeClass = tab.ItemClass;
            }
        }

        private void OnTabSelected(string itemClass)
        {
            _activeClass = itemClass;
            if (tabs != null)
                for (int i = 0; i < tabs.Count; i++)
                    if (tabs[i] != null)
                        tabs[i].SetSelected(string.Equals(tabs[i].ItemClass, itemClass, StringComparison.OrdinalIgnoreCase));

            RefreshInventoryGrid();
        }

        // -------------------- Refresh --------------------

        private void Refresh()
        {
            if (string.IsNullOrEmpty(_characterID))
            {
                HideAllTiles();
                return;
            }

            CharacterDefinition def = null;
            var defs = IDosGamesData.Config?.TitlePublicConfiguration?.Character?.Definitions;
            defs?.TryGetValue(_characterID, out def);

            CharacterModel model = null;
            var characters = IDosGamesData.User?.State?.Character?.Characters;
            characters?.TryGetValue(_characterID, out model);

            if (nameText != null)
            {
                nameText.text = !string.IsNullOrEmpty(model?.Name)
                    ? model.Name
                    : def?.Identity?.DisplayName ?? _characterID;
            }

            RefreshCharacterImage(def);
            RefreshSlots(model, def);
            RefreshStatReadouts(model, def);
            RefreshInventoryGrid();
        }

        private void RefreshCharacterImage(CharacterDefinition def)
        {
            if (characterImage == null) return;
            var assetPaths = def?.Identity?.AssetPaths;
            if (assetPaths == null) return;

            string path = null;
            if (assetPaths.TryGetValue("portrait", out var p) && !string.IsNullOrWhiteSpace(p)) path = p;
            else if (assetPaths.TryGetValue("fullArt", out var f) && !string.IsNullOrWhiteSpace(f)) path = f;
            else if (assetPaths.TryGetValue("icon", out var i) && !string.IsNullOrWhiteSpace(i)) path = i;

            if (path != _loadedImagePath)
            {
                _loadedImagePath = path;
                _ = LoadCharacterImage(path);
            }
        }

        private async Task LoadCharacterImage(string path)
        {
            if (string.IsNullOrEmpty(path) || characterImage == null) return;
            var sprite = await ImageLoader.GetSpriteAsync(path);
            if (sprite != null && this != null && characterImage != null && _loadedImagePath == path)
                characterImage.sprite = sprite;
        }

        private void RefreshSlots(CharacterModel model, CharacterDefinition charDef)
        {
            if (slots == null) return;

            var unstackables = IDosGamesData.User?.State?.InventoryV2?.UnstackableItems;
            int characterLevel = model?.Level ?? 0;

            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                if (slot == null) continue;

                CharacterEquipmentSlot rule = null;
                charDef?.Equipment?.Slots?.TryGetValue(slot.SlotID, out rule);

                EquippedItem equipped = null;
                model?.Equipment?.TryGetValue(slot.SlotID, out equipped);

                UnstackableItemInstanceState inst = null;
                if (equipped != null && !string.IsNullOrWhiteSpace(equipped.ItemInstanceID))
                    unstackables?.TryGetValue(equipped.ItemInstanceID, out inst);

                ItemDefinition itemDef = ResolveItemDefinition(inst, equipped);

                bool isLocked = rule != null && characterLevel < rule.MinCharacterLevel;
                var rarity = ResolveRarity(itemDef?.Metadata?.RarityID);
                var classSprite = ResolveClassIcon(itemDef?.ItemClass);

                slot.Bind(equipped, inst, itemDef, isLocked, rarity, classSprite, OnSlotClicked);
            }
        }

        private void RefreshStatReadouts(CharacterModel model, CharacterDefinition charDef)
        {
            if (statReadouts == null) return;

            for (int i = 0; i < statReadouts.Count; i++)
            {
                var row = statReadouts[i];
                if (row == null) continue;

                if (string.IsNullOrWhiteSpace(row.StatKey) || charDef?.Stats == null
                    || !charDef.Stats.TryGetValue(row.StatKey, out var statDef))
                {
                    if (row.Root != null) row.Root.SetActive(false);
                    continue;
                }

                if (row.Root != null) row.Root.SetActive(true);

                int statLevel = 0;
                model?.StatLevels?.TryGetValue(row.StatKey, out statLevel);
                double baseValue = CharacterStatMath.ComputeValueAtLevel(statDef, statLevel);

                AggregateEquippedBonuses(model, row.StatKey, out double equippedFlat, out double equippedPercent);

                double total = (baseValue + equippedFlat) * (1.0 + equippedPercent);
                if (row.ValueText != null) row.ValueText.text = $"{total:0}";
            }
        }

        private void AggregateEquippedBonuses(CharacterModel model, string statKey, out double flat, out double percent)
        {
            flat = 0;
            percent = 0;
            if (model?.Equipment == null) return;

            var unstackables = IDosGamesData.User?.State?.InventoryV2?.UnstackableItems;
            foreach (var kv in model.Equipment)
            {
                var equipped = kv.Value;
                if (equipped == null || string.IsNullOrWhiteSpace(equipped.ItemInstanceID)) continue;

                UnstackableItemInstanceState inst = null;
                unstackables?.TryGetValue(equipped.ItemInstanceID, out inst);

                var itemDef = ResolveItemDefinition(inst, equipped);
                if (itemDef?.Stats == null) continue;

                int level = Mathf.Max(1, inst?.Level ?? 1);
                var (f, p) = ItemUpgradeMath.GetScaledBonuses(itemDef.Stats, statKey, itemDef.Equipment?.Upgrade, level);
                flat += f;
                percent += p;
            }
        }

        // -------------------- Inventory grid --------------------

        private void RefreshInventoryGrid()
        {
            if (content == null || itemPrefab == null)
            {
                HideAllTiles();
                return;
            }

            var entries = CollectInventoryEntries();

            entries.Sort((a, b) =>
            {
                int lvlCmp = (b.Instance?.Level ?? 0).CompareTo(a.Instance?.Level ?? 0);
                if (lvlCmp != 0) return lvlCmp;
                return string.CompareOrdinal(a.ItemID, b.ItemID);
            });

            for (int i = 0; i < entries.Count; i++)
            {
                var tile = GetOrSpawn(i);
                tile.gameObject.SetActive(true);

                var e = entries[i];
                var rarity = ResolveRarity(e.Def?.Metadata?.RarityID);
                var classSprite = ResolveClassIcon(e.Def?.ItemClass);

                if (e.IsStackable)
                {
                    tile.BindStackable(e.ItemID, e.Amount, e.Def,
                        EquipmentItemMode.InventoryGrid, rarity, classSprite, OnStackableTileClicked);
                }
                else
                {
                    tile.BindUnstackable(e.Instance, e.Def,
                        EquipmentItemMode.InventoryGrid, e.IsEquipped, rarity, classSprite, OnUnstackableTileClicked);
                }
            }

            for (int i = entries.Count; i < _spawned.Count; i++)
                if (_spawned[i] != null)
                    _spawned[i].gameObject.SetActive(false);
        }

        private struct TileEntry
        {
            public bool IsStackable;
            public UnstackableItemInstanceState Instance;
            public string ItemID;
            public long Amount;
            public ItemDefinition Def;
            public bool IsEquipped;
        }

        private List<TileEntry> CollectInventoryEntries()
        {
            var entries = new List<TileEntry>();
            var state = IDosGamesData.User?.State?.InventoryV2;
            if (state == null) return entries;

            if (state.UnstackableItems != null)
            {
                foreach (var kv in state.UnstackableItems)
                {
                    var inst = kv.Value;
                    if (inst == null) continue;

                    var def = ResolveItemDefinition(inst, null);
                    if (def == null) continue;
                    if (!MatchesActiveTab(def)) continue;

                    entries.Add(new TileEntry
                    {
                        IsStackable = false,
                        Instance = inst,
                        ItemID = inst.ItemID,
                        Amount = 0,
                        Def = def,
                        IsEquipped = inst.EquippedSlot != null && !string.IsNullOrWhiteSpace(inst.EquippedSlot.SlotID),
                    });
                }
            }

            if (state.Items != null)
            {
                foreach (var kv in state.Items)
                {
                    var totals = kv.Value;
                    if (totals == null || totals.StackableAmount <= 0) continue;

                    var def = ResolveItemDefinitionByID(kv.Key);
                    if (def == null) continue;
                    if (!def.IsStackable) continue;
                    if (!MatchesActiveTab(def)) continue;

                    entries.Add(new TileEntry
                    {
                        IsStackable = true,
                        Instance = null,
                        ItemID = kv.Key,
                        Amount = totals.StackableAmount,
                        Def = def,
                        IsEquipped = false,
                    });
                }
            }

            return entries;
        }

        private bool MatchesActiveTab(ItemDefinition def)
        {
            if (string.IsNullOrEmpty(_activeClass)) return true;
            return string.Equals(def?.ItemClass, _activeClass, StringComparison.OrdinalIgnoreCase);
        }

        private EquipmentItem GetOrSpawn(int index)
        {
            while (_spawned.Count <= index)
                _spawned.Add(Instantiate(itemPrefab, content));
            return _spawned[index];
        }

        private void HideAllTiles()
        {
            for (int i = 0; i < _spawned.Count; i++)
                if (_spawned[i] != null)
                    _spawned[i].gameObject.SetActive(false);
        }

        // -------------------- Click handlers --------------------

        private void OnSlotClicked(string slotID, string itemInstanceID)
        {
            if (popup == null) return;
            if (!string.IsNullOrWhiteSpace(itemInstanceID))
                popup.Show(itemInstanceID, _characterID, slotID);
            // Empty slot click: player picks an item from the grid below.
        }

        private void OnUnstackableTileClicked(string itemInstanceID)
        {
            if (popup == null || string.IsNullOrWhiteSpace(itemInstanceID)) return;
            popup.Show(itemInstanceID, _characterID);
        }

        private void OnStackableTileClicked(string itemID)
        {
            Debug.Log($"[EquipmentPanel] TODO: stackable item '{itemID}' clicked — no popup wired for stackables yet.");
        }

        private async void OnUnequipAllClicked()
        {
            if (string.IsNullOrWhiteSpace(_characterID)) return;

            CharacterModel model = null;
            IDosGamesData.User?.State?.Character?.Characters?.TryGetValue(_characterID, out model);
            if (model?.Equipment == null || model.Equipment.Count == 0) return;

            var slotIDs = model.Equipment.Keys.ToList();
            if (buttonUnequipAll != null) buttonUnequipAll.interactable = false;

            var result = await CharacterService.UnequipItems(_characterID, slotIDs);

            if (this == null) return;
            if (buttonUnequipAll != null) buttonUnequipAll.interactable = true;
            if (result != null && !result.Success) Refresh();
        }

        private void OnEquipAllClicked()
        {
            Debug.Log("[EquipmentPanel] TODO: auto-equip-best flow not implemented yet.");
        }

        private void OnPropertyClicked()
        {
            Debug.Log("[EquipmentPanel] TODO: property flow not implemented yet.");
        }

        // -------------------- Helpers --------------------

        private static ItemDefinition ResolveItemDefinition(UnstackableItemInstanceState inst, EquippedItem equipped)
        {
            var catalogs = IDosGamesData.Config?.TitlePublicConfiguration?.Item?.Catalogs;
            if (catalogs == null) return null;

            string catalogID = inst?.CatalogID ?? equipped?.CatalogID;
            string itemID = inst?.ItemID ?? equipped?.ItemID;
            if (string.IsNullOrWhiteSpace(itemID)) return null;

            if (!string.IsNullOrWhiteSpace(catalogID)
                && catalogs.TryGetValue(catalogID, out var c)
                && c?.Items != null
                && c.Items.TryGetValue(itemID, out var direct))
            {
                return direct;
            }

            foreach (var cat in catalogs.Values)
            {
                if (cat?.Items == null) continue;
                if (cat.Items.TryGetValue(itemID, out var d)) return d;
            }
            return null;
        }

        private static ItemDefinition ResolveItemDefinitionByID(string itemID)
        {
            if (string.IsNullOrWhiteSpace(itemID)) return null;
            var catalogs = IDosGamesData.Config?.TitlePublicConfiguration?.Item?.Catalogs;
            if (catalogs == null) return null;

            foreach (var cat in catalogs.Values)
            {
                if (cat?.Items == null) continue;
                if (cat.Items.TryGetValue(itemID, out var d)) return d;
            }
            return null;
        }

        private ItemRarityVisual ResolveRarity(string rarityID) => visuals?.ResolveRarityVisual(rarityID);
        private Sprite ResolveClassIcon(string itemClass) => visuals?.ResolveClassIcon(itemClass);

        // -------------------- Event handlers --------------------

        private void HandleItemsEquipped(string charID, List<EquipSlotPair> pairs) => Refresh();
        private void HandleItemsUnequipped(string charID, List<string> slotIDs) => Refresh();
        private void HandleAllUnequipped() => Refresh();
    }
}
