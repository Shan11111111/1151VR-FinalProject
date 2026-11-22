using UnityEngine;
namespace ForestAdventure
{
    // Interaction near the portal uses this. Trigger can optionally start immediately on entry.
    public class BossPortal : MonoBehaviour
    {
        public bool enterOnTouch = true;
        private void OnTriggerEnter(Collider other)
        {
            if (enterOnTouch && other.GetComponentInParent<PlayerController>() != null)
                TryEnter();
        }
        public void TryEnter()
        {
            var gm = GameManager.Instance;
            if (gm != null && gm.BossPortalActive) gm.EnterBossPortal();
            else Debug.Log("Boss portal locked. Collect the green gems and unlock the special gun first.");
        }
    }
}
