using UnityEngine;

namespace Items
{
    [CreateAssetMenu(fileName = "NewItemData", menuName = "Items/New Item Data")]
    public class ItemData : ScriptableObject
    {
        public new string name;
        public Sprite icon;
        [TextArea] public string description;
        public int maxAmount = int.MaxValue;
        public Type type;
        public Rarity rarity;

        public string GetItemType()
        {
            if (type == Type.None)
                return string.Empty;

            return type.ToString().Replace("_", " ");
        }

        public string GetItemRarity()
        {
            if (rarity == Rarity.None)
                return string.Empty;

            return rarity.ToString().Replace("_", " ");
        }

        public Color GetItemRarityColor()
        {
            return rarity switch
            {
                Rarity.Uncommon => Color.yellowGreen,
                Rarity.Rare => Color.cornflowerBlue,
                Rarity.Epic => Color.violet,
                Rarity.Legendary => Color.orange,
                _ => Color.gray8
            };
        }

        public Color GetItemTypeColor()
        {
            return type switch
            {
                Type.Weapon => GetItemRarityColor(),
                Type.Quest_Item => Color.red,
                _ => Color.gray8
            };
        }

        public enum Type { None, Consumable, Weapon, Resource, Misc, Quest_Item, Collectable, Key }
        public enum Rarity { None, Common, Uncommon, Rare, Epic, Legendary }
    }
}