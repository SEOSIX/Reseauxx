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


    public override void OnNetworkSpawn()
    {
        playerPseudo.OnValueChanged += (oldPseudo, newPseudo) =>
        {
            Debug.Log($"Pseudo mis à jour : {newPseudo}");
            StartCoroutine(PseudoManager.instance.DebugDislayConnexion(newPseudo.ToString()));
        };
        
        playerLife.OnValueChanged += (oldLife, newLife) =>
        {
            if (IsOwner && LifeManager.instance != null)
            {
                LifeManager.instance.SetLife(newLife);
            }
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
        
        if (IsServer)
        {
            playerManager.RegisterPlayer(this);
        }
        
        if (IsOwner)
        {
            FollowCamera cam = Camera.main?.GetComponent<FollowCamera>();
            if (cam != null)
                cam.SetTarget(transform);

            string pseudo = PseudoManager.instance.playerName.text; 
            SetPseudoServerRpc(pseudo);
            ApplyRoleCameraSettings();
        }
    }
    
    private IEnumerator ServerClockLoop() 
    {
        clockRunning = true;
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
        playerLife.Value = Mathf.Max(0, playerLife.Value - damage);
    }

    
    [ServerRpc(RequireOwnership = false)]
    public void StopClockServerRpc()
    {
        clockRunning = false;
    }
    
    #endregion
    
    [ClientRpc]
    private void UpdateClockClientRpc(float time)
    {
        if (Clock.instance != null)
        {
            Clock.instance.SetElapsedTime(time);
        }
    }
    
    private void ApplyRoleCameraSettings()
    {
        FollowCamera cam = Camera.main?.GetComponent<FollowCamera>();
        if (cam == null)
        {
            return;
        }
        bool isSeaker = gameObject.CompareTag("Seaker");

        if (isSeaker)
        {
            cam.height = 1.42f;
            cam.distance = 0.8f;
            Camera.main.fieldOfView = 70;
            LifeManager.instance.playerSlider.gameObject.SetActive(false);
            Cursor.instance.cursorMain.SetActive(false);
        }
        else
        {
            if (LifeManager.instance != null)
            {
                LifeManager.instance.playerSlider.gameObject.SetActive(true);
                LifeManager.instance.playerSlider.value = LifeManager.instance.playerSlider.maxValue;
            }

            if (Cursor.instance != null)
                Cursor.instance.cursorMain.SetActive(true);

            cam.height = 1.49f;
            cam.distance = 3.24f;
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
