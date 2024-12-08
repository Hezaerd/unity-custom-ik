using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace IK
{
    public class BaseIKResolver : MonoBehaviour
    {
        [Header("IK Settings")]
        public Transform root;
        public Transform target;
        public float threshold = 0.1f;
        public int maxIterations = 10;

        [Header("Joints")]
        public Transform[] joints;

        public virtual void Init() {}

        public virtual void Solve() {}

        [Button]
        public void AutoDetectJoints()
        {
            if (root == null)
            {
                Debug.LogError("Root transform is not assigned!");
                return;
            }

            if (target == null)
            {
                Debug.LogError("Target transform is not assigned!");
                return;
            }

            List<Transform> detectedJoints = new List<Transform>();
            Transform current = target.parent;

            // Traverse up the hierarchy until we reach the root or null
            while (current != null && current != root.parent)
            {
                detectedJoints.Insert(0, current);

                if (current == root)
                {
                    break;
                }

                current = current.parent;
            }

            if (!detectedJoints.Contains(root))
            {
                Debug.LogError("Root is not in the same hierarchy as the target!");
                return;
            }

            joints = detectedJoints.ToArray();
            Debug.Log($"Successfully detected {joints.Length} joints in the IK chain");
        }
    }
}
