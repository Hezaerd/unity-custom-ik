using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IK
{
    public class CCDResolver : BaseIKResolver
    {
        [SerializeField] private Transform endEffector;
        [SerializeField] private float positionThreshold = 0.01f;

        // Configurable weights for better control
        [SerializeField] private float[] positionWeights = { 0.2f, 0.7f, 0.6f, 0.5f };
        [SerializeField] private float[] rotationWeights = { 0.1f, 0.5f, 0.5f, 0.8f };

        private void Update()
        {
            if (target != null && joints != null && joints.Length > 0)
            {
                Solve();
            }
        }

        public override void Solve()
        {
            float sqrDistance;

            for (int iteration = 0; iteration < maxIterations; iteration++)
            {
                for (int i = joints.Length - 1; i >= 0; i--)
                {
                    Transform joint = joints[i];

                    // Apply rotation weights for the end effector
                    if (i == joints.Length - 1)
                    {
                        joint.rotation = Quaternion.Slerp(
                            joint.rotation,
                            target.rotation,
                            rotationWeights[Mathf.Min(i, rotationWeights.Length - 1)]
                        );
                    }
                    else
                    {
                        // First, apply some rotation towards target orientation
                        joint.rotation = Quaternion.Slerp(
                            joint.rotation,
                            target.rotation,
                            rotationWeights[Mathf.Min(i, rotationWeights.Length - 1)]
                        );

                        // Then, adjust position using FromToRotation
                        Vector3 toEndEffector = joints[^1].position - joint.position;
                        Vector3 toTarget = target.position - joint.position;

                        Quaternion newRotation = Quaternion.FromToRotation(toEndEffector, toTarget) * joint.rotation;
                        joint.rotation = Quaternion.Slerp(
                            joint.rotation,
                            newRotation,
                            positionWeights[Mathf.Min(i, positionWeights.Length - 1)]
                        );
                    }

                    // Check if we've reached the target
                    sqrDistance = (joints[^1].position - target.position).sqrMagnitude;
                    if (sqrDistance <= positionThreshold)
                        return;
                }
            }
        }
    }
}
