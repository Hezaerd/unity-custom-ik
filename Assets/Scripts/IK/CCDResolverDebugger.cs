using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace IK
{
    [RequireComponent(typeof(CCDResolver))]
    public class CCDResolverDebugger : MonoBehaviour
    {
        private CCDResolver resolver;

        [Header("Gizmo Settings")]
        public bool showGizmos = true;
        public Color chainColor = new Color(0f, 1f, 0f, 0.5f);
        public float jointRadius = 0.05f;

        private void Awake()
        {
            resolver = GetComponent<CCDResolver>();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!showGizmos || resolver == null) return;

            var joints = resolver.joints;
            var target = resolver.target;
            var root = resolver.root;

            if (joints == null) return;

            // Cache the original Gizmos color
            Color originalColor = Gizmos.color;
            Gizmos.color = chainColor;

            // Draw joints and connections
            for (int i = 0; i < joints.Length; i++)
            {
                if (joints[i] == null) continue;

                // Draw sphere at joint position
                Gizmos.DrawWireSphere(joints[i].position, jointRadius);

                // Draw line to next joint or target
                if (i < joints.Length - 1 && joints[i + 1] != null)
                {
                    Gizmos.DrawLine(joints[i].position, joints[i + 1].position);
                }
                else if (i == joints.Length - 1 && target != null)
                {
                    // Draw line to target from last joint
                    Gizmos.DrawLine(joints[i].position, target.position);
                }
            }

            // Draw target
            if (target != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(target.position, jointRadius * 1.5f);
            }

            // Draw root
            if (root != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(root.position, jointRadius * 1.5f);
            }

            // Restore original Gizmos color
            Gizmos.color = originalColor;
        }
#endif
    }
}