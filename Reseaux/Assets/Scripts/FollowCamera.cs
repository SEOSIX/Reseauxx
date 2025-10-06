using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [Header("Target")]
    private Transform target;

    [Header("Distance & Height")]
    public float distance = 5f;
    public float height = 2f;

    [Header("Rotation")]
    public float mouseSensitivity = 100f;
    private float yaw = 0f;
    private float pitch = 0f;
    public float pitchMin = -40f;
    public float pitchMax = 85f;

    void LateUpdate()
    {
        if (target == null)
            return;
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 position = target.position - rotation * Vector3.forward * distance + Vector3.up * height;

        transform.rotation = rotation;
        transform.position = position;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
