using UnityEngine;

namespace ForestAdventure
{
    public enum PickupKind { Gem, Ammo, Gun }
    public class PickupItem : MonoBehaviour
    {
        public PickupKind kind;
        [Min(1)] public int amount = 1;
        public bool TryCollect(PlayerCombat player)
        {
            if (player == null) return false;
            switch (kind)
            {
                case PickupKind.Gem:
                    if (GameManager.Instance == null) return false;
                    GameManager.Instance.AddGems(amount);
                    if (AudioManager.Instance != null) AudioManager.Instance.GemCollected();
                    break;
                case PickupKind.Ammo: player.AddReserveAmmo(amount); break;
                case PickupKind.Gun: player.EquipGun(); break;
            }
            Destroy(gameObject);
            return true;
        }
    }
}
