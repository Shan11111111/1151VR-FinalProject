using System.Collections;
using UnityEngine;

namespace ForestAdventure
{
    // Put on the Boss (or boss root) together with BossController.
    // Animator optional. Event in the eat animation can call CompleteEatAnimation().
    public class BossEatSequence : MonoBehaviour
    {
        [Min(0f)] public float fallbackDuration = 2f;
        public string animatorTrigger = "Eat";
        public Animator animator;
        private bool playing;
        private Coroutine routine;
        private void Awake() { if (animator == null) animator = GetComponentInChildren<Animator>(); }
        public void BeginEat(PlayerController player)
        {
            if (playing) return;
            playing = true;
            if (player != null) player.SetLocked(true);
            if (AudioManager.Instance != null) AudioManager.Instance.Eaten();
            if (animator != null)
            {
                foreach (var p in animator.parameters)
                {
                    if (p.name == animatorTrigger && p.type == AnimatorControllerParameterType.Trigger)
                    { animator.SetTrigger(animatorTrigger); break; }
                }
            }
            routine = StartCoroutine(Fallback());
        }
        private IEnumerator Fallback()
        {
            yield return new WaitForSecondsRealtime(fallbackDuration);
            CompleteEatAnimation();
        }
        public void CompleteEatAnimation()
        {
            if (!playing) return;
            playing = false;
            if (routine != null) { StopCoroutine(routine); routine = null; }
            if (GameManager.Instance != null) GameManager.Instance.EatenByBoss();
        }
    }
}
