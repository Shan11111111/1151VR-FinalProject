using UnityEngine;
using UnityEngine.SceneManagement;

namespace ForestAdventure
{
    // Attach to a persistent manager object only in the *formal* multi-scene build.
    // Do NOT add to the ForestDemoBootstrap test scene.
    [RequireComponent(typeof(GameManager))]
    public class SceneFlowManager : MonoBehaviour
    {
        public string exploreScene = "Level1_Forest";
        public string huntingScene = "Level2_Hunting";
        public string bossScene = "Level3_Boss";
        public string resultScene = "Result";
        public bool automaticallyLoadPhaseScenes = true;
        private GameManager manager;
        private void Awake() { manager = GetComponent<GameManager>(); }
        private void OnEnable()
        {
            if (manager == null) manager = GetComponent<GameManager>();
            manager.PhaseChanged += OnPhaseChanged;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        private void OnDisable()
        {
            if (manager != null) manager.PhaseChanged -= OnPhaseChanged;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        private void OnPhaseChanged(GamePhase phase)
        {
            if (!automaticallyLoadPhaseScenes) return;
            string scene = phase == GamePhase.Explore ? exploreScene : phase == GamePhase.Hunting ? huntingScene :
                           phase == GamePhase.Boss ? bossScene : resultScene;
            LoadSafe(scene);
        }
        public void RestartCurrentLevel()
        {
            if (manager == null) return;
            manager.RestartPhase();
            string scene = manager.Phase == GamePhase.Explore ? exploreScene :
                           manager.Phase == GamePhase.Hunting ? huntingScene : bossScene;
            LoadSafe(scene);
        }
        public void RestartEntireGame()
        {
            if (manager == null) return;
            manager.ResetRun();
            LoadSafe(exploreScene);
        }
        private static void LoadSafe(string scene)
        {
            if (string.IsNullOrWhiteSpace(scene) || !Application.CanStreamedLevelBeLoaded(scene))
            {
                Debug.LogError("SceneFlowManager: scene not in Build Profiles Scene List: " + scene);
                return;
            }
            if (SceneManager.GetActiveScene().name != scene) SceneManager.LoadScene(scene);
        }
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Newly instantiated level player receives the persistent progress.
            if (manager == null) return;
            var combat = FindFirstObjectByType<PlayerCombat>();
            if (combat != null) manager.Progress.Apply(combat);
        }
    }
}
