using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float jumpForce = 5f;
    
    
    [Header("WallRunning")]
    public LayerMask wall;
    public LayerMask ground;
    public float wallRunForce = 10f;
    public float maxWallRunTime = 1.5f;
    private float wallRunTimer;

    [Header("Detection")]
    public float wallCheckDistance = 0.6f;
    public float minJumpHeight = 1.5f;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    public Transform oritentation;
    private bool wallLeft;
    private bool wallRight;

    private Rigidbody rb;
    private bool canJump = true;
    private bool isWallRunning = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        rb.freezeRotation = true;
    }

    private void Update()
    {
        if (!IsOwner) return;

        CheckForWall();
        StateMachine();

        if (Input.GetKeyDown(KeyCode.Space) && canJump && !isWallRunning)
        {
            Jump();
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        Move();
        if (isWallRunning)
            WallRunningMovement();
    }

    private void Move()
    {
        if (isWallRunning) return;

        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(moveX, 0, moveY).normalized;
        if (direction.magnitude > 0.1f)
        {
            Vector3 moveDir = (Quaternion.FromToRotation(Vector3.up, transform.up) * direction);
            Vector3 move = moveDir * moveSpeed * Time.deltaTime;
            rb.MovePosition(rb.position + move);
            transform.forward = moveDir;
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        canJump = false;
        Invoke(nameof(ResetJump), 0.3f);
    }

    private void ResetJump()
    {
        canJump = true;
    }

    private void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, oritentation.right, out rightWallHit, wallCheckDistance, wall);
        wallLeft = Physics.Raycast(transform.position, -oritentation.right, out leftWallHit, wallCheckDistance, wall);
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, ground);
    }

    private void StateMachine()
    {
        float moveY = Input.GetAxis("Vertical");
        if ((wallLeft || wallRight) && moveY > 0 && AboveGround())
        {
            if (!isWallRunning)
                StartWallRun();
        }
        else if (isWallRunning)
        {
            StopWallRun();
        }
    }

    private void StartWallRun()
    {
        isWallRunning = true;
        wallRunTimer = maxWallRunTime;
        rb.useGravity = false;
    }

    private void WallRunningMovement()
    {
        wallRunTimer -= Time.deltaTime;
        if (wallRunTimer <= 0)
        {
            StopWallRun();
            return;
        }

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, Vector3.up);

        if (Vector3.Dot(oritentation.forward, wallForward) < 0)
            wallForward = -wallForward;
        rb.linearVelocity = new Vector3(wallForward.x * wallRunForce, rb.linearVelocity.y, wallForward.z * wallRunForce);
        rb.AddForce(-wallNormal * 50f, ForceMode.Force);
        rb.AddForce(Vector3.down * 5f, ForceMode.Acceleration);
    }

    private void StopWallRun()
    {
        isWallRunning = false;
        rb.useGravity = true;
    }
}
