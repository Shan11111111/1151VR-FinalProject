using System.Collections;
using UnityEngine;
namespace ForestAdventure
{
    // Primitive/demo-only waking up visual: camera rises from pillow-height to standing-height.
    // Formal animation can replace this component with real bed/camera animation.
    public class DemoWakeSequence : MonoBehaviour
    {
        [Min(0.1f)] public float wakeDuration = 2.5f;
        public float pillowEyeHeight = 0.65f;
        public float standingEyeHeight = 1.6f;
        private bool done;
        public void PlayWake()
        {
            if (done) return;
            done = true;
            StartCoroutine(Wake());
        }
        private IEnumerator Wake()
        {
            var controller = GetComponent<PlayerController>();
            if (controller != null) controller.SetLocked(true);
            var cam = controller != null ? controller.playerCamera : GetComponentInChildren<Camera>();
            if (cam != null)
            {
                Vector3 target = cam.transform.localPosition;
                Vector3 start = target; start.y = pillowEyeHeight;
                target.y = standingEyeHeight;
                float elapsed = 0f;
                while (elapsed < wakeDuration)
                {
                    elapsed += Time.deltaTime;
                    cam.transform.localPosition = Vector3.Lerp(start,target,Mathf.Clamp01(elapsed/wakeDuration));
                    yield return null;
                }
                cam.transform.localPosition = target;
            }
            if (controller != null) controller.SetLocked(false);
        }
    }
}
