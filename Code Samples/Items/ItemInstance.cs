using Singletons.Player;
using Singletons.Interface;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System.Collections.Generic;

namespace Items
{
    public class ItemInstance : InteractableObject
    {
        [Header("Item Data")]
        [SerializeField] private ItemData itemData;
        [SerializeField] private int amount;

        [Header("Interactions")]
        [SerializeField] private bool _isInteractable = true;
        public override bool IsInteractable() => _isInteractable;
        public override UnityEvent OnObjectFocus { get; set; } = new();
        public override UnityEvent OnObjectUnfocus { get; set; } = new();
        public override UnityEvent<InteractionType> OnObjectInteract { get; set; } = new();
        private readonly HashSet<InteractionType> _availableInteractions = new();

        [Header("Highlighting")]
        [SerializeField] private bool _isHighlightable = true;
        public override bool IsHighlightable() => _isHighlightable;
        public override UnityEvent<bool> OnObjectHighlight { get; set; } = new();



        #region Unity Methods

        protected override void Awake()
        {
            base.Awake();

            OutlineColor = itemData.GetItemTypeColor();
        }

        protected override void OnEnable()
        {
            OnObjectFocus.AddListener(ShowItemTooltip);
            OnObjectUnfocus.AddListener(HideItemTooltip);
            OnObjectInteract.AddListener(PerformInteraction);

            base.OnEnable();
        }


        #endregion

        #region Interactable Methods

        /// <summary>
        /// Determines the available interactions for the current item based on its type and the player's state.
        /// </summary>
        private void CheckAvailableInteractions()
        {
            _availableInteractions.Clear();

            if (itemData.type == ItemData.Type.Weapon && PlayerCombat.Instance.HasWeaponAttached)
            {
                _availableInteractions.Add(InteractionType.Secondary);
            }
            else if (itemData.type == ItemData.Type.Weapon)
            {
                _availableInteractions.Add(InteractionType.Primary);
            }

            if (InventoryHandler.Instance.CanAddItem(itemData, amount))
            {
                _availableInteractions.Add(InteractionType.Primary);
            }
        }

        /// <summary>
        /// Displays the tooltip for the current item based on its available interactions.
        /// </summary>
        private void ShowItemTooltip()
        {
            CheckAvailableInteractions();

            if (_availableInteractions.Count == 0)
            {
                HudController.Instance.ShowItemTooltip(itemData, transform.position, false, false);
            }
            else if (_availableInteractions.Count == 1)
            {
                if (_availableInteractions.First() == InteractionType.Primary)
                {
                    HudController.Instance.ShowItemTooltip(itemData, transform.position, true, false);
                }
                else
                {
                    HudController.Instance.ShowItemTooltip(itemData, transform.position, false, true);
                }
            }
            else
            {
                HudController.Instance.ShowItemTooltip(itemData, transform.position, true, true);
            }
        }

        /// <summary>
        /// Hides the currently displayed item tooltip, if one is visible.
        /// </summary>
        private static void HideItemTooltip()
        {
            HudController.Instance.HideItemTooltip();
        }

        private void PerformInteraction(InteractionType interactionType)
        {
            switch (interactionType)
            {
                case InteractionType.Primary:
                    PickupItem();
                    break;
                case InteractionType.Secondary:
                    ReplaceItem();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Picks up the item, adds it to the player's inventory, and updates the item's state accordingly.
        /// </summary>
        public void PickupItem()
        {
            if (itemData.type == ItemData.Type.Weapon && TryGetComponent(out Weapon weapon) && !PlayerCombat.Instance.HasWeaponAttached)
            {
                OnObjectHighlight?.Invoke(false);
                OnObjectUnfocus?.Invoke();
                PlayerCombat.Instance.OnWeaponAttach?.Invoke(weapon);
            }
            else if (itemData.type != ItemData.Type.Weapon && InventoryHandler.Instance.AddItem(itemData, ref amount))
            {
                OnObjectHighlight?.Invoke(false);
                OnObjectUnfocus?.Invoke();
                Destroy(gameObject);
            }
        }

        public void ReplaceItem()
        {
            if (itemData.type == ItemData.Type.Weapon && TryGetComponent(out Weapon weapon))
            {
                OnObjectUnfocus?.Invoke();
                PlayerCombat.Instance.OnWeaponAttach?.Invoke(weapon);
            }
        }

        #endregion

        
    }
}