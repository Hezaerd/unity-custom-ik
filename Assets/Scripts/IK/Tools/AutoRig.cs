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
    [SerializeField]
    private string targetSuffix = "_Target";

    [FoldoutGroup("Settings")]
    [SerializeField]
    private IKType ikType = IKType.FABRIK;

    private enum IKType
    {
        FABRIK,
        CCD
    }

    [ButtonGroup]
    [Button("Setup IK", ButtonSizes.Large)]
    [GUIColor(0, 1, 0)]
    public void AutoSetupIK()
    {
        // Create a container for targets if it doesn't exist
        GameObject targetsContainer = GameObject.Find("IK_Targets");
        if (targetsContainer == null)
            targetsContainer = new GameObject("IK_Targets");

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

            // Add resolver based on type
            Component resolver = null;
            switch (ikType)
            {
                case IKType.FABRIK:
                    var fabricResolver = transform.gameObject.AddComponent<FabricResolver>();
                    fabricResolver.chainLength = chainLength;
                    resolver = fabricResolver;
                    break;

                case IKType.CCD:
                    var ccdResolver = transform.gameObject.AddComponent<CCDResolver>();
                    ccdResolver.chainLength = chainLength;
                    resolver = ccdResolver;
                    break;
            }

            if (resolver == null) continue;

            // Create target if needed
            if (createTargets)
            {
                var targetGo = new GameObject($"{transform.name}{targetSuffix}");
                targetGo.transform.position = transform.position;
                targetGo.transform.rotation = transform.rotation;
                targetGo.transform.parent = targetsContainer.transform;

                // Assign target based on resolver type
                if (resolver is FabricResolver fabric)
                    fabric.Target = targetGo.transform;
                else if (resolver is CCDResolver ccd)
                    ccd.Target = targetGo.transform;

                #if UNITY_EDITOR
                Selection.activeGameObject = targetGo;
                #endif
            }

            Debug.Log($"Added {ikType} resolver to {transform.name} with chain length {chainLength}");
        }
    }

    [ButtonGroup]
    [Button("Clear Setup", ButtonSizes.Large)]
    [GUIColor(1, 0.5f, 0)]
    public void ClearIKSetup()
    {
        // Remove all resolvers and their targets
        var fabricResolvers = GetComponentsInChildren<FabricResolver>();
        var ccdResolvers = GetComponentsInChildren<CCDResolver>();

        // Find and destroy the targets container
        var targetsContainer = GameObject.Find("IK_Targets");
        if (targetsContainer != null)
            DestroyImmediate(targetsContainer);

        // Remove resolvers
        foreach (var resolver in fabricResolvers)
            DestroyImmediate(resolver);

        foreach (var resolver in ccdResolvers)
            DestroyImmediate(resolver);

        Debug.Log("Cleared all IK setups");
    }
}
