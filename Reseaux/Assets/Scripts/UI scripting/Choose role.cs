using Unity.Netcode;
using UnityEngine;

public class Chooserole : MonoBehaviour
{
    
    public void ChooseRole(string tag)
    {
        var localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject;
        PlayerNetwork playerNetwork = localPlayer.GetComponent<PlayerNetwork>();
        
        playerNetwork.SetRoleServerRpc(tag);
    }
}

