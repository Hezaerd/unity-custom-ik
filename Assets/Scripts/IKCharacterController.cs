using UnityEngine;
using Sirenix.OdinInspector;

public class IKCharacterController : MonoBehaviour
{
    #region IK
    [FoldoutGroup("IK")]
    [FoldoutGroup("IK/Head")]
    [SerializeField] private IK.CCDResolver IKHead;
    [FoldoutGroup("IK/Left Hand")]
    [SerializeField] private IK.CCDResolver IKLeftHand;
    [FoldoutGroup("IK/Right Hand")]
    [SerializeField] private IK.CCDResolver IKRightHand;
    [FoldoutGroup("IK/Left Foot")]
    [SerializeField] private IK.CCDResolver IKLeftFoot;
    [FoldoutGroup("IK/Right Foot")]
    [SerializeField] private IK.CCDResolver IKRightFoot;

    [FoldoutGroup("IK/Head")]
    [SerializeField] private GameObject IKTargetHead;
    [FoldoutGroup("IK/Left Hand")]
    [SerializeField] private GameObject IKTargetLeftHand;
    [FoldoutGroup("IK/Right Hand")]
    [SerializeField] private GameObject IKTargetRightHand;
    [FoldoutGroup("IK/Left Foot")]
    [SerializeField] private GameObject IKTargetLeftFoot;
    [FoldoutGroup("IK/Right Foot")]
    [SerializeField] private GameObject IKTargetRightFoot;
    #endregion IK



    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
