using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ForestAdventure
{
    /// <summary>
    /// DESKTOP PRESENTATION / TEST ONLY. Add alongside ForestDemoBootstrap.
    /// F1: complete exploration through public game APIs.
    /// F2: kill one actually spawned animal (real death event and real gem drop).
    /// F3: simulate remaining hunting kills / gem quota and open Boss stage.
    /// F4: defeat Boss (actual health/death event).
    /// F5: play Boss eating sequence and fail.
    /// Do not include this component in the production Cardboard scene.
    /// </summary>
    public class ForestThreeLevelQuickDemo : MonoBehaviour
    {
        [Header("Desktop demonstration only")]
        public bool showOnScreenControls = true;
        public bool enableHotkeys = true;
        private string status = "Ready. F1 explore, F2 real animal death, F3 boss, F4 victory, F5 eaten.";
        private GUIStyle statusStyle;

        private void Update()
        {
            if (!enableHotkeys) return;
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb.f1Key.wasPressedThisFrame) CompleteExplore();
            if (kb.f2Key.wasPressedThisFrame) KillOneRealAnimal();
            if (kb.f3Key.wasPressedThisFrame) FastForwardToBoss();
            if (kb.f4Key.wasPressedThisFrame) DefeatBoss();
            if (kb.f5Key.wasPressedThisFrame) DemonstrateEaten();
#else
            if (Input.GetKeyDown(KeyCode.F1)) CompleteExplore();
            if (Input.GetKeyDown(KeyCode.F2)) KillOneRealAnimal();
            if (Input.GetKeyDown(KeyCode.F3)) FastForwardToBoss();
            if (Input.GetKeyDown(KeyCode.F4)) DefeatBoss();
            if (Input.GetKeyDown(KeyCode.F5)) DemonstrateEaten();
#endif
        }

        private GameManager Manager()
        {
            var gm = GameManager.Instance;
            if (gm == null) status = "GameManager not ready. Check ForestDemoBootstrap.";
            return gm;
        }

        public void CompleteExplore()
        {
            var gm = Manager();
            if (gm == null) return;
            if (gm.Phase != GamePhase.Explore) { status = "F1 needs EXPLORE phase."; return; }
            gm.AcceptQuest();
            gm.CompleteExploration();
            status = "Level 1 demo complete -> HUNT. Wait for one animal to spawn.";
        }

        public void KillOneRealAnimal()
        {
            var gm = Manager();
            if (gm == null) return;
            if (gm.Phase != GamePhase.Hunting) { status = "F2 needs HUNT phase."; return; }
            EnemyHealth target = null;
            float closest = float.PositiveInfinity;
            var player = FindFirstObjectByType<PlayerController>();
            foreach (var enemy in FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None))
            {
                if (enemy == null || enemy.IsDead || enemy.GetComponent<BossController>() != null) continue;
                float d = player != null ? (enemy.transform.position - player.transform.position).sqrMagnitude : 0f;
                if (d < closest) { closest = d; target = enemy; }
            }
            if (target == null) { status = "No living animal yet. Wait 1-2 seconds and press F2 again."; return; }
            string animalName = target.name;
            target.TakeDamage(999999);
            status = animalName + " killed via real EnemyHealth. Look for green gem after death delay.";
        }

        public void FastForwardToBoss()
        {
            var gm = Manager();
            if (gm == null) return;
            if (gm.Phase == GamePhase.Explore) CompleteExplore();
            if (gm.Phase != GamePhase.Hunting) { status = "F3 needs EXPLORE or HUNT."; return; }
            // Shortcut ONLY: bypasses real kills and drops to demonstrate level transitions.
            int remaining = Mathf.Max(0, gm.requiredKills - gm.Kills);
            for (int i = 0; i < remaining; i++) gm.RegisterKill();
            int gemsMissing = Mathf.Max(0, gm.requiredCollectedGems - gm.GemsCollectedTotal);
            if (gemsMissing > 0) gm.AddGems(gemsMissing);
            bool opened = gm.TryUnlockBoss();
            status = opened ? "Level 2 SIMULATED completion -> BOSS. F4 victory / F5 eaten." : "Boss unlock failed; check manager state.";
        }

        public void DefeatBoss()
        {
            var gm = Manager();
            if (gm == null) return;
            if (gm.Phase != GamePhase.Boss) { status = "F4 needs BOSS phase."; return; }
            var boss = FindFirstObjectByType<BossController>();
            if (boss == null) { status = "BossController missing. Check Bootstrap Console."; return; }
            var hp = boss.GetComponent<EnemyHealth>();
            if (hp == null) { status = "Boss EnemyHealth missing."; return; }
            hp.TakeDamage(999999);
            status = "Boss defeated using real health/death event -> VICTORY.";
        }

        public void DemonstrateEaten()
        {
            var gm = Manager();
            if (gm == null) return;
            if (gm.Phase != GamePhase.Boss) { status = "F5 needs BOSS phase."; return; }
            var boss = FindFirstObjectByType<BossController>();
            if (boss == null) { status = "BossController missing."; return; }
            var sequence = boss.GetComponent<BossEatSequence>();
            if (sequence == null) sequence = boss.gameObject.AddComponent<BossEatSequence>();
            sequence.BeginEat(FindFirstObjectByType<PlayerController>());
            status = "Boss eat event triggered. Test fallback shows FAILED after ~2 seconds (no final animation yet).";
        }

        private void OnGUI()
        {
            if (!showOnScreenControls) return;
            if (statusStyle == null)
            {
                statusStyle = new GUIStyle(GUI.skin.box) { fontSize = 14, alignment = TextAnchor.MiddleLeft, wordWrap = true };
                statusStyle.normal.textColor = Color.white;
            }
            // Keep the demo toolbar below the existing HUD, so it does not cover it.
            float w = Mathf.Min(Screen.width - 20f, 690f);
            GUI.Box(new Rect(10f, 96f, w, 72f), "DEV DEMO ONLY — 1/2/3 stages | F1 Explore | F2 REAL animal death + gem | F3 SIMULATE hunting -> Boss | F4 Win | F5 Eaten", statusStyle);
            GUI.Box(new Rect(10f, 170f, w, 38f), status, statusStyle);
        }
    }
}
