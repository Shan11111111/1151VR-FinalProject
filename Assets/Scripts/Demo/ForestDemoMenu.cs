using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ForestAdventure
{
    /// <summary>Desktop/GameObject-only start/pause instructions. Remove for Cardboard UI.</summary>
    public class ForestDemoMenu : MonoBehaviour
    {
        private bool started;
        private bool paused;
        private PlayerController player;
        private GUIStyle titleStyle;
        private void Start()
        {
            // Bootstrap adds this component during its Start; defer first frame so Player exists.
            StartCoroutine(Initialize());
        }
        private System.Collections.IEnumerator Initialize()
        {
            yield return null;
            player = FindFirstObjectByType<PlayerController>();
            ShowCursorAndStop();
        }
        private void Update()
        {
            bool escape = false;
#if ENABLE_INPUT_SYSTEM
            escape = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
            escape |= Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame;
#else
            escape = Input.GetKeyDown(KeyCode.Escape);
#endif
            if (escape && started && GameManager.Instance != null &&
                GameManager.Instance.Phase != GamePhase.Lost && GameManager.Instance.Phase != GamePhase.Won)
            {
                if (paused) Resume(); else Pause();
            }
        }
        private void ShowCursorAndStop()
        {
            Time.timeScale = 0f;
            if (player != null) player.SetLocked(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        private void Begin()
        {
            started = true;
            paused = false;
            Resume();
            var wake = FindFirstObjectByType<DemoWakeSequence>();
            if (wake != null) wake.PlayWake();
        }
        private void Pause() { paused = true; ShowCursorAndStop(); }
        private void Resume()
        {
            paused = false;
            Time.timeScale = 1f;
            if (player == null) player = FindFirstObjectByType<PlayerController>();
            if (player != null) player.SetLocked(false);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        private void OnDestroy() { Time.timeScale = 1f; }
        private void OnGUI()
        {
            if (started && !paused) return;
            float w = 420, h = 260;
            Rect r = new Rect((Screen.width-w)/2f, (Screen.height-h)/2f, w, h);
            GUI.Box(r, "");
            GUI.Label(new Rect(r.x+30, r.y+20, w-60, 40), "FOREST ADVENTURE - PRIMITIVE DEMO");
            GUI.Label(new Rect(r.x+30, r.y+65, w-60, 100),
                "Level 1: Wake up, talk to guardian, pick up gun/ammo, portal\nLevel 2: 3 species x 10, kill 30 AND collect 30 gems\nUnlock special gun -> enter Boss portal -> defeat Bear in 2 min\nWASD+Mouse, E pick up, Left Mouse shoot, ESC pause");
            if (GUI.Button(new Rect(r.x+70, r.y+190, w-140, 45), started ? "RESUME" : "START DEMO"))
            {
                if (!started) Begin(); else Resume();
            }
        }
    }
}
