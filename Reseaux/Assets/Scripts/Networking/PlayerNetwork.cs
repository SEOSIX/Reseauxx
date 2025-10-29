using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Renderer playerRenderer;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PropMorpher propMorpher;
    [SerializeField] private FollowCamera cameraFollow;
    
    [Header("Stats")]
    [SerializeField] private float baseLife = 100f;

    public PlayerManager playerManager;
    
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
            playerManager.RegisterPlayer(this);
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

        if (!IsOwner) return;
        
        FollowCamera cam = Camera.main?.GetComponent<FollowCamera>();
        if (gameObject.CompareTag("Seaker"))
        {
            if (cam != null)
            {
                cam.height = 0.35f;
                cam.distance = 0.47f;
            }
            gameObject.GetComponent<PropMorpher>().enabled = false;
//            Gun.instance.gameObject.SetActive(true);
            Camera.main.fieldOfView = 70;
        }
        else
        {
            gameObject.GetComponent<PropMorpher>().enabled = true;
//            Gun.instance.gameObject.SetActive(false);
            Camera.main.fieldOfView = 70;
        }
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
