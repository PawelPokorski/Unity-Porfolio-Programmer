using UnityEngine;

namespace Items
{
    public class Weapon : MonoBehaviour
    {
        public float damage;
        public float speed;
        public int durability;

        public void Attack()
        {
            // Look for enemies in range and apply damage
            // If an enemy is hit, reduce durability
            // If durability reaches zero, destroy the weapon
            // Apply cooldown based on speed

            Debug.Log("Attack");
            durability--;

            if (durability <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}