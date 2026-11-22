using System;
using UnityEngine;

namespace ForestAdventure
{
    [Serializable]
    public class PlayerProgressData
    {
        public bool hasGun;
        public bool gunEquipped;
        public int magazine;
        public int reserveAmmo;
        public int gunDamage;
        public bool specialGunUnlocked;
        public bool specialGunEquipped;
        public int gems;
        public int kills;

        public void Capture(PlayerCombat c, GameManager gm)
        {
            if (c != null)
            {
                hasGun = c.hasGun;
                gunEquipped = c.gunEquipped;
                magazine = c.magazine;
                reserveAmmo = c.reserveAmmo;
                gunDamage = c.gunDamage;
                specialGunUnlocked = c.specialGunUnlocked;
                specialGunEquipped = c.specialGunEquipped;
            }
            if (gm != null) { gems = gm.Gems; kills = gm.Kills; }
        }
        public void Apply(PlayerCombat c)
        {
            if (c == null) return;
            c.hasGun = hasGun;
            c.gunEquipped = hasGun && gunEquipped;
            c.magazine = Mathf.Clamp(magazine, 0, c.magazineSize);
            c.reserveAmmo = Mathf.Max(0, reserveAmmo);
            c.gunDamage = Mathf.Max(1, gunDamage);
            c.specialGunUnlocked = specialGunUnlocked;
            c.specialGunEquipped = specialGunUnlocked && specialGunEquipped;
        }
    }
}
