using Unity.Netcode;
using UnityEngine;

public class Ball : MonoBehaviour
{
    [SerializeField] private float lifeTime = 5f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Hidder"))
        {
            PlayerNetwork player = other.gameObject.GetComponent<PlayerNetwork>();
            if (player != null)
                player.TakeDamageServerRpc(10);
        }
    }
}
