using Unity.Netcode;
using UnityEngine;

public class Ball : MonoBehaviour
{
    [SerializeField] private float lifeTime = 5f;

    [SerializeField] private PlayerNetwork player;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Hidder"))
        {
            if (player != null)
                player.TakeDamageServerRpc(10);
        }
    }
}
