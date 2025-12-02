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
   
    
    
    private void OnEnable()
    {
        AddHiddersToList();
    }

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
    
    public void AddHiddersToList()
    {
        for (int i = 0; i < connectedPlayers.Count; i++)
        {
            if (CompareTag("Hidder"))
            {
                hidders += 1;
                Debug.Log(hidders);
            }
        }
    }
}