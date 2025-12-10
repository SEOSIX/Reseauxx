using System;
using System.Collections;
using DefaultNamespace;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Renderer playerRenderer;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PropMorpher propMorpher;
    [SerializeField] private FollowCamera cameraFollow;
    [SerializeField] private GameObject EXPLOSION;
    
    [Header("Stats")]
    [SerializeField] private float baseLife = 100f;

    public PlayerManager playerManager;
    
    private NetworkVariable<PlayerData> playerData = new(
        new PlayerData { life = 100, stunt = false },
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );
    
    private NetworkVariable<int> playerLife = new NetworkVariable<int>(
        20,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    
    private NetworkVariable<FixedString32Bytes> playerPseudo = new(
        "Unknown",
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    
    private NetworkVariable<float> elapsedTime = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    
    private bool clockRunning = false;
    
    public string PlayerPseudo => playerPseudo.Value.ToString();
    public int CurrentLife => playerLife.Value;


    public override void OnNetworkSpawn()
    {
        playerPseudo.OnValueChanged += (oldPseudo, newPseudo) =>
        {
            StartCoroutine(PseudoManager.instance.DebugDislayConnexion(newPseudo.ToString()));
        };
       
        playerLife.OnValueChanged += (oldLife, newLife) =>
        {
            OnLifeChanged(oldLife, newLife);
        };

        elapsedTime.OnValueChanged += (oldTime, newTime) =>
        {
            Clock.instance.UpdateTimer(newTime);
        };
        
        if (IsServer)
        {
            playerLife.Value = (int)baseLife;
            playerManager.RegisterPlayer(this);
            StartCoroutine(ServerClockLoop());
        }
        
        if (playerRenderer == null)
            playerRenderer = GetComponentInChildren<Renderer>();
        
        if (IsOwner)
        {
            FollowCamera cam = Camera.main?.GetComponent<FollowCamera>();
            if (cam != null)
                cam.SetTarget(transform);
            ApplyRoleCameraSettings();
           
            if (LifeManager.instance != null)
            {
                LifeManager.instance.SetLife(playerLife.Value);
            }
        }
    }
   
    private void OnLifeChanged(int oldLife, int newLife)
    {
        if (IsOwner && LifeManager.instance != null)
        {
            LifeManager.instance.SetLife(newLife);
            LifeManager.instance.CheckLifePlayer();
        }
    }
    
   
    
    private IEnumerator ServerClockLoop() 
    {
        clockRunning = true;
		Clock.instance.StartClock();
        while (clockRunning)
        {
            elapsedTime.Value += Time.deltaTime;
            yield return null;
        }
    }

    #region server RPC
    [ServerRpc(RequireOwnership = false)]
    public void SetPseudoServerRpc(string pseudo)
    {
        playerPseudo.Value = pseudo;
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damage)
    {
        int oldLife = playerLife.Value;
        playerLife.Value = Mathf.Max(0, playerLife.Value - damage);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetLifeServerRpc(int newLife)
    {
        playerLife.Value = newLife;
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void StopClockServerRpc()
    {
        clockRunning = false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyColliderServerRpc()
    {
        DesactivateColliderForOthersClientRpc();

        GameObject explosion = Instantiate(EXPLOSION, transform.position, transform.rotation * Quaternion.identity);
        var netObj = explosion.GetComponent<NetworkObject>();
        if (netObj != null)
            netObj.Spawn();
    }
    
    
    #endregion
    
    #region Client RPC
    [ClientRpc]
    private void UpdateClockClientRpc(float time)
    {
        if (Clock.instance != null)
        {
            Clock.instance.SetElapsedTime(time);
        }
    }

    [ClientRpc]
    public void DesactvatePlayerDeadClientRpc()
    {
            if (IsOwner)
            {
                var color = playerRenderer.material.color;
                color.a = 0.2f;
                playerRenderer.material.color = color;
            }
            else
            {
                MeshRenderer meshRenderer = playerRenderer.GetComponent<MeshRenderer>();
                MeshFilter meshFilter = playerRenderer.GetComponent<MeshFilter>();

                if (meshRenderer != null)
                    meshRenderer.enabled = false;
                if (meshFilter != null)
                {
                    meshFilter.sharedMesh = null;
                }
            }
    }
    
    [ClientRpc]
    private void DesactivateColliderForOthersClientRpc()
    {
        if(IsOwner)
            return;
        
        Collider col = GetComponentInChildren<Collider>();
        if (col != null)
            Destroy(col);
        DesactvatePlayerDeadClientRpc();
    }
    #endregion
    
    private void ApplyRoleCameraSettings()
    {
        FollowCamera cam = Camera.main?.GetComponent<FollowCamera>();
        if (cam == null)
            return;
        
        bool isSeaker = gameObject.CompareTag("Seaker");

        if (isSeaker)
        {
            cam.height = 1.33f;
            cam.distance = 3.33f;
            Camera.main.fieldOfView = 80;
        }
        else
        {
            cam.height = 2.57f;
            cam.distance = 3.24f;
            Camera.main.fieldOfView = 80;
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