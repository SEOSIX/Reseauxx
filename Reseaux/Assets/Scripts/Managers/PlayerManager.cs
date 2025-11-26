using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PlayerManager : NetworkBehaviour
{

    public static List<PlayerNetwork> connectedPlayers = new List<PlayerNetwork>();

    public static int hidders = 0;
    
    private void OnEnable()
    {
        AddHiddersToList();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    public void RegisterPlayer(PlayerNetwork player)
    {
        if (!connectedPlayers.Contains(player))
        {
            connectedPlayers.Add(player);
            AddHiddersToList();
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        connectedPlayers.RemoveAll(p => p.OwnerClientId == clientId);
        Debug.Log($"Players lefts {connectedPlayers.Count}");
    }
    
    public void AddHiddersToList()
    {
        for (int i = 0; i < connectedPlayers.Count; i++)
        {
            if (CompareTag("Hidder"))
            {
                hidders++;
                Debug.Log(hidders);
            }
        }
    }
}