using UnityEngine;
using UnityEngine.AI;

namespace ForestAdventure
{
    public enum AnimalBehaviour { Patrol, Flee, Approach }

    // Works with NavMeshAgent when baked; direct movement fallback for the primitive demo.
    [RequireComponent(typeof(EnemyHealth))]
    public class EnemyAI : MonoBehaviour
    {
        public AnimalBehaviour behaviour;
        public Transform target;
        public Vector3 huntCentre;
        [Min(1)] public float huntRadius = 24f;
        [Min(0.1f)] public float speed = 2f;
        [Min(1)] public float detectionRange = 12f;
        private NavMeshAgent agent;
        private EnemyHealth health;
        private Vector3 destination;
        private float refresh;
        private float stuckTimer;
        private Vector3 lastPosition;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            health = GetComponent<EnemyHealth>();
            lastPosition = transform.position;
        }
        private void Start()
        {
            if (target == null)
            {
                var player = FindFirstObjectByType<PlayerController>();
                if (player != null) target = player.transform;
            }
            PickPatrolDestination();
        }
        private void Update()
        {
            if (health.IsDead || GameManager.Instance == null || GameManager.Instance.Phase != GamePhase.Hunting) return;
            refresh -= Time.deltaTime;
            if (refresh <= 0)
            {
                refresh = 0.7f;
                DecideDestination();
            }
            Move();
            if ((transform.position - lastPosition).sqrMagnitude < 0.01f) stuckTimer += Time.deltaTime;
            else { stuckTimer = 0; lastPosition = transform.position; }
            if (stuckTimer > 3f) { PickPatrolDestination(); stuckTimer = 0; }
        }
        private void DecideDestination()
        {
            if (target == null) { PickPatrolDestination(); return; }
            Vector3 delta = transform.position - target.position;
            float distance = delta.magnitude;
            if (behaviour == AnimalBehaviour.Flee && distance < detectionRange)
                SetDestination(transform.position + delta.normalized * 8f);
            else if (behaviour == AnimalBehaviour.Approach && distance < detectionRange)
                SetDestination(target.position);
            else if ((transform.position - destination).sqrMagnitude < 3f || behaviour == AnimalBehaviour.Patrol)
                PickPatrolDestination();
        }
        public void ForceNewDestination() { PickPatrolDestination(); }
        private void PickPatrolDestination()
        {
            Vector2 p = Random.insideUnitCircle * huntRadius;
            SetDestination(huntCentre + new Vector3(p.x, 0f, p.y));
        }
        private void SetDestination(Vector3 point)
        {
            Vector3 flat = point - huntCentre;
            flat.y = 0;
            if (flat.magnitude > huntRadius) point = huntCentre + flat.normalized * huntRadius;
            destination = point;
            if (agent != null && agent.isOnNavMesh)
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(point, out hit, 4f, NavMesh.AllAreas))
                {
                    destination = hit.position;
                    agent.SetDestination(destination);
                }
            }
        }
        private void Move()
        {
            if (agent != null && agent.isOnNavMesh)
            {
                agent.speed = speed;
                return;
            }
            // Basic fallback for demo only; formal forest needs baked NavMesh.
            Vector3 direction = destination - transform.position;
            direction.y = 0;
            if (direction.sqrMagnitude < 0.15f) return;
            Vector3 step = direction.normalized * speed * Time.deltaTime;
            transform.position += step;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction.normalized), Time.deltaTime * 5f);
        }
    }
}
