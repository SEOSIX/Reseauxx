using System;
using System.Numerics;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float sprintSpeed = 15f;
    
    [Header("Camera Control")]
    [SerializeField] private float lookSensitivity = 2f;
    [SerializeField] private float rotationSmoothness = 10f;
    
    [Header("Jump Control")]
    [SerializeField] private float jumpForce= 20f;
    [SerializeField] private float maxHeight;
    [SerializeField] private float gravityForce;
    [SerializeField] private float fallingForce;
    [SerializeField] private LayerMask layerMask;
    
    
    
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
        Jump(); 
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
        
        Vector3 rot = Gun.instance.zoneToInstanciate.transform.eulerAngles;
        rot.y = targetRotation.eulerAngles.y;
        if (Gun.instance != null)
            Gun.instance.zoneToInstanciate.transform.eulerAngles = rot;
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

    private bool isJumping = false;
    private float startY;

    private void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isJumping = true;
            startY = transform.position.y;
            Debug.Log("Jumping");
        }

        if (isJumping)
        {
            if (transform.position.y < startY + maxHeight)
                rb.AddForce(Vector3.up * jumpForce * Time.deltaTime, ForceMode.Impulse);
            else
            {
                rb.AddForce(Vector3.down * gravityForce * Time.deltaTime, ForceMode.Impulse);
                isJumping = false;
            }
        }
    }


    private bool ISGrounded()
    {
        RaycastHit hit;
        Vector3 origin = transform.position;
        float distance = 0.2f;
        Vector3 direction = Vector3.down; 
        if (Physics.Raycast(origin, direction, out hit, distance, layerMask))
        {
            return true;
        }
        return false;
    }
}
