using UnityEngine;

namespace ForestAdventure
{
    // Assign clips in Inspector once audio assets arrive from the team.
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }
        public AudioClip shotClip;
        public AudioClip punchClip;
        public AudioClip enemyDeathClip;
        public AudioClip gemPickupClip;
        public AudioClip bossRoarClip;
        public AudioClip eatenClip;
        public AudioSource source;
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            if (source == null) source = GetComponent<AudioSource>();
            if (source == null) source = gameObject.AddComponent<AudioSource>();
        }
        private void OnDestroy() { if (Instance == this) Instance = null; }
        public void Play(AudioClip clip)
        {
            if (clip != null && source != null) source.PlayOneShot(clip);
        }
        public void Shoot() { Play(shotClip); }
        public void Punch() { Play(punchClip); }
        public void EnemyDied() { Play(enemyDeathClip); }
        public void GemCollected() { Play(gemPickupClip); }
        public void Roar() { Play(bossRoarClip); }
        public void Eaten() { Play(eatenClip); }
    }
}
