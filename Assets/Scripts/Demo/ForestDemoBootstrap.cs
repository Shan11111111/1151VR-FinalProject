using UnityEngine;

namespace ForestAdventure
{
    // Place on ONE empty GameObject in a blank scene. Builds a playable primitive prototype on Play.
    public class ForestDemoBootstrap : MonoBehaviour
    {
        [Header("Level rules (Inspector)")]
        [Min(1)] public int animalsPerSpecies = 10;
        [Min(10)] public float level1Seconds = 180f;
        [Min(10)] public float level2Seconds = 240f;
        [Min(10)] public float level3Seconds = 120f;
        [Min(1)] public int animalHP = 4;
        [Min(1)] public int fistDamage = 1;
        [Min(1)] public int basicGunDamage = 4;
        [Min(1)] public int specialGunDamage = 12;
        [Min(1)] public int bossHP = 120;
        [Min(0)] public int startingReserveAmmo = 12;
        [Min(0)] public int ammoPerPickup = 12;
        [Min(1)] public int maxAliveAnimals = 5;
        [Min(0.1f)] public float animalSpawnInterval = 1.2f;
        public int demoTotalAnimals => animalsPerSpecies * 3;
        private GameObject bossObject;
        private GameObject bossPortalObject;
        private Transform player;
        private GameManager gameManager;
        private void Start()
        {
            if (FindFirstObjectByType<GameManager>() != null)
            {
                Debug.LogWarning("Demo bootstrap needs an otherwise empty scene with no preexisting GameManager.");
                return;
            }
            var gmObject = new GameObject("GameManager");
            gameManager = gmObject.AddComponent<GameManager>();
            gameManager.requiredKills = demoTotalAnimals;
            gameManager.explorationSeconds = level1Seconds;
            gameManager.huntingSeconds = level2Seconds;
            gameManager.bossSeconds = level3Seconds;
            gameManager.requiredCollectedGems = demoTotalAnimals;
            gameManager.ResetRun(); // apply configured exploration timer before the first frame
            gmObject.AddComponent<DemoHUD>();
            gmObject.AddComponent<AudioManager>();
            gameManager.PhaseChanged += OnPhaseChanged;
            gmObject.AddComponent<ForestDemoMenu>();

            CreateBox("Ground", new Vector3(0, -0.55f, 0), new Vector3(85, 1, 85), new Color(0.29f, 0.48f, 0.30f));
            for (int i = 0; i < 16; i++)
            {
                float rad = i * Mathf.PI * 2f / 16f;
                Vector3 p = new Vector3(Mathf.Sin(rad) * 31, 2.5f, Mathf.Cos(rad) * 31);
                CreateBox("ForestTree_" + i, p, new Vector3(2f, 5f, 2f), new Color(0.19f, 0.27f, 0.14f));
            }
            var pObj = new GameObject("Player");
            pObj.transform.position = new Vector3(0, 0.1f, -12);
            var cc = pObj.AddComponent<CharacterController>();
            cc.height = 1.8f; cc.center = new Vector3(0, 0.9f, 0);
            var cameraObj = new GameObject("Main Camera");
            cameraObj.tag = "MainCamera";
            cameraObj.transform.SetParent(pObj.transform, false);
            cameraObj.transform.localPosition = new Vector3(0, 1.6f, 0);
            var cam = cameraObj.AddComponent<Camera>();
            cameraObj.AddComponent<AudioListener>();
            var controller = pObj.AddComponent<PlayerController>();
            controller.playerCamera = cam;
            var combat = pObj.AddComponent<PlayerCombat>();
            combat.aimCamera = cam;
            combat.hasGun = false; combat.gunEquipped = false;
            combat.punchDamage = fistDamage; combat.gunDamage = basicGunDamage;
            combat.specialGunDamage = specialGunDamage;
            combat.reserveAmmo = startingReserveAmmo; // Level 1 starts unarmed
            player = pObj.transform;
            pObj.AddComponent<DemoWakeSequence>();
            var npc = CreateBox("Forest Guardian - interact", new Vector3(2, 1, -14), new Vector3(1.5f, 2, 1.5f), Color.magenta);
            npc.AddComponent<ForestGuardianNPC>();
            var objective = CreateBox("Level1 Portal (Blue)", new Vector3(0, 2, -3), new Vector3(3, 4, 2), Color.blue);
            objective.GetComponent<Collider>().isTrigger = true;
            objective.AddComponent<ExplorationGoal>();
            var spawn = new GameObject("EnemySpawner");
            spawn.transform.position = Vector3.zero;
            var spawner = spawn.AddComponent<EnemySpawner>();
            spawner.maxAlive = maxAliveAnimals;
            spawner.animalHP = animalHP;
            spawner.counts = new int[] { animalsPerSpecies, animalsPerSpecies, animalsPerSpecies };
            gameManager.requiredKills = spawner.TotalCount();
            spawner.spawnInterval = animalSpawnInterval;
            spawner.spawnPoints = new Transform[3];
            for (int i = 0; i < 3; i++)
            {
                var point = new GameObject("SpawnPoint_0" + (i+1));
                point.transform.SetParent(spawn.transform);
                point.transform.position = new Vector3(-17 + i * 17, 1, 15);
                spawner.spawnPoints[i] = point.transform;
            }
            var gun = CreateBox("Gun Pickup (Yellow Small Box)", new Vector3(0, 0.4f, -9), new Vector3(0.9f, .5f, .45f), new Color(.95f,.83f,.28f));
            gun.AddComponent<PickupItem>().kind = PickupKind.Gun;
            for (int i = 0; i < 13; i++)
            {
                float x = Random.Range(-23f, 23f); float z = Random.Range(-16f, 22f);
                if (Mathf.Abs(x) < 4 && Mathf.Abs(z) < 4) continue;
                var ammo = CreateBox("Ammo", new Vector3(x, 0.4f, z), Vector3.one * .7f, Color.cyan).AddComponent<PickupItem>();
                ammo.kind = PickupKind.Ammo; ammo.amount = ammoPerPickup;
            }
            var altar = CreateBox("WeaponUnlockAltar (Yellow)", new Vector3(22, 1, -9), new Vector3(2, 2, 2), Color.yellow);
            altar.AddComponent<WeaponUnlockAltar>();
            var bossPortal = CreateBox("Boss Portal (Locked until special gun)", new Vector3(27, 2, -9), new Vector3(2.2f, 4f, 2.2f), new Color(.3f, .15f, .9f));
            bossPortal.GetComponent<Collider>().isTrigger = true;
            bossPortal.AddComponent<BossPortal>();
            bossPortalObject = bossPortal;
            bossPortalObject.SetActive(false);
            bossObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            bossObject.name = "ForestBoss";
            bossObject.transform.position = new Vector3(0, 2, 20);
            bossObject.transform.localScale = new Vector3(3f, 2f, 3f);
            bossObject.GetComponent<Renderer>().material.color = Color.red;
            bossObject.SetActive(false);
            Debug.Log("Demo ready: speak to purple guardian, touch blue goal, defeat animals, collect green gems, interact with yellow altar, beat boss.");
        }
        private void Update()
        {
            if (bossPortalObject != null && gameManager != null)
            {
                bool visible = gameManager.BossPortalActive;
                if (bossPortalObject.activeSelf != visible) bossPortalObject.SetActive(visible);
            }
        }
        private static GameObject CreateBox(string name, Vector3 pos, Vector3 scale, Color color)
        {
            GameObject o = GameObject.CreatePrimitive(PrimitiveType.Cube);
            o.name = name; o.transform.position = pos; o.transform.localScale = scale;
            o.GetComponent<Renderer>().material.color = color;
            return o;
        }
        private void OnPhaseChanged(GamePhase phase)
        {
            if (phase == GamePhase.Boss)
            {
                var spawner = FindFirstObjectByType<EnemySpawner>();
                if (spawner != null) spawner.enabled = false;
                foreach (var enemy in FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None))
                    if (enemy != null && enemy.gameObject != bossObject) Destroy(enemy.gameObject);
                bossObject.SetActive(true);
                var hp = bossObject.AddComponent<EnemyHealth>();
                hp.maxHealth = bossHP;
                hp.ResetHealth(bossHP);
                var boss = bossObject.AddComponent<BossController>();
                boss.player = player;
                player.position = new Vector3(0, .1f, -8);
                var controller = player.GetComponent<PlayerController>();
                if (controller != null) controller.SetLocked(false);
            }
            else if (phase == GamePhase.Lost || phase == GamePhase.Won)
            {
                var controller = player != null ? player.GetComponent<PlayerController>() : null;
                if (controller != null) controller.SetLocked(true);
            }
        }
        private void OnDestroy()
        {
            if (gameManager != null) gameManager.PhaseChanged -= OnPhaseChanged;
        }
    }
}
