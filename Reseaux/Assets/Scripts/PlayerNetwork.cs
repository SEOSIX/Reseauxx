using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Renderer playerRenderer;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PropMorpher propMorpher;

    [Header("Stats")]
    [SerializeField] private float baseLife = 100f;

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

    public override void OnNetworkSpawn()
    {
        if (playerRenderer == null)
            playerRenderer = GetComponentInChildren<Renderer>();

        if (movement == null)
            movement = GetComponent<PlayerMovement>();

        if (propMorpher == null)
            propMorpher = GetComponent<PropMorpher>();

        playerColor.OnValueChanged += (oldColor, newColor) =>
        {
            if (playerRenderer != null)
                playerRenderer.material.color = newColor;
        };

        if (IsServer)
        {
            if (IsHost)
                playerColor.Value = Color.green;
            else
                playerColor.Value = Color.blue;
        }

        if (playerRenderer != null)
            playerRenderer.material.color = playerColor.Value;

        if (IsOwner)
        {
            if (movement != null)
                movement.enabled = true;

            if (propMorpher != null)
                propMorpher.enabled = true;

            FollowCamera cam = Camera.main?.GetComponent<FollowCamera>();
            if (cam != null)
                cam.SetTarget(transform);
        }
        else
        {
            if (movement != null)
                movement.enabled = false;

            if (propMorpher != null)
                propMorpher.enabled = false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetRoleServerRpc(string newTag)
    {
        gameObject.tag = newTag;
        SetRoleClientRpc(newTag);
    }

    [ClientRpc]
    private void SetRoleClientRpc(string newTag)
    {
        gameObject.tag = newTag;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ModifyLifeServerRpc(float amount)
    {
        PlayerData data = playerData.Value;
        data.life = Mathf.Clamp(data.life + (int)amount, 0, (int)baseLife);
        playerData.Value = data;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = IsOwner ? Color.cyan : Color.gray;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 2, 0.25f);
    }
#endif
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
