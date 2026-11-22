using UnityEngine;

namespace ForestAdventure
{
    // Simple prototype overlay. Replace with classmate's World Space Canvas/TMP later.
    [RequireComponent(typeof(EnemyHealth))]
    public class EnemyWorldLabel : MonoBehaviour
    {
        private EnemyHealth health;
        private GUIStyle style;
        private void Awake() { health = GetComponent<EnemyHealth>(); }
        private void OnGUI()
        {
            if (health == null || health.IsDead || Camera.main == null) return;
            Vector3 screen = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2.5f);
            if (screen.z <= 0) return;
            if (style == null)
            {
                style = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.MiddleCenter, fontSize = 14 };
                style.normal.textColor = Color.white;
            }
            GUI.Box(new Rect(screen.x - 55, Screen.height - screen.y - 16, 110, 29),
                "#" + health.EnemyId.ToString("00") + " HP " + health.CurrentHealth + "/" + health.maxHealth, style);
        }
    }
}
