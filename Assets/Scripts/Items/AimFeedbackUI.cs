using UnityEngine;

namespace ForestAdventure
{
    public class AimFeedbackUI : MonoBehaviour
    {
        [Header("References")]
        public Camera aimCamera;
        public PlayerCombat combat;

        [Header("Detection")]
        public float senseRange = 80f;
        public float pickupRange = 3f;

        [Header("UI")]
        public bool showDistance = true;

        private string statusText = "未瞄準目標";
        private Color statusColor = Color.gray;

        private string actionText = "";
        private float actionTimer = 0f;

        private AimState currentAimState = AimState.None;
        private float currentDistance = 0f;

        private enum AimState
        {
            None,
            EnemyInRange,
            EnemyTooFar,
            PickupInRange,
            PickupTooFar,
            Other
        }

        private void Reset()
        {
            combat = GetComponent<PlayerCombat>();

            if (aimCamera == null)
            {
                Camera cam = GetComponentInChildren<Camera>();
                if (cam != null) aimCamera = cam;
            }
        }

        private void Update()
        {
            UpdateAimState();
            UpdateAttackFeedback();

            if (actionTimer > 0f)
                actionTimer -= Time.deltaTime;
        }

        private void UpdateAimState()
        {
            if (aimCamera == null || combat == null)
            {
                currentAimState = AimState.None;
                statusText = "請指定 Camera / PlayerCombat";
                statusColor = Color.red;
                return;
            }

            Ray ray = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            if (!Physics.Raycast(ray, out RaycastHit hit, senseRange))
            {
                currentAimState = AimState.None;
                currentDistance = 0f;
                statusText = "沒對到目標";
                statusColor = Color.gray;
                return;
            }

            currentDistance = hit.distance;

            EnemyHealth enemy = hit.collider.GetComponentInParent<EnemyHealth>();
            if (enemy != null)
            {
                float attackRange = GetCurrentAttackRange();

                if (hit.distance <= attackRange)
                {
                    currentAimState = AimState.EnemyInRange;
                    statusText = showDistance
                        ? $"可攻擊　距離 {hit.distance:0.0} / {attackRange:0.0}"
                        : "可攻擊";
                    statusColor = Color.green;
                }
                else
                {
                    currentAimState = AimState.EnemyTooFar;
                    statusText = showDistance
                        ? $"太遠了　距離 {hit.distance:0.0} / {attackRange:0.0}"
                        : "太遠了";
                    statusColor = new Color(1f, 0.65f, 0f);
                }

                return;
            }

            PickupItem pickup = hit.collider.GetComponentInParent<PickupItem>();
            if (pickup != null)
            {
                if (hit.distance <= pickupRange)
                {
                    currentAimState = AimState.PickupInRange;
                    statusText = showDistance
                        ? $"可拾取：{pickup.kind}　距離 {hit.distance:0.0}"
                        : $"可拾取：{pickup.kind}";
                    statusColor = Color.cyan;
                }
                else
                {
                    currentAimState = AimState.PickupTooFar;
                    statusText = showDistance
                        ? $"物品太遠　距離 {hit.distance:0.0} / {pickupRange:0.0}"
                        : "物品太遠";
                    statusColor = new Color(1f, 0.65f, 0f);
                }

                return;
            }

            currentAimState = AimState.Other;
            statusText = "有對到東西，但不是敵人";
            statusColor = Color.white;
        }

        private void UpdateAttackFeedback()
        {
            bool attackPressed =
                Input.GetMouseButtonDown(0) ||
                Input.GetKeyDown(KeyCode.Space);

            if (!attackPressed) return;

            switch (currentAimState)
            {
                case AimState.EnemyInRange:
                    actionText = "命中範圍內";
                    break;

                case AimState.EnemyTooFar:
                    actionText = "太遠，沒打到";
                    break;

                case AimState.None:
                    actionText = "沒打到";
                    break;

                default:
                    actionText = "沒有打到敵人";
                    break;
            }

            actionTimer = 0.8f;
        }

        private float GetCurrentAttackRange()
        {
            // 如果你 PlayerCombat 的欄位名稱不同，就改成你自己的名稱
            if (combat.hasGun && combat.gunEquipped)
                return combat.shootRange;

            return combat.meleeRange;
        }

        private void OnGUI()
        {
            DrawCrosshair();
            DrawStatusBox();
            DrawActionText();
        }

        private void DrawCrosshair()
        {
            float cx = Screen.width * 0.5f;
            float cy = Screen.height * 0.5f;

            GUI.color = statusColor;

            GUI.Box(new Rect(cx - 1f, cy - 10f, 2f, 20f), "");
            GUI.Box(new Rect(cx - 10f, cy - 1f, 20f, 2f), "");

            GUI.color = Color.white;
        }

        private void DrawStatusBox()
        {
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 18;
            style.normal.textColor = Color.white;

            float w = 320f;
            float h = 32f;
            float x = (Screen.width - w) * 0.5f;
            float y = Screen.height * 0.72f;

            Color old = GUI.color;
            GUI.color = new Color(statusColor.r, statusColor.g, statusColor.b, 0.85f);
            GUI.Box(new Rect(x, y, w, h), statusText, style);
            GUI.color = old;
        }

        private void DrawActionText()
        {
            if (actionTimer <= 0f || string.IsNullOrEmpty(actionText)) return;

            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 20;
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = Color.yellow;

            float w = 260f;
            float h = 30f;
            float x = (Screen.width - w) * 0.5f;
            float y = Screen.height * 0.78f;

            GUI.Label(new Rect(x, y, w, h), actionText, style);
        }
    }
}