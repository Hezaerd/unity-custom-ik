using UnityEngine;
using Sirenix.OdinInspector;

public class ThirdPersonCamera : MonoBehaviour
{
    [FoldoutGroup("References")]
    [SerializeField] private Transform target;

    [FoldoutGroup("Follow Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0, 2, -5);
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float rotationSpeed = 2f;

    [FoldoutGroup("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minZoomDistance = 2f;
    [SerializeField] private float maxZoomDistance = 10f;
    [SerializeField] private float currentZoomDistance;

    [FoldoutGroup("Collision")]
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 5f;
    [SerializeField] private float collisionRadius = 0.2f;
    [SerializeField] private LayerMask collisionLayers;

    [FoldoutGroup("Constraints")]
    [SerializeField] private Vector2 verticalRotationLimits = new Vector2(-30f, 60f);
    [SerializeField] private float currentVerticalRotation;

    private Vector3 currentVelocity;
    private float targetDistance;

    private void Start()
    {
        if (target == null)
        {
            Debug.LogError("No target assigned to ThirdPersonCamera!");
            enabled = false;
            return;
        }

        // Initialize position and rotation
        currentZoomDistance = offset.magnitude;
        transform.position = target.position + offset.normalized * currentZoomDistance;
        transform.LookAt(target.position);
        targetDistance = currentZoomDistance;
        currentVerticalRotation = transform.eulerAngles.x;

        // Ensure cursor starts invisible
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        HandleZoom();
        HandleRotation();
        HandlePosition();
        HandleCollision();
    }

    private void HandleZoom()
    {
        // Get scroll wheel input
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollInput) > 0.01f)
        {
            // Update zoom distance with scroll input
            currentZoomDistance = Mathf.Clamp(
                currentZoomDistance - scrollInput * zoomSpeed,
                minZoomDistance,
                maxZoomDistance
            );

            // Update offset magnitude while maintaining direction
            offset = offset.normalized * currentZoomDistance;
        }
    }

    private void HandleRotation()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        // Apply vertical rotation with constraints
        currentVerticalRotation -= mouseY;
        currentVerticalRotation = Mathf.Clamp(currentVerticalRotation, verticalRotationLimits.x, verticalRotationLimits.y);

        // Rotate around target
        transform.RotateAround(target.position, Vector3.up, mouseX);

        // Apply vertical rotation
        Vector3 direction = (transform.position - target.position).normalized;
        float distance = currentZoomDistance;

        Quaternion rotation = Quaternion.Euler(currentVerticalRotation, transform.eulerAngles.y, 0);
        Vector3 targetPosition = target.position + rotation * Vector3.back * distance;

        transform.position = targetPosition;
        transform.LookAt(target.position);
    }

    private void HandlePosition()
    {
        // Calculate desired position using current zoom distance
        Vector3 desiredPosition = target.position + transform.rotation * (offset.normalized * currentZoomDistance);

        // Smoothly move to desired position
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref currentVelocity,
            1f / followSpeed
        );
    }

    private void HandleCollision()
    {
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        // Check for obstacles between camera and target
        if (Physics.SphereCast(
            target.position,
            collisionRadius,
            -directionToTarget,
            out RaycastHit hit,
            maxDistance,
            collisionLayers))
        {
            // Adjust camera position to avoid collision
            float newDistance = Mathf.Clamp(hit.distance, minDistance, maxDistance);
            transform.position = target.position - directionToTarget * newDistance;

            // Update zoom distance if collision forces us closer
            if (newDistance < currentZoomDistance)
                currentZoomDistance = newDistance;
        }
    }

    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (target == null) return;

        // Draw camera bounds
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, collisionRadius);

        // Draw target connection
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, target.position);

        // Draw collision check
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(target.position, collisionRadius);
        Gizmos.DrawLine(
            target.position,
            target.position + (transform.position - target.position).normalized * maxDistance
        );

        // Draw zoom limits
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(target.position, minZoomDistance);
        Gizmos.DrawWireSphere(target.position, maxZoomDistance);
    }
    #endif
}