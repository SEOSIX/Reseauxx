using System;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class NetwordkSetup : NetworkBehaviour
{
    public GameObject Hidder;
    public GameObject Seaker;

    public PlayerNetwork playerNetwork;
    private int seekerAvailiables = 1;
    public void Spawned()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayer;
    }

    private void SpawnPlayer(ulong clientId)
    {
        if (!IsServer) return;
        GameObject prefabToSpawn = new GameObject();

        if (seekerAvailiables >= 1)
        {
            prefabToSpawn  = Random.value < 0.5f ? Seaker : Hidder;
            if (prefabToSpawn == Seaker)
            {
                seekerAvailiables = 0;
                FollowCamera cam = Camera.main?.GetComponent<FollowCamera>();
                cam.distance = -0.1f;
                cam.height = 0.85f;
                cam.xAxis = 0f;
                Camera.main.fieldOfView = 70;
            }
            else
            {
                LifeManager.instance.playerSlider.gameObject.SetActive(true);
                LifeManager.instance.playerSlider.value = LifeManager.instance.playerSlider.maxValue;
                Cursor.instance.cursorMain.SetActive(true);
                Camera.main.fieldOfView = 70;
            }
        }
        else
        {
            prefabToSpawn = Hidder;
            LifeManager.instance.playerSlider.gameObject.SetActive(true);
            LifeManager.instance.playerSlider.value = LifeManager.instance.playerSlider.maxValue;
            Cursor.instance.cursorMain.SetActive(true);
            Camera.main.fieldOfView = 70;
        }
        GameObject playerInstance = Instantiate(prefabToSpawn, Vector3.zero, Quaternion.identity);
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }
}