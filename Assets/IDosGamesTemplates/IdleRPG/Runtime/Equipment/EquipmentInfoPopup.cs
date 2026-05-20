using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
    /// <summary>
    /// Controls <c>Canvas/Popup_EquipmentItemInfo</c>. Renders one item instance and exposes
    /// Equip + Level Up actions. Mirrors <see cref="CharacterStatPopup"/>'s lifecycle.
    /// </summary>
    public class EquipmentInfoPopup : MonoBehaviour
    {
        [Serializable]
        public class StatRow
        {
            public GameObject Root;
            public Image Icon;
            public TextMeshProUGUI NameText;
            public TextMeshProUGUI ValueText;
        }

        [Header("Root")]
        [SerializeField] private GameObject root;

        [Header("Header")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI gradeText;
        [SerializeField] private Image roleIcon;
        [SerializeField] private Image itemImage;
        [SerializeField] private TextMeshProUGUI infoText;
        [SerializeField] private List<Image> basicFrameImages;

        [Header("Stars")]
        [SerializeField] private List<GameObject> stars;
        [SerializeField] private List<GameObject> specialStars;

        [Header("Stats")]
        [SerializeField] private List<StatRow> statRows;
        [SerializeField] private List<ItemStatDisplay> statDisplays;

        [Header("Buttons")]
        [SerializeField] private Button equipButton;
        [SerializeField] private Button levelUpButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI levelUpCostText;
        [SerializeField] private TextMeshProUGUI equipButtonLabel;

        [Header("Mappings")]
        [SerializeField] private List<ItemRarityLabel> rarityLabels;
        [SerializeField] private List<ItemClassIcon> classIcons;

        private string _instanceID;
        private string _characterID;
        private string _targetSlotID;
        private string _loadedImagePath;

        private void Awake()
        {
            if (root == null) root = gameObject;

            if (equipButton != null)
            {
                equipButton.onClick.RemoveAllListeners();
                equipButton.onClick.AddListener(OnEquipClicked);
            }
            if (levelUpButton != null)
            {
                levelUpButton.onClick.RemoveAllListeners();
                levelUpButton.onClick.AddListener(OnLevelUpClicked);
            }
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(Hide);
            }
        }

        private void OnEnable()
        {
            CharacterService.OnItemsEquipped += HandleItemsEquipped;
            CharacterService.OnItemsUnequipped += HandleItemsUnequipped;
            if (IDosGamesData.User != null)
                IDosGamesData.User.OnInventoryUpdated += Refresh;
        }

        private void OnDisable()
        {
            CharacterService.OnItemsEquipped -= HandleItemsEquipped;
            CharacterService.OnItemsUnequipped -= HandleItemsUnequipped;
            if (IDosGamesData.User != null)
                IDosGamesData.User.OnInventoryUpdated -= Refresh;
        }

        /// <summary>
        /// Show the popup for a specific instance. <paramref name="targetSlotID"/> overrides
        /// the slot the Equip button will target (otherwise we pick the first allowed slot
        /// from the item's <c>AllowedSlotIDs</c>).
        /// </summary>
        public void Show(string itemInstanceID, string characterID, string targetSlotID = null)
        {
            if (string.IsNullOrWhiteSpace(itemInstanceID)) return;

            _instanceID = itemInstanceID;
            _characterID = characterID;
            _targetSlotID = targetSlotID;
            _loadedImagePath = null;

            EnsureRoot().SetActive(true);
            Refresh();
        }

        public void Hide()
        {
            EnsureRoot().SetActive(false);
            _instanceID = null;
            _characterID = null;
            _targetSlotID = null;
            _loadedImagePath = null;
        }

        private GameObject EnsureRoot() => root != null ? root : (root = gameObject);

        private void Refresh()
        {
            if (string.IsNullOrEmpty(_instanceID)) return;

            var state = IDosGamesData.User?.State;
            UnstackableItemInstanceState inst = null;
            state?.InventoryV2?.UnstackableItems?.TryGetValue(_instanceID, out inst);

            if (inst == null)
            {
                Hide();
                return;
            }

            var def = ResolveDefinition(inst);

            if (titleText != null)
                titleText.text = def?.DisplayName ?? inst.ItemID ?? string.Empty;
            if (infoText != null)
                infoText.text = def?.Description ?? string.Empty;

            ApplyRarity(def?.Metadata?.RarityID);
            ApplyClassIcon(def?.ItemClass);
            ApplyStats(inst, def);

            string imgPath = ResolveImagePath(def);
            if (imgPath != _loadedImagePath)
            {
                _loadedImagePath = imgPath;
                _ = LoadImage(imgPath);
            }

            ApplyEquipButton(inst, def);
            ApplyLevelUpButton(inst, def);
        }

        // ===== Rarity / class =====

        private void ApplyRarity(string rarityID)
        {
            ItemRarityLabel label = ResolveRarity(rarityID);
            if (label == null) return;

            if (gradeText != null) gradeText.text = label.DisplayName ?? string.Empty;

            if (basicFrameImages != null)
                for (int i = 0; i < basicFrameImages.Count; i++)
                    if (basicFrameImages[i] != null)
                        basicFrameImages[i].color = label.BasicFrameColor;

            ApplyStars(label.UseSpecialStars ? specialStars : stars, label.StarCount);
            ApplyStars(label.UseSpecialStars ? stars : specialStars, 0);
        }

        private ItemRarityLabel ResolveRarity(string rarityID)
        {
            if (rarityLabels == null || rarityLabels.Count == 0) return null;

            if (!string.IsNullOrEmpty(rarityID))
            {
                for (int i = 0; i < rarityLabels.Count; i++)
                {
                    var l = rarityLabels[i];
                    if (l != null && string.Equals(l.RarityID, rarityID, StringComparison.OrdinalIgnoreCase))
                        return l;
                }
            }
            return rarityLabels[0];
        }

        private static void ApplyStars(List<GameObject> list, int activeCount)
        {
            if (list == null) return;
            for (int i = 0; i < list.Count; i++)
                if (list[i] != null)
                    list[i].SetActive(i < activeCount);
        }

        private void ApplyClassIcon(string itemClass)
        {
            if (roleIcon == null || classIcons == null || classIcons.Count == 0 || string.IsNullOrEmpty(itemClass))
                return;

            for (int i = 0; i < classIcons.Count; i++)
            {
                var c = classIcons[i];
                if (c != null && string.Equals(c.ItemClass, itemClass, StringComparison.OrdinalIgnoreCase))
                {
                    if (c.Icon != null) roleIcon.sprite = c.Icon;
                    return;
                }
            }
        }

        // ===== Stats =====

        private void ApplyStats(UnstackableItemInstanceState inst, ItemDefinition def)
        {
            if (statRows == null || statRows.Count == 0) return;

            int level = Mathf.Max(1, inst?.Level ?? 1);
            var upgrade = def?.Equipment?.Upgrade;
            var stats = def?.Stats;

            var keys = new List<string>();
            if (stats?.FlatBonuses != null)
                foreach (var k in stats.FlatBonuses.Keys) keys.Add(k);
            if (stats?.PercentBonuses != null)
                foreach (var k in stats.PercentBonuses.Keys)
                    if (!keys.Contains(k)) keys.Add(k);

            for (int i = 0; i < statRows.Count; i++)
            {
                var row = statRows[i];
                if (row == null) continue;

                if (i >= keys.Count)
                {
                    if (row.Root != null) row.Root.SetActive(false);
                    continue;
                }

                string key = keys[i];
                var display = ResolveStatDisplay(key);

                if (row.Root != null) row.Root.SetActive(true);
                if (row.Icon != null && display?.Icon != null) row.Icon.sprite = display.Icon;
                if (row.NameText != null)
                    row.NameText.text = !string.IsNullOrEmpty(display?.DisplayName) ? display.DisplayName : key;

                var scaled = ItemUpgradeMath.GetScaledBonuses(stats, key, upgrade, level);
                string valueText = FormatStat(scaled.flat, scaled.percent);
                if (row.ValueText != null) row.ValueText.text = valueText;
            }
        }

        private ItemStatDisplay ResolveStatDisplay(string key)
        {
            if (statDisplays == null) return null;
            for (int i = 0; i < statDisplays.Count; i++)
            {
                var d = statDisplays[i];
                if (d != null && string.Equals(d.Key, key, StringComparison.OrdinalIgnoreCase))
                    return d;
            }
            return null;
        }

        private static string FormatStat(double flat, double percent)
        {
            if (System.Math.Abs(flat) > 0 && System.Math.Abs(percent) > 0)
                return $"{flat:0.##} (+{percent * 100:0.#}%)";
            if (System.Math.Abs(percent) > 0)
                return $"{percent * 100:0.#}%";
            return $"{flat:0.##}";
        }

        // ===== Equip =====

        private void ApplyEquipButton(UnstackableItemInstanceState inst, ItemDefinition def)
        {
            if (equipButton == null) return;

            bool isEquipped = inst?.EquippedSlot != null
                              && !string.IsNullOrWhiteSpace(inst.EquippedSlot.SlotID);

            string slot = ResolveEquipTargetSlot(inst, def, out bool slotAllowed);

            equipButton.interactable = !isEquipped && slotAllowed && !string.IsNullOrWhiteSpace(_characterID);
            if (equipButtonLabel != null)
                equipButtonLabel.text = isEquipped ? "Unequip" : "Equip";
        }

        private string ResolveEquipTargetSlot(UnstackableItemInstanceState inst, ItemDefinition def, out bool allowed)
        {
            allowed = false;

            if (!string.IsNullOrWhiteSpace(_targetSlotID))
            {
                allowed = true;
                return _targetSlotID;
            }

            var allowedSlots = def?.Equipment?.AllowedSlotIDs;
            if (allowedSlots == null || allowedSlots.Count == 0) return null;

            CharacterDefinition charDef = null;
            var defs = IDosGamesData.Config?.TitlePublicConfiguration?.Character?.Definitions;
            if (!string.IsNullOrWhiteSpace(_characterID))
                defs?.TryGetValue(_characterID, out charDef);

            var charSlots = charDef?.Equipment?.Slots;

            for (int i = 0; i < allowedSlots.Count; i++)
            {
                var s = allowedSlots[i];
                if (string.IsNullOrWhiteSpace(s)) continue;
                if (charSlots == null || charSlots.ContainsKey(s))
                {
                    allowed = true;
                    return s;
                }
            }

            return allowedSlots[0];
        }

        private async void OnEquipClicked()
        {
            if (string.IsNullOrWhiteSpace(_instanceID) || string.IsNullOrWhiteSpace(_characterID)) return;

            var state = IDosGamesData.User?.State;
            UnstackableItemInstanceState inst = null;
            state?.InventoryV2?.UnstackableItems?.TryGetValue(_instanceID, out inst);
            if (inst == null) return;

            bool isEquipped = inst.EquippedSlot != null && !string.IsNullOrWhiteSpace(inst.EquippedSlot.SlotID);

            if (equipButton != null) equipButton.interactable = false;

            OperationResult<SuccessResponse> result;
            if (isEquipped)
            {
                result = await CharacterService.UnequipItems(
                    inst.EquippedSlot.CharacterID ?? _characterID,
                    new List<string> { inst.EquippedSlot.SlotID });
            }
            else
            {
                var def = ResolveDefinition(inst);
                string slot = ResolveEquipTargetSlot(inst, def, out bool allowed);
                if (!allowed || string.IsNullOrWhiteSpace(slot))
                {
                    if (equipButton != null) equipButton.interactable = true;
                    return;
                }

                result = await CharacterService.EquipItems(_characterID, new List<EquipSlotPair>
                {
                    new EquipSlotPair { SlotID = slot, ItemInstanceID = _instanceID }
                });
            }

            if (this == null || result == null) return;
            if (!result.Success) Refresh();
        }

        // ===== Level Up =====

        private void ApplyLevelUpButton(UnstackableItemInstanceState inst, ItemDefinition def)
        {
            if (levelUpButton == null) return;

            var upgrade = def?.Equipment?.Upgrade;
            int level = Mathf.Max(1, inst?.Level ?? 1);
            bool atMax = ItemUpgradeMath.IsMaxLevel(level, upgrade);

            long cost = atMax ? 0 : ItemUpgradeMath.ComputeNextLevelCost(upgrade, level);

            if (levelUpCostText != null)
                levelUpCostText.text = atMax ? "MAX" : (cost > 0 ? cost.ToString() : "Free");

            levelUpButton.interactable = !atMax;
        }

        private async void OnLevelUpClicked()
        {
            if (string.IsNullOrWhiteSpace(_instanceID)) return;

            if (levelUpButton != null) levelUpButton.interactable = false;

            bool success = await TryUpgradeItemLevel(_instanceID);

            if (this == null) return;
            if (!success) Refresh();
        }

        /// <summary>
        /// Stub for the not-yet-added V2 item-level-up endpoint. Replace the body with
        /// <c>var r = await ItemService.UpgradeItemLevel(itemInstanceID); return r.Success;</c>
        /// once <c>ItemAPI</c>/<c>ItemService</c>/<c>ItemModels</c> land.
        /// </summary>
        private static Task<bool> TryUpgradeItemLevel(string itemInstanceID)
        {
            Debug.Log($"[EquipmentInfoPopup] TODO: ItemService.UpgradeItemLevel('{itemInstanceID}') — endpoint not wired yet.");
            return Task.FromResult(false);
        }

        // ===== Helpers =====

        private static ItemDefinition ResolveDefinition(UnstackableItemInstanceState inst)
        {
            if (inst == null) return null;
            var catalogs = IDosGamesData.Config?.TitlePublicConfiguration?.Item?.Catalogs;
            if (catalogs == null) return null;

            if (!string.IsNullOrWhiteSpace(inst.CatalogID)
                && catalogs.TryGetValue(inst.CatalogID, out var catalog)
                && catalog?.Items != null
                && !string.IsNullOrWhiteSpace(inst.ItemID)
                && catalog.Items.TryGetValue(inst.ItemID, out var def))
            {
                return def;
            }

            foreach (var c in catalogs.Values)
            {
                if (c?.Items == null) continue;
                if (!string.IsNullOrWhiteSpace(inst.ItemID) && c.Items.TryGetValue(inst.ItemID, out var d))
                    return d;
            }
            return null;
        }

        private static string ResolveImagePath(ItemDefinition def)
        {
            if (def?.AssetPaths == null) return null;
            if (def.AssetPaths.TryGetValue("portrait", out var p) && !string.IsNullOrWhiteSpace(p)) return p;
            if (def.AssetPaths.TryGetValue("icon", out var i) && !string.IsNullOrWhiteSpace(i)) return i;
            foreach (var v in def.AssetPaths.Values)
                if (!string.IsNullOrWhiteSpace(v)) return v;
            return null;
        }

        private async Task LoadImage(string path)
        {
            if (string.IsNullOrEmpty(path) || itemImage == null) return;
            var sprite = await ImageLoader.GetSpriteAsync(path);
            if (sprite != null && this != null && itemImage != null && _loadedImagePath == path)
                itemImage.sprite = sprite;
        }

        // ===== Event handlers =====

        private void HandleItemsEquipped(string charID, List<EquipSlotPair> pairs)
        {
            for (int i = 0; i < pairs.Count; i++)
                if (pairs[i] != null && pairs[i].ItemInstanceID == _instanceID)
                {
                    Refresh();
                    return;
                }
        }

        private void HandleItemsUnequipped(string charID, List<string> slotIDs) => Refresh();
    }
}
