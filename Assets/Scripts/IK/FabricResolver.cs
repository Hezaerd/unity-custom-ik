using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace IK
{
    public class FabricResolver : MonoBehaviour
    {
        public int chainLength = 2;

        public Transform Target;
        public Transform Pole;

        [Header("Settings")]
        public int maxIterations = 10;
        public float delta = 0.001f;

        [Range(0, 1)]
        public float snapBackWeight = 1f;

        protected float[] BonesLength; //Target to Origin
        protected float CompleteLength;
        protected Transform[] Bones;
        protected Vector3[] Positions;
        protected Vector3[] StartDirectionSucc;
        protected Quaternion[] StartRotationBone;
        protected Quaternion StartRotationTarget;
        protected Transform Root;

        public void Awake()
        {
            Bones = new Transform[chainLength + 1];
            Positions = new Vector3[chainLength + 1];
            BonesLength = new float[chainLength];
            StartDirectionSucc = new Vector3[chainLength + 1];
            StartRotationBone = new Quaternion[chainLength + 1];

            // Search for root bone
            Root = transform;
            for (var i = 0; i < chainLength; i++)
            {
              if (Root == null)
                {
                    Debug.LogError("Root bone not found");
                    throw new UnityException("Root bone not found");
                }

                Root = Root.parent;
            }

            // Init the target
            if (Target == null)
            {
                Target = new GameObject(gameObject.name + "_Target").transform;
                SetPositionRootSpace(Target, GetPositionRootSpace(transform));
            }
            StartRotationTarget = GetRotationRootSpace(Target);

            // Init bones
            var current = transform;
            CompleteLength = 0;
            for (var i = Bones.Length - 1; i >= 0; i--)
            {
                Bones[i] = current;
                StartRotationBone[i] = GetRotationRootSpace(current);

                if (i == Bones.Length - 1)
                    StartDirectionSucc[i] = GetPositionRootSpace(Target) - GetPositionRootSpace(current);
                else
                {
                    //mid bone
                    StartDirectionSucc[i] = GetPositionRootSpace(Bones[i + 1]) - GetPositionRootSpace(current);
                    BonesLength[i] = StartDirectionSucc[i].magnitude;
                    CompleteLength += BonesLength[i];
                }

                current = current.parent;
            }
        }

        /// <summary>
        /// We use LateUpdate to solve the IK after all the engine physics calculations
        /// </summary>
        public void LateUpdate()
        {
            if (Target != null)
                Solve();
        }

        private void Solve()
        {
            if (BonesLength.Length != chainLength)
                throw new UnityException("BonesLength.Length != chainLength");

            // Get Position
            for (int i = 0; i < Bones.Length; i++)
                Positions[i] = GetPositionRootSpace(Bones[i]);

            var targetPosition = GetPositionRootSpace(Target);
            var targetRotation = GetRotationRootSpace(Target);

            // Check if the first bone is possible to reach
            if ((targetPosition - GetPositionRootSpace(Bones[0])).sqrMagnitude >= CompleteLength * CompleteLength)
            {
                var direction = (targetPosition - Positions[0]).normalized;
                for (int i = 1; i < Positions.Length; i++)
                    Positions[i] = Positions[i - 1] + direction * BonesLength[i - 1];
            }
            else
            {
                for (int i = 0; i < Positions.Length - 1; i++)
                    Positions[i + 1] = Vector3.Lerp(Positions[i + 1], Positions[i] + StartDirectionSucc[i], snapBackWeight);

                for (int iteration = 0; iteration < maxIterations; iteration++)
                {
                    for (int i = Positions.Length - 1; i > 0; i--)
                    {
                        if (i == Positions.Length - 1)
                            Positions[i] = targetPosition;
                        else
                            Positions[i] = Positions[i + 1] + (Positions[i] - Positions[i + 1]).normalized * BonesLength[i];
                    }

                    for (int i = 1; i < Positions.Length; i++)
                        Positions[i] = Positions[i - 1] + (Positions[i] - Positions[i - 1]).normalized * BonesLength[i - 1];

                    if ((Positions[^1] - targetPosition).sqrMagnitude < delta * delta)
                        break;
                }
            }

            // Move towards pole
            if (Pole != null)
            {
                var polePosition = GetPositionRootSpace(Pole);
                for (int i = 0; i < Positions.Length; i++)
                {
                    // We use a plane instead of a raycast to find the closest point
                    // This is more efficient and works well for our case
                    // We use the previous and next bone to define the plane
                    // We then project the pole and the bone onto the plane
                    // And calculate the signed angle between the two projected points
                    // We use this angle to rotate the bone around its axis

                    var plane = new Plane(Positions[i + 1] - Positions[i - 1], Positions[i - 1]);
                    var projectedPole = plane.ClosestPointOnPlane(polePosition);
                    var projectedBone = plane.ClosestPointOnPlane(Positions[i]);
                    var angle = Vector3.SignedAngle(projectedBone - Positions[i - 1], projectedPole - Positions[i - 1], plane.normal);
                    Positions[i] = Quaternion.AngleAxis(angle, plane.normal) * (Positions[i] - Positions[i - 1]) + Positions[i - 1];
                }
            }

            // And now we can FINALLY set the positions and rotations
            for (int i = 0; i < Positions.Length; i++)
            {
                // rot
                if (i == Positions.Length - 1)
                    SetRotationRootSpace(Bones[i], Quaternion.Inverse(targetRotation) * StartRotationTarget * Quaternion.Inverse(StartRotationBone[i]));
                else
                    SetRotationRootSpace(Bones[i], Quaternion.FromToRotation(StartDirectionSucc[i], Positions[i + 1] - Positions[i]) * Quaternion.Inverse(StartRotationBone[i])); // I should simplify this...

                // pos
                SetPositionRootSpace(Bones[i], Positions[i]);
            }
        }

        private Vector3 GetPositionRootSpace(Transform current)
        {
            if (Root == null)
                return current.position;
            else
                return Quaternion.Inverse(Root.rotation) * (current.position - Root.position);
        }

        private void SetPositionRootSpace(Transform current, Vector3 position)
        {
            if (Root == null)
                current.position = position;
            else
                current.position = Root.rotation * position + Root.position;
        }

        private Quaternion GetRotationRootSpace(Transform current)
        {
            if (Root == null)
                return current.rotation;
            else
                return Quaternion.Inverse(current.rotation) * Root.rotation;
        }

        private void SetRotationRootSpace(Transform current, Quaternion rotation)
        {
            if (Root == null)
                current.rotation = rotation;
            else
                current.rotation = Root.rotation * rotation;
        }

        private void OnDrawGizmos()
        {
            #if UNITY_EDITOR
            var current = transform;
            for (int i = 0; i < chainLength && current != null && current.parent != null; i++)
            {
                var scale = Vector3.Distance(current.position, current.parent.position) * 0.1f;
                Handles.matrix = Matrix4x4.TRS(current.position, Quaternion.FromToRotation(Vector3.up, current.parent.position - current.position), new Vector3(scale, Vector3.Distance(current.parent.position, current.position), scale));
                Handles.color = Color.green;
                Handles.DrawWireCube(Vector3.up * 0.5f, Vector3.one);
                current = current.parent;
            }
            #endif
        }

    }
}
