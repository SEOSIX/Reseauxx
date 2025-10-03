using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    private NetworkVariable<int> _randomNumber = new(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private KeyCode left = KeyCode.A;
    [SerializeField] private KeyCode right = KeyCode.D;
    [SerializeField] private KeyCode down = KeyCode.S;
    [SerializeField] private KeyCode up = KeyCode.W;

    private NetworkVariable<PlayerData> playerData = new(new PlayerData
    {
        life = 100,
        stunt = false,
    },NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    
    Vector3 direction;


    public override void OnNetworkSpawn()
    {
        playerData.OnValueChanged += (PlayerData previousValue , PlayerData newValue) =>
        {
            Debug.Log(OwnerClientId + "life" + newValue.life + "stunt"+ newValue.stunt);
        };
    }
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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TestRpc();
        }
        
        direction = direction.normalized;
        
        transform.position += direction * moveSpeed * Time.deltaTime;
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