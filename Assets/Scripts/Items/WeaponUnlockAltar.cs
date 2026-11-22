using UnityEngine;
namespace ForestAdventure
{
    public class WeaponUnlockAltar : MonoBehaviour
    {
        public void TryUse()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            if (!gm.TryUnlockSpecialGun())
                Debug.Log("Need all animals defeated AND all required green gems picked up.");
            else Debug.Log("Special gun unlocked! Switch weapons with Q/Triangle. Boss portal is now active.");
        }
    }
}
