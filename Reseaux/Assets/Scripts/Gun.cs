using System;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public static Gun instance { get; private set;}
    [SerializeField] private GameObject balls;
    [SerializeField] private Transform zoneToInstanciate;
    [SerializeField] private float shootingForce = 10f;

    [SerializeField] private int ballMax;


    private void Awake()
    {
        instance = this;
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Shooting();
        }

        if (ballMax <= 0)
        {
            Debug.Log("Les hidders ont gagnés");
        }
    }

    private void Shooting()
    {
        GameObject objectToInstanciate = Instantiate(balls, zoneToInstanciate.position, Quaternion.identity);
        Rigidbody rb = objectToInstanciate.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(zoneToInstanciate.forward * shootingForce, ForceMode.Impulse);
        }
        ballMax--;
    }
}