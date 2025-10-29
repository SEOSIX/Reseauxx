using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PlayerManager : NetworkBehaviour
{

    private readonly List<PlayerNetwork> connectedPlayers = new List<PlayerNetwork>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        AssignRandomRoles();
    }
    
    public void RegisterPlayer(PlayerNetwork player)
    {
        if (!connectedPlayers.Contains(player))
            connectedPlayers.Add(player);

        //Debug.Log($"{pseudo} connecté. Total: {connectedPlayers.Count}");
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

        foreach (var player in connectedPlayers)
        {
            string role = Random.value < 0.5f ? "Hidder" : "Seaker";
            player.SetRoleServerRpc(role);
        }
    }
}