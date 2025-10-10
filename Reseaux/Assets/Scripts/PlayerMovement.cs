using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float jumpForce = 5f;

    private Rigidbody rb;
    private bool canJump = true;

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

        Move();

        if (Input.GetKeyDown(KeyCode.Space) && canJump)
        {
            Jump();
        }
        if (rb.linearVelocity.y < 0)
        {
            rb.AddForce(Vector3.down * 20f, ForceMode.Acceleration);
        }
    }

    private void Move()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(moveX, 0, moveY).normalized;
        if (direction.magnitude > 0.1f)
        {
            Vector3 moveDir = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0) * direction;
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
    }

    private void OnCollisionEnter(Collision collision)
    {
        canJump = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        canJump = false;
    }
}
