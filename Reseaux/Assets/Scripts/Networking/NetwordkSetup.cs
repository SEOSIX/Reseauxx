using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class NetwordkSetup : NetworkBehaviour
{
    public static NetwordkSetup instance { get; private set; }
    public GameObject Hidder;
    public GameObject Seaker;

    public GameObject SeakerWons;
    public Transform spawnerSeaker;
    public Transform spawnerHider;

    
    private int seekerAvailiables = 1;

    private void Awake()
    {
        instance = this;
    }

    public void Spawned()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayer;
    }
    
    private void SpawnPlayer(ulong clientId)
    {
        if (!IsServer) return;

        GameObject prefabToSpawn;

        if (seekerAvailiables >= 1)
        {
            prefabToSpawn = Random.value < 0.8f ? Seaker : Hidder;

            if (prefabToSpawn == Seaker)
            {
                seekerAvailiables = 0;
            }
        }
        else
        {
            prefabToSpawn = Hidder;
        }
        var playerInstance = Instantiate(
            prefabToSpawn,
            (prefabToSpawn == Seaker ? spawnerSeaker : spawnerHider).position,
            Quaternion.identity
        );
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        
        
        if (prefabToSpawn == Hidder)
        {
            LifeManager.instance.playerSlider.gameObject.SetActive(true);
            LifeManager.instance.playerSlider.value = LifeManager.instance.playerSlider.maxValue;
            Cursor.instance.cursorMain.SetActive(true);
        }
    }

    public void CheckNumberHidder()
    {
        if (PlayerManager.hidders <= 0)
        {
            SeakerWons.SetActive(true);
            Debug.Log("A pu");
        }
        else
        {
            return;
        }
    }
}