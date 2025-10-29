using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    public static PlayerManager Instance;

    private readonly List<PlayerNetwork> connectedPlayers = new List<PlayerNetwork>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    public void RegisterPlayer(PlayerNetwork player)
    {
        if (!connectedPlayers.Contains(player))
            connectedPlayers.Add(player);

        Debug.Log($"Player registered. Total: {connectedPlayers.Count}");
    }

    private void OnClientDisconnected(ulong clientId)
    {
        connectedPlayers.RemoveAll(p => p.OwnerClientId == clientId);
        
        
        //idée, je peut demander un pseudo au Player avant qu'il se connecte et après seulement il se connecte et son pseudo est affiché dans son canvas
        Debug.Log($"Player disconnected. Total: {connectedPlayers.Count}");
    }

    public void AssignRandomRoles()
    {
        if (!IsServer) return;

        if (connectedPlayers.Count == 0) return;

        int seekerIndex = Random.Range(0, connectedPlayers.Count);

        for (int i = 0; i < connectedPlayers.Count; i++)
        {
            string role = i == seekerIndex ? "Seeker" : "Hidder";
            connectedPlayers[i].SetRoleServerRpc(role);
        }
    }
}