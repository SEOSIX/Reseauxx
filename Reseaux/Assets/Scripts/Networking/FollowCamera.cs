using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [Header("Target")]
    private Transform target;

    [Header("Distance & Height")]
    public float distance;
    public float height;
    public float xAxis;

    [Header("Rotation Camera")]
    public float mouseSensitivity = 100f;
    private float yaw;
    private float pitch;
    public float pitchMin = -40f;
    public float pitchMax = 85f;

    public Transform cameraTransform;

    [Header("Anti-Clipping")]
    public float rayRadius = 0.2f;   
    public LayerMask collisionMask;  

    private float currentDistance;

    void Start()
    {
        currentDistance = distance;
    }

    void LateUpdate()
    {
        if (target == null) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPosition =
            target.position
            - rotation * Vector3.forward * distance
            + Vector3.up * height
            + Vector3.right * xAxis;

        Vector3 direction = desiredPosition - target.position;
        float desiredDistance = direction.magnitude;

        if (Physics.SphereCast(target.position, rayRadius, direction.normalized, out RaycastHit hit, desiredDistance, collisionMask))
        {
            currentDistance = hit.distance;
        }
        else
        {
            currentDistance = Mathf.Lerp(currentDistance, distance, Time.deltaTime * 5f);
        }

        Vector3 finalPosition =
            target.position
            - rotation * Vector3.forward * currentDistance
            + Vector3.up * height
            + Vector3.right * xAxis;
        transform.rotation = rotation;
        transform.position = finalPosition;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
