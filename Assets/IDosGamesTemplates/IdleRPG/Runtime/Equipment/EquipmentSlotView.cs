using System;
using UnityEngine;
using UnityEngine.UI;

namespace IDosGames
{
    /// <summary>
    /// One slot around the character in <see cref="EquipmentPanel"/>. The slot itself is a thin
    /// shell — the actual item rendering is delegated to a child <see cref="EquipmentItem"/>
    /// (filling the slot) bound in <see cref="EquipmentItemMode.SlotChild"/> mode.
    /// When the slot is empty the child <see cref="EquipmentItem"/> is simply disabled.
    /// </summary>
    public class EquipmentSlotView : MonoBehaviour
    {
        [Header("Identity")]
        [Tooltip("Slot ID — must match a key in the active character's CharacterEquipment.Slots.")]
        [SerializeField] private string slotID;

        [Header("Refs")]
        [SerializeField] private Button button;

        [Tooltip("Child EquipmentItem that fills the slot when an item is equipped. Disabled when empty.")]
        [SerializeField] private EquipmentItem equippedItemView;

        [Tooltip("Overlay shown when characterLevel < rule.MinCharacterLevel.")]
        [SerializeField] private GameObject lockedOverlay;

        private Action<string, string> _onClick;
        private string _instanceID;

        public string SlotID => slotID;

        private void Awake()
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(InvokeClick);
            }
        }

        public void Bind(
            EquippedItem equipped,
            UnstackableItemInstanceState inst,
            ItemDefinition def,
            bool isLocked,
            ItemRarityVisual rarity,
            Sprite classIconSprite,
            Action<string, string> onClick)
        {
            _onClick = onClick;
            _instanceID = inst?.ItemInstanceID;

            bool hasItem = inst != null && def != null;

            if (lockedOverlay != null) lockedOverlay.SetActive(isLocked);
            if (button != null) button.interactable = !isLocked;

            if (equippedItemView != null)
            {
                if (hasItem)
                {
                    equippedItemView.gameObject.SetActive(true);
                    equippedItemView.BindUnstackable(
                        inst, def,
                        EquipmentItemMode.SlotChild,
                        isEquipped: false, // badge is meaningless inside a slot
                        rarity,
                        classIconSprite,
                        onSelected: null); // clicks are handled by the slot's own button
                }
                else
                {
                    equippedItemView.Clear();
                    equippedItemView.gameObject.SetActive(false);
                }
            }
        }

        private void InvokeClick() => _onClick?.Invoke(slotID, _instanceID);
    }
}
