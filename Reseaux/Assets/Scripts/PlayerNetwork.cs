using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private KeyCode left = KeyCode.A;
    [SerializeField] private KeyCode right = KeyCode.D;
    [SerializeField] private KeyCode down = KeyCode.S;
    [SerializeField] private KeyCode up = KeyCode.W;

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
            {
                playerColor.Value = Color.blue; 
            }
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

        direction = Vector3.zero;
        if (Input.GetKey(left)) direction.x = -1f;
        if (Input.GetKey(right)) direction.x = 1f;
        if (Input.GetKey(down)) direction.z = -1f;
        if (Input.GetKey(up)) direction.z = 1f;

        direction = direction.normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

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
