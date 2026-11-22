using UnityEngine;

namespace ForestAdventure
{
    // OnGUI is intentionally only for the isolated test; classmates can replace this with Canvas/TMP.
    public class DemoHUD : MonoBehaviour
    {
        private GUIStyle style;
        private GUIStyle big;
        private void OnGUI()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            if (style == null)
            {
                style = new GUIStyle(GUI.skin.box) { fontSize = 19, alignment = TextAnchor.MiddleLeft, wordWrap = true };
                style.normal.textColor = Color.white;
                big = new GUIStyle(GUI.skin.box) { fontSize = 29, alignment = TextAnchor.MiddleCenter, wordWrap = true };
                big.normal.textColor = Color.yellow;
            }
            var pc = FindFirstObjectByType<PlayerCombat>();
            int mag = pc == null ? 0 : pc.magazine;
            int reserve = pc == null ? 0 : pc.reserveAmmo;
            string weapon = pc == null ? "none" : pc.ActiveWeaponName;
            string state = gm.Phase == GamePhase.Explore ? "EXPLORE" : gm.Phase == GamePhase.Hunting ? "HUNT" : gm.Phase == GamePhase.Boss ? "BOSS" : gm.Phase.ToString().ToUpper();
            string message = $"{state} | Time: {Mathf.CeilToInt(gm.TimeLeft)}s | Kills: {gm.Kills}/{gm.requiredKills} | Gems: {gm.Gems} | {weapon} | Ammo: {mag}/{reserve}";
            GUI.Box(new Rect(12, 10, Screen.width - 24, 42), message, style);
            if (gm.Phase == GamePhase.Explore)
                GUI.Box(new Rect(12, 55, Screen.width - 24, 34), gm.QuestAccepted ? "Find the blue objective pillar to begin hunting" : "Look at purple guardian and press E / Square", style);
            if (gm.Phase == GamePhase.Hunting && gm.HuntingComplete)
                GUI.Box(new Rect(12, 55, Screen.width - 24, 34), $"Pick up {gm.requiredCollectedGems} green gems (collected {gm.GemsCollectedTotal}), then find yellow altar", style);
            if (gm.Phase == GamePhase.Hunting && gm.BossPortalActive)
                GUI.Box(new Rect(12, 55, Screen.width - 24, 34), "Special gun unlocked! Go to purple Boss portal. Q switches guns.", style);
            if (gm.Phase == GamePhase.Won || gm.Phase == GamePhase.Lost)
            {
                string ending = gm.Phase == GamePhase.Won ? "VICTORY" : "FAILED (Boss ate you or time ran out)";
                GUI.Box(new Rect(Screen.width / 2f - 250, Screen.height / 2f - 75, 500, 90), ending, big);
                if (GUI.Button(new Rect(Screen.width / 2f - 100, Screen.height / 2f + 25, 200, 46), "Restart test"))
                    UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
            }
            if (gm.Phase == GamePhase.Explore || gm.Phase == GamePhase.Hunting || gm.Phase == GamePhase.Boss)
                GUI.Label(new Rect(Screen.width / 2f - 10, Screen.height / 2f - 12, 25, 25), "+", big);
            GUI.Box(new Rect(12, Screen.height - 65, Screen.width - 24, 55),
                "WASD / Mouse | Click/Space attack | E pickup / altar | Q gun/fists | R reload   (Gamepad: sticks / R2 / Square / Triangle / Circle)", style);
        }
    }
}
