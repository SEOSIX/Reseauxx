using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float jumpForce = 5f;

    [Header("Sprint")] 
    [SerializeField] private Slider sprintBarr;
    [SerializeField] private float sprintBarrValue;
    public float minToSprint;
    
    
    [Header("Camera Control")]
    [SerializeField] private float lookSensitivity = 2f;
    [SerializeField] private float rotationSmoothness = 10f;
    
    [SerializeField] private Animator ar;

    private Rigidbody rb;
    private Vector3 moveDirection;
    private Vector3 direction;


    public bool canWalk = true;
    private bool isHiding = false;
    private float baseSpeed;
    private bool canSprint = true;
    private bool isGrounded;
    
    private NetworkVariable<bool> isRunning = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
        );

    
    private bool CanMove()
    {
        return canWalk && !isHiding;
    }

    private void Awake()
    {
        if (sprintBarr != null)
        {
            sprintBarr.maxValue = sprintBarrValue;
            sprintBarr.value = sprintBarr.maxValue;
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.isKinematic = false;
        isRunning.OnValueChanged += (oldVal, newVal) =>
        {
            UpdateAnimator(newVal);
        };
        baseSpeed = moveSpeed;
    }

    private void Update()
    {
        if (!IsOwner) return;
        MoveInput();
        Sprint();
        if (Input.GetMouseButtonDown(0) && canWalk)
        {
            AttackServerRpc();
        }
    }

    private void FixedUpdate()
    {
        Move();
        RotatePlayerToCamera();
        
        if (Input.GetKeyDown(KeyCode.Q))
        {
            isHiding = !isHiding;
        }
    }

    private void MoveInput()
    {
        if (isHiding || !canWalk)
            return;
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        direction = new Vector3(moveX, 0, moveY).normalized;

        bool running = direction.magnitude > 0.1f;
        if (running != isRunning.Value)
        {
            SetRunningServerRpc(running);
        }
    }

    private void Move()
    {
        Vector3 currentVel = rb.linearVelocity;
        if (isHiding || !canWalk)
        {
            rb.linearVelocity = new Vector3(0f , rb.linearVelocity.y, 0f);  
            return;
        }
        if (direction.magnitude > 0.1f)
        {
            UpdateAnimator(true);
            Vector3 moveDir = transform.TransformDirection(direction);
            rb.linearVelocity = new Vector3(
                moveDir.x * moveSpeed,
                currentVel.y,               
                moveDir.z * moveSpeed
            );
        }
        else
        {
            rb.linearVelocity = new Vector3(0f , rb.linearVelocity.y, 0f);  
        }
    }

    private void RotatePlayerToCamera()
    {
        if (!IsOwner)
            return;
        if (isHiding || Camera.main == null) return;

        float targetYaw = Camera.main.transform.eulerAngles.y;
        Quaternion targetRotation = Quaternion.Euler(0, targetYaw, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSmoothness);
    }

    private void Sprint()
    {
        bool isSprinting = false;
        if (isHiding) return;
        if (sprintBarr == null)
        {
            moveSpeed = baseSpeed;
            return;
        }
        
        if (sprintBarr.value <= 0.01f)
            canSprint = false;

        if (!canSprint && sprintBarr.value >= minToSprint)
            canSprint = true;

        bool isTryingToSprint = Input.GetKey(KeyCode.LeftShift);
        
        if (isTryingToSprint && canSprint)
        {
            moveSpeed = sprintSpeed;
            sprintBarr.value -= 5f * Time.deltaTime;
            if (sprintBarr.value <= 0 && canSprint)
            {
                canSprint = false;
                moveSpeed = baseSpeed;
            }
        }
        else
        {
            if (sprintBarr.value != 1f)
            {
                sprintBarr.value += 5 * Time.deltaTime;
            }
            moveSpeed = baseSpeed;
        }

        sprintBarr.value = Mathf.Clamp(sprintBarr.value, 0f, sprintBarr.maxValue);

        float targetFOV = (isTryingToSprint && canSprint && sprintBarr.value > 0) ? 100f : 80f;
        Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, targetFOV, Time.deltaTime * 6f);
    }
    private void UpdateAnimator(bool running)
    {
        if (ar == null) return;
        ar.SetBool("Run", running);
        ar.SetBool("Idle", !running);
    }

    [ServerRpc]
    private void SetRunningServerRpc(bool running)
    {
        isRunning.Value = running;
    }

    [ServerRpc]
    private void AttackServerRpc()
    {
        PlaySlashClientRpc();
    }
    
    [ClientRpc]
    private void PlaySlashClientRpc()
    {
        if (!canWalk) return;
        if (ar == null) return;
        canWalk = false;
        ar.SetTrigger("Slash");
    }
}