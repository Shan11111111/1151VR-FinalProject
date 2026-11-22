using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ForestAdventure
{
    public class PlayerCombat : MonoBehaviour
    {
        public Camera aimCamera;
        public bool hasGun = false; // first level starts barehanded; demo bootstrap may override for quick tests
        public bool gunEquipped = false;
        public int magazineSize = 8;
        public int magazine = 8;
        public int reserveAmmo = 40;
        [Min(1)] public int gunDamage = 4;
        [Min(1)] public int punchDamage = 1;
        [Min(1)] public int specialGunDamage = 12;
        [Min(0)] public int specialUnlockAmmo = 80;
        public bool specialGunUnlocked;
        public bool specialGunEquipped;
        public string ActiveWeaponName => !hasGun || !gunEquipped ? "Fists" : specialGunEquipped ? "Special Gun" : "Basic Gun";
        public void UnlockSpecialGun()
        {
            specialGunUnlocked = true;
            hasGun = true;
            gunEquipped = true;
            specialGunEquipped = true;
        }
        public void SwitchWeapon()
        {
            if (!hasGun) return;
            if (!specialGunUnlocked) { gunEquipped = !gunEquipped; return; }
            if (!gunEquipped) { gunEquipped = true; specialGunEquipped = false; }
            else if (!specialGunEquipped) specialGunEquipped = true;
            else { gunEquipped = false; specialGunEquipped = false; }
        }
        public float shootRange = 75f;
        public float meleeRange = 2.6f;
        public float attackCooldown = 0.35f;
        private float nextAttack;
        private PlayerController movement;
        private void Awake() { movement = GetComponent<PlayerController>(); }
        private void Start() { if (aimCamera == null) aimCamera = GetComponentInChildren<Camera>(); }
        private void Update()
        {
            if (movement != null && movement.Locked) return;
            var gm = GameManager.Instance;
            if (gm == null || (gm.Phase != GamePhase.Explore && gm.Phase != GamePhase.Hunting && gm.Phase != GamePhase.Boss)) return;
            bool attack = false, interact = false, swap = false, reload = false;
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null) attack |= Mouse.current.leftButton.wasPressedThisFrame;
            if (Keyboard.current != null)
            {
                attack |= Keyboard.current.spaceKey.wasPressedThisFrame;
                interact |= Keyboard.current.eKey.wasPressedThisFrame;
                swap |= Keyboard.current.qKey.wasPressedThisFrame;
                reload |= Keyboard.current.rKey.wasPressedThisFrame;
            }
            if (Gamepad.current != null)
            {
                attack |= Gamepad.current.rightTrigger.wasPressedThisFrame;
                interact |= Gamepad.current.buttonWest.wasPressedThisFrame; // square on PS layout
                swap |= Gamepad.current.buttonNorth.wasPressedThisFrame;
                reload |= Gamepad.current.buttonEast.wasPressedThisFrame;
            }
#else
            attack = Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space);
            interact = Input.GetKeyDown(KeyCode.E);
            swap = Input.GetKeyDown(KeyCode.Q);
            reload = Input.GetKeyDown(KeyCode.R);
#endif
            if (swap) SwitchWeapon();
            if (reload) Reload();
            if (interact) Interact();
            if (attack && Time.time >= nextAttack)
            {
                nextAttack = Time.time + attackCooldown;
                Attack();
            }
        }
        public void AddReserveAmmo(int amount) { reserveAmmo += Mathf.Max(0, amount); }
        public void EquipGun() { hasGun = true; gunEquipped = true; specialGunEquipped = false; }
        private void Reload()
        {
            if (!hasGun || reserveAmmo <= 0) return;
            int needed = magazineSize - magazine;
            int added = Mathf.Min(needed, reserveAmmo);
            magazine += added;
            reserveAmmo -= added;
        }
        private void Attack()
        {
            if (aimCamera == null) return;
            bool gun = hasGun && gunEquipped;
            if (gun)
            {
                if (magazine <= 0) { Reload(); return; }
                magazine--;
                if (AudioManager.Instance != null) AudioManager.Instance.Shoot();
            }
            if (!gun && AudioManager.Instance != null) AudioManager.Instance.Punch();
            float range = gun ? shootRange : meleeRange;
            RaycastHit hit;
            if (!Physics.Raycast(aimCamera.transform.position, aimCamera.transform.forward, out hit, range, ~0, QueryTriggerInteraction.Ignore)) return;
            var enemy = hit.collider.GetComponentInParent<EnemyHealth>();
            if (enemy != null)
            {
                // The bear boss is only vulnerable to the newly unlocked weapon.
                if (enemy.GetComponent<BossController>() != null && !specialGunEquipped) return;
                enemy.TakeDamage(gun ? (specialGunEquipped ? specialGunDamage : gunDamage) : punchDamage);
            }
        }
        private void Interact()
        {
            if (aimCamera == null) return;
            RaycastHit hit;
            if (!Physics.Raycast(aimCamera.transform.position, aimCamera.transform.forward, out hit, 4f, ~0, QueryTriggerInteraction.Ignore)) return;
            var pickup = hit.collider.GetComponentInParent<PickupItem>();
            if (pickup != null) { pickup.TryCollect(this); return; }
            var npc = hit.collider.GetComponentInParent<ForestGuardianNPC>();
            if (npc != null) { npc.Interact(); return; }
            var altar = hit.collider.GetComponentInParent<WeaponUnlockAltar>();
            if (altar != null) { altar.TryUse(); return; }
            var portal = hit.collider.GetComponentInParent<BossPortal>();
            if (portal != null) portal.TryEnter();
        }
    }
}
