using UnityEngine;

namespace ForestAdventure
{
    [RequireComponent(typeof(EnemyHealth))]
    public class EnemyGemDrop : MonoBehaviour
    {
        [Tooltip("Optional official green gem prefab; fallback is a green sphere.")]
        public GameObject gemPrefab;
        [Min(0)] public float deathDelay = 0.7f;
        private EnemyHealth health;
        private bool dropped;
        private void Awake() { health = GetComponent<EnemyHealth>(); }
        private void OnEnable() { health.Died += OnDeath; }
        private void OnDisable() { if (health != null) health.Died -= OnDeath; }
        private void OnDeath(EnemyHealth _)
        {
            if (dropped) return;
            dropped = true;
            if (AudioManager.Instance != null) AudioManager.Instance.EnemyDied();
            var ai = GetComponent<EnemyAI>(); if (ai != null) ai.enabled = false;
            var animator = GetComponentInChildren<Animator>();
            if (animator != null && GetComponent<GameplayAnimationBridge>() == null)
                foreach (var p in animator.parameters)
                    if (p.name == "Die" && p.type == AnimatorControllerParameterType.Trigger)
                        { animator.SetTrigger("Die"); break; }
            Invoke(nameof(SpawnGemAndRemove), deathDelay);
        }
        private void SpawnGemAndRemove()
        {
            Vector3 position = transform.position + Vector3.up * 0.6f;
            GameObject gem = gemPrefab != null ? Instantiate(gemPrefab, position, Quaternion.identity) : GameObject.CreatePrimitive(PrimitiveType.Sphere);
            if (gemPrefab == null)
            {
                gem.name = "GreenGem";
                gem.transform.position = position;
                gem.transform.localScale = Vector3.one * 0.5f;
                var renderer = gem.GetComponent<Renderer>(); renderer.material.color = Color.green;
            }
            var pickup = gem.GetComponent<PickupItem>();
            if (pickup == null) pickup = gem.AddComponent<PickupItem>();
            pickup.kind = PickupKind.Gem;
            pickup.amount = 1;
            Destroy(gameObject);
        }
    }
}
