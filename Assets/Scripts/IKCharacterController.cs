using UnityEngine;
using Sirenix.OdinInspector;

public class IKCharacterController : MonoBehaviour
{
    #region References
    [FoldoutGroup("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Animator animator;
    [SerializeField] private ThirdPersonCamera thirdPersonCamera;
    #endregion

    #region Movement Settings
    [FoldoutGroup("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpForce = 5f;

    private Vector3 velocity;
    private bool isGrounded;
    #endregion

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
    [SerializeField] private Transform IKTargetHead;
    [FoldoutGroup("IK/Left Hand")]
    [SerializeField] private Transform IKTargetLeftHand;
    [FoldoutGroup("IK/Right Hand")]
    [SerializeField] private Transform IKTargetRightHand;
    [FoldoutGroup("IK/Left Foot")]
    [SerializeField] private Transform IKTargetLeftFoot;
    [FoldoutGroup("IK/Right Foot")]
    [SerializeField] private Transform IKTargetRightFoot;

    [FoldoutGroup("IK/Foot Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float footHeight = 0.1f;
    [SerializeField] private float raycastDistance = 1f;
    #endregion

    private void Start()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (IKHead == null || IKLeftHand == null || IKRightHand == null || IKLeftFoot == null || IKRightFoot == null)
            Debug.LogError("No IK targets set for IKCharacterController!");

        if (thirdPersonCamera == null)
            thirdPersonCamera = FindObjectOfType<ThirdPersonCamera>();
    }

    private void Update()
    {
        HandleMovement();
        UpdateIKTargets();
    }

    private void HandleMovement()
    {
        // Check if grounded
        isGrounded = characterController.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // Get input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Calculate movement direction
        Vector3 movement = new Vector3(horizontal, 0f, vertical).normalized;

        if (movement.magnitude >= 0.1f)
        {
            // Get camera forward direction
            Vector3 cameraForward = thirdPersonCamera.transform.forward;
            cameraForward.y = 0f;
            cameraForward.Normalize();

            // Calculate movement relative to camera
            Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
            movement = targetRotation * movement;

            // Rotate character
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.LookRotation(movement),
                rotationSpeed * Time.deltaTime
            );
        }

        // Apply movement
        characterController.Move(movement * moveSpeed * Time.deltaTime);

        // Handle jumping
        if (Input.GetButtonDown("Jump") && isGrounded)
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);

        // Update animator if it exists
        if (animator != null)
        {
            animator.SetFloat("Speed", movement.magnitude);
            animator.SetBool("IsGrounded", isGrounded);
        }
    }

    private void UpdateIKTargets()
    {
        // Update foot IK positions based on ground
        if (IKLeftFoot != null && IKTargetLeftFoot != null)
            UpdateFootIK(IKTargetLeftFoot);

        if (IKRightFoot != null && IKTargetRightFoot != null)
            UpdateFootIK(IKTargetRightFoot);

        // Head IK can follow camera or mouse position
        if (IKHead != null && IKTargetHead != null)
        {
            Ray ray = thirdPersonCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                IKTargetHead.position = hit.point;
            }
        }
    }

    private void UpdateFootIK(Transform footTarget)
    {
        // Cast ray from above foot
        Vector3 rayStart = footTarget.position + Vector3.up * raycastDistance;
        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, raycastDistance * 2f, groundLayer))
        {
            // Position foot on ground
            Vector3 targetPosition = hit.point + Vector3.up * footHeight;
            footTarget.position = targetPosition;

            // Align foot with surface normal
            footTarget.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal)
                * transform.rotation;
        }
    }
}
