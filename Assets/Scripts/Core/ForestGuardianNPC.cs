using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
namespace ForestAdventure
{
    public class ForestGuardianNPC : MonoBehaviour
    {
        [TextArea(2, 4)] public string[] dialogueLines = {
            "你終於醒來了。歡迎來到這片森林。",
            "沿路探索並留意地上的槍枝與子彈。沒有槍也能使用拳頭。",
            "走到前方的傳送門，開始第二關的狩獵挑戰。"
        };
        private bool talking;
        private int index;
        private int beganFrame = -1;
        public bool IsTalking => talking;
        public void Interact()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.Phase != GamePhase.Explore || gm.QuestAccepted) return;
            if (!talking) { talking = true; index = 0; beganFrame = Time.frameCount; }
            else NextLine();
        }
        private void Update()
        {
            if (!talking || beganFrame == Time.frameCount) return;
            bool next = false;
#if ENABLE_INPUT_SYSTEM
            next = Keyboard.current != null && (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame);
            next |= Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame;
#else
            next = Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return);
#endif
            if (next) NextLine();
        }
        public void NextLine()
        {
            if (!talking) return;
            index++;
            if (index >= (dialogueLines == null ? 0 : dialogueLines.Length))
            {
                talking = false;
                if (GameManager.Instance != null) GameManager.Instance.AcceptQuest();
            }
        }
        private void OnGUI()
        {
            if (!talking || dialogueLines == null || index >= dialogueLines.Length) return;
            float w = Mathf.Min(Screen.width - 30f, 700f);
            Rect rect = new Rect((Screen.width-w)/2f, Screen.height-185f, w, 110f);
            GUI.Box(rect, "森林守護者  (E / □ 下一句)");
            GUI.Label(new Rect(rect.x+24,rect.y+38,w-48,45),dialogueLines[index]);
            if (GUI.Button(new Rect(rect.x+w-95,rect.y+77,80,28),"下一句")) NextLine();
        }
    }
}
