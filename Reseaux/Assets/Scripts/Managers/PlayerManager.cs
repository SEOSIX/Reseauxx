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

    public GameObject hidderUI;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        if (!IsOwner && hidderUI != null)
            hidderUI.SetActive(false);
        else
        {
            if (hidderUI != null)
                hidderUI.SetActive(true);
        }

        AddHiddersToList();
    }

    private void Awake()
    {
        hidders = 0;
    }

    public void RegisterPlayer(PlayerNetwork player)
    {
        if (!connectedPlayers.Contains(player))
        {
            connectedPlayers.Add(player);
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        connectedPlayers.RemoveAll(p => p.OwnerClientId == clientId);
    }
    
    private void AddHiddersToList()
    {
        foreach (var player in connectedPlayers)
        {
            if (player.CompareTag("Hidder"))
            {
                hidders++;
            }
        }
        Debug.Log("Hidders = " + hidders);
    }
}