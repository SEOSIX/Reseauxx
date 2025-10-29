using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float sprintSpeed = 15f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float fallSpeed = 10f;
    [SerializeField] private bool canUseHeadBob = true;

    [Header("Detection")]
    public float minJumpHeight = 1.5f;
    public LayerMask ground;
    
    [Header("HeadBobParameter")] 
    [SerializeField] private float walkBobSpeed = 14f;
    [SerializeField] private float walkBobAmount = 0.5f;
    [SerializeField] private float sprintBobSpeed = 18f;
    [SerializeField] private float sprintBobAmount = 1f;
    private float defaultYPos = 0f;
    private float timer;

    private Rigidbody rb;
    [SerializeField] private bool canJump = true;
    private Camera playerCamera;
    private Vector3 moveDirection;
    private Vector3 direction;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();
        rb.freezeRotation = true;
        playerCamera = Camera.main;

        defaultYPos = playerCamera.transform.position.y;
    }

    private void Update()
    {
        if (!IsOwner) return;
   
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        Move();
        Sprint();
    }

    private void Move()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        
        direction = new Vector3(moveX, 0, moveY).normalized;
        if (direction.magnitude > 0.1f)
        {
            moveDirection = Quaternion.Euler(0, playerCamera.transform.eulerAngles.y, 0) * direction;
            Vector3 move = moveDirection * moveSpeed * Time.deltaTime;
            rb.MovePosition(rb.position + move);
            transform.forward = moveDirection;
            
        }
    }

    private void Sprint()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            moveSpeed = sprintSpeed;
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, 80f, Time.deltaTime * 10f);
        }
        else
        {
            moveSpeed = 10f;
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, 70f, Time.deltaTime * 10f);
        }
    }
}
