using UnityEngine;

namespace ForestAdventure
{
    public class ExplorationGoal : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<PlayerController>() != null && GameManager.Instance != null)
                GameManager.Instance.CompleteExploration();
        }
    }
}
