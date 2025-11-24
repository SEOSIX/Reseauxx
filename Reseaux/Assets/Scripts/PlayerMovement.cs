using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float sprintSpeed = 15f;
    [SerializeField] private float jumpForce = 5f;
    
    [Header("Camera Control")]
    [SerializeField] private float lookSensitivity = 2f;
    [SerializeField] private float rotationSmoothness = 10f;
    
    [SerializeField] private Animator ar;

    private Rigidbody rb;
    private Vector3 moveDirection;
    private Vector3 direction;

    private bool isHiding = false;
    private float yaw;  
    private float pitch;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();
        rb.freezeRotation = true;
        if (!IsOwner)
        {
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;
        MoveInput();
        Sprint();
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        Move();
        RotatePlayerToCamera();
        if (Input.GetKeyDown(KeyCode.Q))
        {
            isHiding = !isHiding;
        }
    }

    private void MoveInput()
    {
        if (isHiding)
            return;
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        direction = new Vector3(moveX, 0, moveY).normalized;
    }

    private void Move()
    {
        if (isHiding)
            return;
        if (direction.magnitude > 0.1f)
        {
            moveDirection = transform.TransformDirection(direction);
            Vector3 move = moveDirection * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + move);
            if (ar != null)
            {
                ar.SetTrigger("Run");
            }
        }
        else
        {
            if (ar != null)
            {
                ar.ResetTrigger("Run");
            }
        }
    }

    private void RotatePlayerToCamera()
    {
        if (isHiding)
            return;
        if (Camera.main == null) return;

        float targetYaw = Camera.main.transform.eulerAngles.y;
        Quaternion targetRotation = Quaternion.Euler(0, targetYaw, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSmoothness);
    }

    private void Sprint()
    {
        if (isHiding)
            return;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            moveSpeed = sprintSpeed;
        }
        else
        {
            moveSpeed = 10f;
        }
    }
}
