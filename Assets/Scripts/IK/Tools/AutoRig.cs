using UnityEngine;
using IK;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AutoRig : MonoBehaviour
{
    [FoldoutGroup("Settings")]
    [SerializeField, Range(2, 10)]
    private int maxChainLength = 3;

    [FoldoutGroup("Settings")]
    [SerializeField]
    private bool createTargets = true;

    [FoldoutGroup("Settings")]
    [SerializeField, ShowIf("createTargets")]
    private string targetSuffix = "_Target";

    [FoldoutGroup("Settings")]
    [SerializeField]
    private bool createPoles = true;

    [FoldoutGroup("Settings")]
    [SerializeField, ShowIf("createPoles")]
    private string poleSuffix = "_Pole";

    [ButtonGroup]
    [Button("Setup FABRIK", ButtonSizes.Large)]
    [GUIColor(0, 1, 0)]
    public void AutoSetupFABRIK()
    {
        // Find all leaf bones (bones with no children)
        var transforms = GetComponentsInChildren<Transform>();
        foreach (var transform in transforms)
        {
            // Skip the root and bones that have children
            if (transform == this.transform || transform.childCount > 0)
                continue;

            // Count chain length up to maxChainLength
            int chainLength = 0;
            Transform current = transform;
            while (current != null && current != this.transform && chainLength < maxChainLength)
            {
                chainLength++;
                current = current.parent;
            }

            // Skip if chain is too short
            if (chainLength < 2)
                continue;

            // Add FabricResolver if it doesn't exist
            var resolver = transform.GetComponent<FabricResolver>();
            if (resolver == null)
            {
                resolver = transform.gameObject.AddComponent<FabricResolver>();
                resolver.chainLength = chainLength;

                // Create target if needed
                if (createTargets)
                {
                    var targetGo = new GameObject(transform.name + targetSuffix);
                    targetGo.transform.position = transform.position;
                    targetGo.transform.rotation = transform.rotation;

                    // Parent the target to the same parent as the chain root
                    current = transform;
                    for (int i = 0; i < chainLength; i++)
                        current = current.parent;
                    targetGo.transform.parent = current;

                    resolver.Target = targetGo.transform;

                    // Create pole target (optional)
                    if (createPoles)
                    {
                        var poleGo = new GameObject(transform.name + poleSuffix);
                        poleGo.transform.position = transform.position + transform.forward * 0.5f;
                        poleGo.transform.parent = current;
                        resolver.Pole = poleGo.transform;
                    }

                    #if UNITY_EDITOR
                    Selection.activeGameObject = targetGo;
                    #endif
                }

                Debug.Log($"Added FabricResolver to {transform.name} with chain length {chainLength}");
            }
        }
    }

    [ButtonGroup]
    [Button("Clear Setup", ButtonSizes.Large)]
    [GUIColor(1, 0.5f, 0)]
    public void ClearFABRIKSetup()
    {
        // Remove all FabricResolvers and their targets
        var resolvers = GetComponentsInChildren<FabricResolver>();
        foreach (var resolver in resolvers)
        {
            if (resolver.Target != null && resolver.Target.name.EndsWith(targetSuffix))
                DestroyImmediate(resolver.Target.gameObject);
            if (resolver.Pole != null && resolver.Pole.name.EndsWith(poleSuffix))
                DestroyImmediate(resolver.Pole.gameObject);
            DestroyImmediate(resolver);
        }

        Debug.Log("Cleared all FABRIK setups");
    }
}
