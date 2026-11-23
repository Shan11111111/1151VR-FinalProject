
using UnityEngine;

namespace ForestAdventure
{
    public enum PickupKind { Gem, Ammo, Gun }

    public class PickupItem : MonoBehaviour
    {
        public PickupKind kind;

        [Min(1)]
        public int amount = 1;

        public bool TryCollect(PlayerCombat player)
        {
            if (player == null) return false;

            switch (kind)
            {
                case PickupKind.Gem:
                    if (GameManager.Instance == null)
                        return false;

                    // 拾取前的寶石數量
                    int before = GameManager.Instance.Gems;

                    // 增加寶石
                    GameManager.Instance.AddGems(amount);

                    // 拾取後的寶石數量
                    int after = GameManager.Instance.Gems;

                    Debug.Log(
                        "[寶石拾取] Gems: " + before + " → " + after +
                        " | Phase: " + GameManager.Instance.Phase
                    );

                    if (AudioManager.Instance != null)
                        AudioManager.Instance.GemCollected();

                    break;

                case PickupKind.Ammo:
                    player.AddReserveAmmo(amount);
                    Debug.Log("[子彈拾取] 增加：" + amount);
                    break;

                case PickupKind.Gun:
                    player.EquipGun();
                    Debug.Log("[槍枝拾取] 已取得普通槍");
                    break;
            }

            Destroy(gameObject);
            return true;
        }
    }
}
