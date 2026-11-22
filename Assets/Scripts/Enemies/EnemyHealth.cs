using System;
using UnityEngine;

namespace ForestAdventure
{
    // Normal animals: 40 HP, 10 damage / bullet -> 4 hits; 4 damage / punch -> 10 hits.
    public class EnemyHealth : MonoBehaviour
    {
        [Min(1)] public int maxHealth = 40;
        public int CurrentHealth { get; private set; }
        public bool IsDead { get; private set; }
        public int EnemyId { get; private set; }
        public event Action<EnemyHealth> Died;
        public event Action<EnemyHealth> Damaged;

        private void Awake() { CurrentHealth = maxHealth; }
        public void AssignId(int number) { EnemyId = number; }
        public void ResetHealth(int value) { maxHealth = Mathf.Max(1, value); CurrentHealth = maxHealth; IsDead = false; }
        public void TakeDamage(int amount)
        {
            if (IsDead || amount <= 0) return;
            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            Damaged?.Invoke(this);
            if (CurrentHealth == 0)
            {
                IsDead = true; // prevents duplicate kill/drop events
                Died?.Invoke(this);
            }
        }
    }
}
