using Unity.Netcode;
using UnityEngine;

public class Ball : MonoBehaviour
{
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private PlayerNetwork zaza;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hidder"))
        {
            if (zaza != null)
            {
                zaza.TakeDamageServerRpc(10);
                LifeManager.instance.CheckLifePlayer();
            }
        }
    }
}
