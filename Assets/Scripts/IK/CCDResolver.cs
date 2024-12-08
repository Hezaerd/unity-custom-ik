using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace IK
{
    public class CCDResolver : MonoBehaviour
    {
        public int chainLength = 2;
        public Transform Target;

        [Header("Settings")]
        [SerializeField] private int maxIterations = 10;
        [SerializeField] private float threshold = 0.001f;

        [Header("Debug")]
        [SerializeField] private bool drawGizmos = true;
        [ReadOnly] public float currentError;

        protected Transform[] joints;
        protected Quaternion[] initialRotations;
        protected Quaternion targetInitialRotation;
        protected Quaternion endInitialRotation;
        protected Transform root;

        private void Awake()
        {
            InitializeChain();
        }

        private void InitializeChain()
        {
            joints = new Transform[chainLength + 1];

            // Store initial rotations
            if (Target != null)
                targetInitialRotation = Target.rotation;
            endInitialRotation = transform.rotation;

            // Find root and initialize chain
            var current = transform;
            for (var i = chainLength; i >= 0; i--)
            {
                if (i < chainLength)
                {
                    if (current == null)
                    {
                        Debug.LogError("Chain length is longer than ancestor chain!");
                        return;
                    }
                    current = current.parent;
                }

                if (i == chainLength)
                    joints[i] = transform;
                else
                    joints[i] = current;
            }

            root = joints[0];
        }

        private void LateUpdate()
        {
            if (Target == null) return;
            Solve();
        }

        private void Solve()
        {
            var lastBone = joints[joints.Length - 1];
            currentError = Vector3.Distance(lastBone.position, Target.position);

            for (int iteration = 0; iteration < maxIterations; iteration++)
            {
                for (int i = joints.Length - 1; i >= 0; i--)
                {
                    if (i == joints.Length - 1)
                    {
                        // Handle end effector rotation
                        joints[i].rotation = Target.rotation *
                            Quaternion.Inverse(targetInitialRotation) *
                            endInitialRotation;
                    }
                    else
                    {
                        // Regular CCD rotation
                        joints[i].rotation = Quaternion.FromToRotation(
                            lastBone.position - joints[i].position,
                            Target.position - joints[i].position
                        ) * joints[i].rotation;
                    }
                }

                // Check if we've reached the target
                currentError = Vector3.Distance(lastBone.position, Target.position);
                if (currentError <= threshold)
                    break;
            }
        }

        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // if (!drawGizmos || joints == null) return;

            // // Draw chain
            // for (int i = 0; i < joints.Length - 1; i++)
            // {
            //     if (joints[i] == null || joints[i + 1] == null) continue;

            //     var scale = Vector3.Distance(joints[i].position, joints[i + 1].position) * 0.1f;
            //     Handles.matrix = Matrix4x4.TRS(
            //         joints[i].position,
            //         Quaternion.FromToRotation(Vector3.up, joints[i + 1].position - joints[i].position),
            //         new Vector3(scale, Vector3.Distance(joints[i + 1].position, joints[i].position), scale)
            //     );
            //     Handles.color = Color.blue;
            //     Handles.DrawWireCube(Vector3.up * 0.5f, Vector3.one);
            // }

            // // Draw error
            // if (Target != null && joints[joints.Length - 1] != null)
            // {
            //     Handles.color = Color.yellow;
            //     Handles.DrawDottedLine(joints[joints.Length - 1].position, Target.position, 5);
            // }
        }
        #endif
    }
}
