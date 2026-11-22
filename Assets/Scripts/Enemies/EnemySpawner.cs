using System.Collections.Generic;
using UnityEngine;

namespace ForestAdventure
{
    /// <summary>Three groups, one spawn point per group. Prefabs optional; primitives for demo.</summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Three species; null uses a visible primitive")]
        public GameObject[] animalPrefabs = new GameObject[3];
        public Transform[] spawnPoints = new Transform[3];
        [Header("Counts per species; example 10/10/10")]
        public int[] counts = new int[] { 10, 10, 10 };
        [Range(1, 20)] public int maxAlive = 5;
        [Min(0.1f)] public float spawnInterval = 1.2f;
        [Min(1)] public int animalHP = 40;
        // Legacy fallback field for prefab configurations from previous demo
        public GameObject enemyPrefab;
        private readonly List<EnemyHealth> living = new List<EnemyHealth>();
        private readonly int[] spawned = new int[3];
        private int totalSpawned;
        private float timer;
        private int nextGroup;

        private void Update()
        {
            GameManager gm = GameManager.Instance;
            if (gm == null || gm.Phase != GamePhase.Hunting) return;
            living.RemoveAll(e => e == null || e.IsDead);
            if (living.Count >= maxAlive || totalSpawned >= TotalCount()) return;
            timer -= Time.deltaTime;
            if (timer > 0f) return;
            timer = spawnInterval;
            int group = PickGroup();
            if (group >= 0) Spawn(group);
        }
        private int Count(int group) => counts != null && counts.Length > group ? Mathf.Max(0, counts[group]) : 0;
        public int TotalCount() => Count(0) + Count(1) + Count(2);
        private int PickGroup()
        {
            // Rotating groups while keeping exactly the requested number per species.
            for (int attempt = 0; attempt < 3; attempt++)
            {
                int group = (nextGroup + attempt) % 3;
                if (spawned[group] < Count(group)) { nextGroup = (group + 1) % 3; return group; }
            }
            return -1;
        }
        private void Spawn(int group)
        {
            Vector3 position = transform.position;
            if (spawnPoints != null && spawnPoints.Length > group && spawnPoints[group] != null)
                position = spawnPoints[group].position;
            GameObject prefab = animalPrefabs != null && animalPrefabs.Length > group ? animalPrefabs[group] : null;
            if (prefab == null) prefab = enemyPrefab;
            GameObject animal = prefab != null ? Instantiate(prefab, position, Quaternion.identity) : CreatePrimitive(group, position);
            EnemyHealth health = animal.GetComponent<EnemyHealth>();
            if (health == null) health = animal.AddComponent<EnemyHealth>();
            health.ResetHealth(animalHP);
            if (animal.GetComponent<Collider>() == null) animal.AddComponent<CapsuleCollider>();
            if (animal.GetComponent<EnemyGemDrop>() == null) animal.AddComponent<EnemyGemDrop>();
            if (animal.GetComponent<EnemyWorldLabel>() == null) animal.AddComponent<EnemyWorldLabel>();
            EnemyAI ai = animal.GetComponent<EnemyAI>();
            if (ai == null) ai = animal.AddComponent<EnemyAI>();
            ai.behaviour = (AnimalBehaviour)group;
            ai.huntCentre = transform.position;
            ai.huntRadius = 24f;
            totalSpawned++;
            spawned[group]++;
            health.AssignId(totalSpawned);
            health.Died += OnEnemyDied;
            living.Add(health);
            animal.name = "Animal_Type" + (group + 1) + "_" + totalSpawned.ToString("00");
        }
        private static GameObject CreatePrimitive(int group, Vector3 position)
        {
            PrimitiveType primitive = group == 0 ? PrimitiveType.Capsule : group == 1 ? PrimitiveType.Cube : PrimitiveType.Sphere;
            GameObject o = GameObject.CreatePrimitive(primitive);
            o.transform.position = position;
            o.transform.localScale = group == 1 ? new Vector3(1.3f, 1.5f, 1.3f) : Vector3.one;
            o.GetComponent<Renderer>().material.color = group == 0 ? new Color(.85f,.52f,.26f) : group == 1 ? new Color(.4f,.7f,.95f) : new Color(.83f,.55f,.82f);
            return o;
        }
        private void OnEnemyDied(EnemyHealth enemy)
        {
            enemy.Died -= OnEnemyDied;
            living.Remove(enemy);
            if (GameManager.Instance != null) GameManager.Instance.RegisterKill();
        }
    }
}
