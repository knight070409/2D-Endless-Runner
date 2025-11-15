using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private Transform target; 
    [SerializeField] private Vector3 offset = new Vector3(3f, 2f, -10f);
    [SerializeField] private float smoothSpeed = 5f;

    [Header("Follow Constraints")]
    [SerializeField] private bool followX = true;
    [SerializeField] private bool followY = true;
    [SerializeField] private bool clampY = true;
    [SerializeField] private float minY = 0f;
    [SerializeField] private float maxY = 10f;

    [Header("Look Ahead")]
    [SerializeField] private bool useLookAhead = true;
    [SerializeField] private float lookAheadDistance = 2f;

    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPos = target.position + offset;

        // Add look ahead based on player velocity
        if (useLookAhead)
        {
            Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
            if (targetRb != null)
            {
                targetPos.x += targetRb.linearVelocity.x * lookAheadDistance * 0.1f;
            }
        }

        // Apply follow constraints
        Vector3 desiredPosition = transform.position;

        if (followX)
            desiredPosition.x = targetPos.x;

        if (followY)
            desiredPosition.y = targetPos.y;

        // Clamp Y position if needed
        if (clampY)
        {
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
        }

        // Keep Z position (camera depth)
        desiredPosition.z = offset.z;

        // Smooth follow
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, 1f / smoothSpeed);
    }

    // Call this to set the target at runtime
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
