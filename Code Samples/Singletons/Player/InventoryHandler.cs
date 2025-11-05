using Items;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Singletons.Player
{
    public class InventoryHandler : Singleton<InventoryHandler>
    {
        [Header("Stats")]
        [SerializeField] private int _maxSlots = 5;

        private readonly List<InventoryItem> _items = new();

        public bool AddItem(ItemData itemData, ref int amount)
        {
            if (itemData == null || amount <= 0)
                return false;

            int remaining = amount;

            foreach (var item in _items.Where(i => i.itemData == itemData && !i.IsStackFull()))
            {
                int space = item.itemData.maxAmount - item.amount;
                int toAdd = Mathf.Min(space, remaining);

                item.amount += toAdd;
                remaining -= toAdd;

                if (remaining <= 0)
                {
                    amount = 0;
                    return true;
                }
            }

            while (remaining > 0 && _items.Count < _maxSlots)
            {
                int toAdd = Mathf.Min(itemData.maxAmount, remaining);
                _items.Add(new InventoryItem(itemData, toAdd));
                remaining -= toAdd;
            }

            amount = remaining;
            return remaining == 0;
        }

        public void RemoveStack(InventoryItem item)
        {
            if (item == null || !_items.Contains(item))
                return;

            _items.Remove(item);
        }

        public void RemoveItem(ItemData itemData, int amount = 1)
        {
            if (itemData == null || amount <= 0)
                return;

            foreach (var item in _items.Where(i => i.itemData == itemData).ToList())
            {
                if (amount <= 0)
                    return;

                if (item.amount <= amount)
                {
                    amount -= item.amount;
                    _items.Remove(item);
                }
                else
                {
                    item.amount -= amount;
                    return;
                }
            }
        }

        public bool CanAddItem(ItemData itemData, int amount = 1)
        {
            if (itemData == null || amount <= 0)
                return false;

            int remaining = amount;

            foreach (var item in _items.Where(i => i.itemData == itemData && !i.IsStackFull()))
            {
                int space = itemData.maxAmount - item.amount;
                int toAdd = Mathf.Min(space, remaining);
                remaining -= toAdd;

                if (toAdd > 0)
                    return true;
            }

            int freeSlots = _maxSlots - _items.Count;
            int neededStacks = Mathf.CeilToInt((float)remaining / itemData.maxAmount);

            return freeSlots >= neededStacks;
        }

        public List<InventoryItem> GetAllItems() => _items;

        public int GetEmptySlots() => _maxSlots - _items.Count;

        public bool IsInventoryFull => _items.Count >= _maxSlots;
    }
}