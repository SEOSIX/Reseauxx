using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private KeyCode left = KeyCode.A;
    [SerializeField] private KeyCode right = KeyCode.D;
    [SerializeField] private KeyCode down = KeyCode.S;
    [SerializeField] private KeyCode up = KeyCode.W;

    
    Vector3 direction;
    void Update()
    {
        if (!IsOwner)
        {
            return;
        }
        
        direction = Vector3.zero;
        if (Input.GetKey(left))
        {
            direction.x = -1f;
        }
        if (Input.GetKey(right))
        {
            direction.x = 1f;
        }
        if (Input.GetKey(down))
        {
            direction.z = -1f;
        }
        if (Input.GetKey(up))
        {
            direction.z = 1f;
        }
        
        direction = direction.normalized;
        
        transform.position += direction * moveSpeed * Time.deltaTime;
    }
}
