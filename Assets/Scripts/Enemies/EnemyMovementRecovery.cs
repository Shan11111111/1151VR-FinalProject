using UnityEngine;
using UnityEngine.AI;

namespace ForestAdventure
{
    // Optional formal-scene recovery; needs baked NavMesh to be useful.
    [RequireComponent(typeof(EnemyAI))]
    public class EnemyMovementRecovery : MonoBehaviour
    {
        public float checkInterval = 1f;
        public float stuckSeconds = 3f;
        public float minTravelPerCheck = 0.08f;
        public float sampleRadius = 6f;
        private NavMeshAgent agent;
        private EnemyAI ai;
        private EnemyHealth hp;
        private float nextCheck;
        private float notMovingSeconds;
        private Vector3 lastPosition;
        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>(); ai = GetComponent<EnemyAI>(); hp = GetComponent<EnemyHealth>();
            lastPosition = transform.position;
        }
        private void Update()
        {
            if (hp != null && hp.IsDead) return;
            if (GameManager.Instance == null || GameManager.Instance.Phase != GamePhase.Hunting) return;
            if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;
            if (Time.time < nextCheck) return;
            nextCheck = Time.time + checkInterval;
            bool wantsMove = agent.hasPath && agent.remainingDistance > agent.stoppingDistance + 0.4f;
            bool stuck = wantsMove && (transform.position - lastPosition).sqrMagnitude < minTravelPerCheck * minTravelPerCheck;
            notMovingSeconds = stuck ? notMovingSeconds + checkInterval : 0f;
            lastPosition = transform.position;
            if (agent.pathStatus == NavMeshPathStatus.PathInvalid || notMovingSeconds >= stuckSeconds)
            {
                notMovingSeconds = 0f;
                ai.ForceNewDestination();
                if (!agent.hasPath || agent.pathStatus == NavMeshPathStatus.PathInvalid)
                {
                    NavMeshHit hit;
                    Vector3 pos = ai.huntCentre + Random.insideUnitSphere * (ai.huntRadius * 0.6f);
                    if (NavMesh.SamplePosition(pos, out hit, sampleRadius, NavMesh.AllAreas))
                    {
                        agent.Warp(hit.position); // do not change enemy ID, health, or kill count
                        ai.ForceNewDestination();
                    }
                }
            }
        }
    }
}
