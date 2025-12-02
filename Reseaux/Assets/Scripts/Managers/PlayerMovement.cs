using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float jumpForce = 5f;

    [Header("Step")]
    [SerializeField] private GameObject stepRayUpper;
    [SerializeField] private GameObject stepRayLower;
    [SerializeField] private float stepHeight = 0.4f;
    [SerializeField] private float stepSmooth = 0.1f;

    [Header("Sprint")] 
    [SerializeField] private Slider sprintBarr;
    [SerializeField] private float sprintBarrValue;
    
    [Header("Camera Control")]
    [SerializeField] private float lookSensitivity = 2f;
    [SerializeField] private float rotationSmoothness = 10f;
    
    [SerializeField] private Animator ar;

    private Rigidbody rb;
    private Vector3 moveDirection;
    private Vector3 direction;

    private bool isHiding = false;
    private float baseSpeed;
    private bool canSprint = true;
    
    private NetworkVariable<bool> isRunning = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
        );


    private void Awake()
    {
        stepRayUpper.transform.position = new Vector3(stepRayUpper.transform.position.x, stepHeight,
            stepRayUpper.transform.position.z);
		if(sprintBarr != null)
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
    }

    private void FixedUpdate()
    {
        Move();
        RotatePlayerToCamera();
        
        if (Input.GetKeyDown(KeyCode.Q))
        {
            isHiding = !isHiding;
        }

		if(sprintBarr != null)
		{
        	canSprint = sprintBarr.value >= sprintBarrValue;
		}
        stepClimb();
    }

    private void MoveInput()
    {
        if (isHiding)
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
        if (isHiding)
        {
            rb.linearVelocity = new Vector3(0f , rb.linearVelocity.y, 0f);  
            return;
        }
        if (direction.magnitude > 0.1f)
        {
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
        if (isHiding) return;

        if (sprintBarr == null)
        {
            moveSpeed = baseSpeed;
            return;
        }

        if (!canSprint)
            return;
        
        Camera camera = Camera.main;

        bool wantsToSprint = Input.GetKey(KeyCode.LeftShift);

        if (wantsToSprint && canSprint)
        {
            moveSpeed = sprintSpeed;
            sprintBarr.value -= 20f * Time.deltaTime;
        }
        else
        {
            moveSpeed = baseSpeed;
            sprintBarr.value += 10f * Time.deltaTime; 
        }
        sprintBarr.value = Mathf.Clamp(sprintBarr.value, 0f, sprintBarr.maxValue);
        
        float targetFOV = wantsToSprint && canSprint ? 100f : 80f;
        camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, targetFOV, Time.deltaTime * 6f);
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
    
    void stepClimb()
    {
        RaycastHit hitLower;
        if (Physics.Raycast(stepRayLower.transform.position, transform.TransformDirection(Vector3.forward), out hitLower, 0.1f))
        {
            RaycastHit hitUpper;
            if (!Physics.Raycast(stepRayUpper.transform.position, transform.TransformDirection(Vector3.forward), out hitUpper, 0.2f))
            {
                rb.position -= new Vector3(0f, -stepSmooth * Time.deltaTime, 0f);
            }
        }

        RaycastHit hitLower45;
        if (Physics.Raycast(stepRayLower.transform.position, transform.TransformDirection(1.5f,0,1), out hitLower45, 0.1f))
        {

            RaycastHit hitUpper45;
            if (!Physics.Raycast(stepRayUpper.transform.position, transform.TransformDirection(1.5f,0,1), out hitUpper45, 0.2f))
            {
                rb.position -= new Vector3(0f, -stepSmooth * Time.deltaTime, 0f);
            }
        }

        RaycastHit hitLowerMinus45;
        if (Physics.Raycast(stepRayLower.transform.position, transform.TransformDirection(-1.5f,0,1), out hitLowerMinus45, 0.1f))
        {

            RaycastHit hitUpperMinus45;
            if (!Physics.Raycast(stepRayUpper.transform.position, transform.TransformDirection(-1.5f,0,1), out hitUpperMinus45, 0.2f))
            {
                rb.position -= new Vector3(0f, -stepSmooth * Time.deltaTime, 0f);
            }
        }
    }
}