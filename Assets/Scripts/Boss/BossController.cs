
using System.Collections;
using UnityEngine;

namespace ForestAdventure
{
    public enum BossState
    {
        Idle, Chase, Prepare, Charge, Recover, Dead
    }

    [RequireComponent(typeof(EnemyHealth))]
    public class BossController : MonoBehaviour
    {
        [Header("Player")]
        public Transform player;

        [Header("Movement")]
        public float chaseSpeed = 3.7f;
        public float chargeSpeed = 13f;

        [Header("Attack")]
        public float prepareSeconds = 1.6f;
        public float chargeSeconds = 1.3f;
        public float recoverSeconds = 1.3f;
        public float attackDistance = 9f;
        public float hitDistance = 1.5f;

        [Header("Enrage")]
        public float enragedHealthRatio = 0.35f;

        [Header("Boss Arena")]
        public bool limitToArena = true;
        public Vector3 arenaCenter = Vector3.zero;
        [Min(2f)] public float arenaRadius = 35f;

        [Header("Eat Sequence")]
        [Min(0.5f)] public float failSafeSeconds = 2.5f;

        public BossState State { get; private set; } = BossState.Idle;
        public bool Enraged { get; private set; }

        private EnemyHealth health;
        private Collider bossCollider;
        private float stateTimer;
        private Vector3 lockedDirection;
        private bool alreadyHit;
        private bool eating;

        private void Awake()
        {
            health = GetComponent<EnemyHealth>();
            bossCollider = GetComponent<Collider>();
        }

        private void OnEnable()
        {
            if (health == null)
                health = GetComponent<EnemyHealth>();

            health.Died += OnDied;
        }

        private void OnDisable()
        {
            if (health != null)
                health.Died -= OnDied;
        }

        private void Start()
        {
            if (player == null)
            {
                PlayerController pc =
                    FindFirstObjectByType<PlayerController>();

                if (pc != null)
                    player = pc.transform;
            }

            State = BossState.Chase;
        }

        private void Update()
        {
            GameManager gm = GameManager.Instance;

            if (gm == null ||
                gm.Phase != GamePhase.Boss ||
                health.IsDead ||
                player == null ||
                eating)
                return;

            if (!Enraged &&
                health.CurrentHealth <=
                health.maxHealth * enragedHealthRatio)
            {
                Enraged = true;
                Debug.Log("[Boss] 進入狂怒狀態");
            }

            float factor = Enraged ? 1.45f : 1f;

            Vector3 towards = player.position - transform.position;
            towards.y = 0f;

            stateTimer -= Time.deltaTime;

            switch (State)
            {
                case BossState.Chase:
                    if (towards.magnitude <= attackDistance)
                    {
                        State = BossState.Prepare;
                        stateTimer = prepareSeconds / factor;

                        if (AudioManager.Instance != null)
                            AudioManager.Instance.Roar();

                        break;
                    }

                    if (towards.sqrMagnitude > 0.01f)
                    {
                        transform.rotation = Quaternion.Slerp(
                            transform.rotation,
                            Quaternion.LookRotation(towards),
                            Time.deltaTime * 4f
                        );

                        Vector3 step =
                            towards.normalized *
                            chaseSpeed * factor *
                            Time.deltaTime;

                        MoveInsideArena(step);
                    }
                    break;

                case BossState.Prepare:
                    if (stateTimer <= 0f)
                    {
                        lockedDirection =
                            towards.sqrMagnitude > 0.01f
                            ? towards.normalized
                            : transform.forward;

                        lockedDirection.y = 0f;
                        lockedDirection.Normalize();

                        State = BossState.Charge;
                        stateTimer = chargeSeconds;
                        alreadyHit = false;

                        Debug.Log("[Boss] 開始衝撞");
                    }
                    break;

                case BossState.Charge:
                    Vector3 displacement =
                        lockedDirection *
                        chargeSpeed * factor *
                        Time.deltaTime;

                    // 先偵測衝撞路徑是否有障礙物
                    if (ObstacleAhead(displacement))
                    {
                        StartRecovery(factor);
                        break;
                    }

                    // 在實際撞到碰撞體前，先判定玩家命中
                    if (!alreadyHit &&
                        PlayerInChargePath(displacement))
                    {
                        alreadyHit = true;
                        BeginEatPlayer();
                        break;
                    }

                    // 衝撞不可超出 Boss 活動區域
                    Vector3 nextPosition =
                        transform.position + displacement;

                    if (limitToArena &&
                        !InsideArena(nextPosition))
                    {
                        StartRecovery(factor);
                        break;
                    }

                    transform.position = nextPosition;

                    if (stateTimer <= 0f)
                        StartRecovery(factor);

                    break;

                case BossState.Recover:
                    if (stateTimer <= 0f)
                        State = BossState.Chase;
                    break;
            }
        }

        private void MoveInsideArena(Vector3 displacement)
        {
            Vector3 next =
                transform.position + displacement;

            if (limitToArena)
            {
                Vector3 offset = next - arenaCenter;
                offset.y = 0f;

                if (offset.magnitude > arenaRadius)
                {
                    offset =
                        offset.normalized * arenaRadius;

                    next.x = arenaCenter.x + offset.x;
                    next.z = arenaCenter.z + offset.z;
                }
            }

            transform.position = next;
        }

        private bool InsideArena(Vector3 position)
        {
            Vector3 difference = position - arenaCenter;
            difference.y = 0f;

            return difference.sqrMagnitude <=
                   arenaRadius * arenaRadius;
        }

        private bool ObstacleAhead(Vector3 displacement)
        {
            float distance = displacement.magnitude;

            if (distance <= 0.0001f)
                return false;

            Vector3 origin =
                transform.position + Vector3.up * 0.5f;

            RaycastHit[] hits = Physics.RaycastAll(
                origin,
                lockedDirection,
                distance + 0.5f,
                ~0,
                QueryTriggerInteraction.Ignore
            );

            foreach (RaycastHit hit in hits)
            {
                // 排除 Boss 本身
                if (hit.collider.transform == transform ||
                    hit.collider.GetComponentInParent<BossController>()
                    == this)
                    continue;

                // 玩家由專門的命中判定處理
                if (hit.collider.GetComponentInParent<PlayerController>()
                    != null)
                    continue;

                return true;
            }

            return false;
        }

        private float SafeHitDistance()
        {
            float bossRadius = 0.5f;

            if (bossCollider != null)
            {
                Vector3 extents = bossCollider.bounds.extents;
                bossRadius = Mathf.Max(extents.x, extents.z);
            }

            float playerRadius = 0.5f;

            CharacterController cc =
                player.GetComponent<CharacterController>();

            if (cc != null)
            {
                Vector3 extents = cc.bounds.extents;
                playerRadius = Mathf.Max(extents.x, extents.z);
            }

            // 在碰撞體實際重疊之前觸發吃掉事件
            return Mathf.Max(
                hitDistance,
                bossRadius + playerRadius + 0.4f
            );
        }

        private bool PlayerInChargePath(Vector3 displacement)
        {
            Vector3 start = transform.position;
            Vector3 end = start + displacement;
            Vector3 target = player.position;

            start.y = 0f;
            end.y = 0f;
            target.y = 0f;

            Vector3 segment = end - start;
            float lengthSquared = segment.sqrMagnitude;

            float t = 0f;

            if (lengthSquared > 0.0001f)
            {
                t = Mathf.Clamp01(
                    Vector3.Dot(target - start, segment)
                    / lengthSquared
                );
            }

            Vector3 nearestPoint = start + segment * t;

            float distance =
                Vector3.Distance(nearestPoint, target);

            return distance <= SafeHitDistance();
        }

        private void StartRecovery(float factor)
        {
            State = BossState.Recover;
            stateTimer = recoverSeconds / factor;
        }

        private void BeginEatPlayer()
        {
            if (eating)
                return;

            eating = true;

            Debug.Log("[Boss] 衝撞命中！觸發吃掉玩家事件");

            PlayerController pc =
                player.GetComponent<PlayerController>();

            if (pc == null)
                pc = player.GetComponentInParent<PlayerController>();

            // 立刻停止玩家移動，避免繼續被推走
            if (pc != null)
                pc.enabled = false;

            BossEatSequence sequence =
                GetComponent<BossEatSequence>();

            if (sequence == null)
                sequence = gameObject.AddComponent<BossEatSequence>();

            sequence.BeginEat(pc);

            // 保底：即使動畫未正確回呼，也要進入 Lost
            StartCoroutine(EnsureFailure());
        }

        private IEnumerator EnsureFailure()
        {
            yield return new WaitForSecondsRealtime(
                failSafeSeconds
            );

            GameManager gm = GameManager.Instance;

            if (gm != null && gm.Phase == GamePhase.Boss)
            {
                gm.EatenByBoss();
                Debug.Log("[Boss] 玩家失敗，切換 Lost");
            }
        }

        private void OnDied(EnemyHealth enemy)
        {
            State = BossState.Dead;

            if (GameManager.Instance != null)
                GameManager.Instance.BossDefeated();
        }
    }
}
