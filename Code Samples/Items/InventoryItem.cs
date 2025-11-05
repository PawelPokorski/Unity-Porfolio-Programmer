using UnityEngine;

namespace Items
{
    [System.Serializable]
    public class InventoryItem
    {
        public ItemData itemData;
        public int amount;

        public InventoryItem(ItemData itemData, int amount)
        {
            this.itemData = itemData;
            this.amount = Mathf.Clamp(amount, 0, itemData.maxAmount);
        }

        public bool IsStackFull() => amount >= itemData.maxAmount;
    }
}