using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 10f;
    
    private GameObject lastHitObject;

    private NetworkVariable<PlayerData> playerData = new(
        new PlayerData { life = 100, stunt = false },
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );
    private NetworkVariable<Color> playerColor = new(
        Color.white,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private Renderer _renderer;
    private Vector3 direction;

    public override void OnNetworkSpawn()
    {
        _renderer = GetComponentInChildren<Renderer>();
        playerColor.OnValueChanged += (oldColor, newColor) =>
        {
            _renderer.material.color = newColor;
        };

        if (IsServer)
        {
            if (IsHost)
                playerColor.Value = Color.green;
            if (OwnerClientId == 1)
                playerColor.Value = Color.blue;
        }

        _renderer.material.color = playerColor.Value;

        if (IsOwner)
        {
            FollowCamera cam = Camera.main.GetComponent<FollowCamera>();
            if (cam != null)
            {
                cam.SetTarget(transform);
            }
        }
    }

    void Update()
    {
        if (!IsOwner)
            return;
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        direction = new Vector3(moveX, 0f, moveY).normalized;

        if (direction.magnitude >= 0.1f)
        {
            Vector3 moveDir = Quaternion.Euler(0f, Camera.main.transform.eulerAngles.y, 0f) * direction;
            transform.position += moveDir * moveSpeed * Time.deltaTime;
            transform.forward = moveDir;
        }
        
        Vector3 cameraForward = Camera.main.transform.forward; 
        Debug.DrawRay(Camera.main.transform.position, cameraForward * 7, Color.green);
        if (Physics.Raycast( Camera.main.transform.position, cameraForward, out RaycastHit hit, 7))
        {
            if (hit.collider.CompareTag("ObjectToTransform"))
            {
                GameObject hitObject = hit.collider.gameObject;
                if (hitObject != lastHitObject)
                {
                    if (lastHitObject != null)
                    {
                        lastHitObject.GetComponent<Renderer>().material.color = Color.white;
                    }
                    hitObject.GetComponent<Renderer>().material.color = Color.cyan;
                    lastHitObject = hitObject;

                    Debug.Log("Touché : " + hitObject.name);
                }
            }
            
        }
        else
        {
            if (lastHitObject != null)
            {
                lastHitObject.GetComponent<Renderer>().material.color = Color.white;
                lastHitObject = null;
            }

            Debug.Log("out");
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TestRpc();
        }
    }

    [Rpc(SendTo.Server)]
    void TestRpc()
    {
        Debug.Log("TestRpc");
    }
}

public struct PlayerData : INetworkSerializable
{
    public int life;
    public bool stunt;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref life);
        serializer.SerializeValue(ref stunt);
    }
}
