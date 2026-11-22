using UnityEngine;

namespace ForestAdventure
{
    // Optional: wire Animator parameters only if they exist. Does not own gameplay state.
    public class GameplayAnimationBridge : MonoBehaviour
    {
        public Animator animator;
        public EnemyHealth health;
        public EnemyAI animal;
        public BossController boss;
        public string speedParameter = "Speed";
        public string dieTrigger = "Die";
        private Vector3 lastPos;
        private bool deadTriggered;
        private void Awake()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (health == null) health = GetComponent<EnemyHealth>();
            if (animal == null) animal = GetComponent<EnemyAI>();
            if (boss == null) boss = GetComponent<BossController>();
            lastPos = transform.position;
        }
        private void Update()
        {
            if (animator == null) return;
            float speed = (transform.position - lastPos).magnitude / Mathf.Max(Time.deltaTime, 0.001f);
            lastPos = transform.position;
            if (HasParameter(speedParameter, AnimatorControllerParameterType.Float)) animator.SetFloat(speedParameter, speed);
            if (!deadTriggered && health != null && health.IsDead)
            {
                deadTriggered = true;
                if (HasParameter(dieTrigger, AnimatorControllerParameterType.Trigger)) animator.SetTrigger(dieTrigger);
            }
            if (boss != null && HasParameter("IsEnraged", AnimatorControllerParameterType.Bool))
                animator.SetBool("IsEnraged", boss.Enraged);
        }
        private bool HasParameter(string name, AnimatorControllerParameterType type)
        {
            foreach (var p in animator.parameters) if (p.name == name && p.type == type) return true;
            return false;
        }
    }
}
