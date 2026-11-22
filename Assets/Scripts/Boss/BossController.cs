using UnityEngine;

namespace ForestAdventure
{
    public enum BossState { Idle, Chase, Prepare, Charge, Recover, Dead }
    [RequireComponent(typeof(EnemyHealth))]
    public class BossController : MonoBehaviour
    {
        public Transform player;
        public float chaseSpeed = 3.7f;
        public float chargeSpeed = 13f;
        public float prepareSeconds = 1.6f;
        public float chargeSeconds = 1.3f;
        public float recoverSeconds = 1.3f;
        public float attackDistance = 9f;
        public float hitDistance = 1.5f;
        public float enragedHealthRatio = 0.35f;
        public BossState State { get; private set; } = BossState.Idle;
        public bool Enraged { get; private set; }
        private EnemyHealth health;
        private float stateTimer;
        private Vector3 lockedDirection;
        private bool alreadyHit;
        private bool eating;

        private void Awake() { health = GetComponent<EnemyHealth>(); }
        private void OnEnable() { health.Died += OnDied; }
        private void OnDisable() { if (health != null) health.Died -= OnDied; }
        private void Start()
        {
            if (player == null)
            {
                var pc = FindFirstObjectByType<PlayerController>();
                if (pc != null) player = pc.transform;
            }
            State = BossState.Chase;
        }
        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.Phase != GamePhase.Boss || health.IsDead || player == null || eating) return;
            if (!Enraged && health.CurrentHealth <= health.maxHealth * enragedHealthRatio) Enraged = true;
            float factor = Enraged ? 1.45f : 1f;
            Vector3 towards = player.position - transform.position;
            towards.y = 0;
            stateTimer -= Time.deltaTime;
            switch (State)
            {
                case BossState.Chase:
                    if (towards.sqrMagnitude > 0.01f)
                    {
                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(towards), Time.deltaTime * 4f);
                        transform.position += towards.normalized * chaseSpeed * factor * Time.deltaTime;
                    }
                    if (towards.magnitude <= attackDistance) { State = BossState.Prepare; stateTimer = prepareSeconds / factor;
                        if (AudioManager.Instance != null) AudioManager.Instance.Roar(); }
                    break;
                case BossState.Prepare:
                    if (stateTimer <= 0)
                    {
                        lockedDirection = towards.sqrMagnitude > 0.01f ? towards.normalized : transform.forward;
                        State = BossState.Charge;
                        stateTimer = chargeSeconds;
                        alreadyHit = false;
                    }
                    break;
                case BossState.Charge:
                    Vector3 previous = transform.position;
                    Vector3 displacement = lockedDirection * chargeSpeed * factor * Time.deltaTime;
                    RaycastHit wall;
                    if (Physics.Raycast(previous + Vector3.up, lockedDirection, out wall, displacement.magnitude + 0.5f, ~0, QueryTriggerInteraction.Ignore)
                        && wall.collider.GetComponentInParent<PlayerController>() == null
                        && wall.collider.GetComponentInParent<BossController>() == null)
                    {
                        State = BossState.Recover;
                        stateTimer = recoverSeconds / factor;
                        break;
                    }
                    transform.position += displacement;
                    if (!alreadyHit && Vector3.Distance(transform.position, player.position) <= hitDistance)
                    {
                        alreadyHit = true;
                        eating = true;
                        var sequence = GetComponent<BossEatSequence>();
                        if (sequence == null) sequence = gameObject.AddComponent<BossEatSequence>();
                        sequence.BeginEat(player.GetComponent<PlayerController>());

                    }
                    if (stateTimer <= 0) { State = BossState.Recover; stateTimer = recoverSeconds / factor; }
                    break;
                case BossState.Recover:
                    if (stateTimer <= 0) State = BossState.Chase;
                    break;
            }
        }
        private void OnDied(EnemyHealth _) { State = BossState.Dead; if (GameManager.Instance != null) GameManager.Instance.BossDefeated(); }
    }
}
