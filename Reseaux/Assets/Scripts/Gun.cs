using System;
using Unity.Netcode;
using UnityEngine;

public class Gun : NetworkBehaviour
{
    public static Gun instance { get; private set;}
    [SerializeField] private GameObject balls;
    [SerializeField] private Transform zoneToInstanciate;
    [SerializeField] private float shootingForce = 10f;

    [SerializeField] private int ballMax;
    [SerializeField] private float fireRate = 0.5f;
    private float lastFireTime = 0f;

    
    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetMouseButton(0))
        {
            TryShoot();
        }
        RotateBallToCamera();
    }

    private void TryShoot()
    {
        if (Time.time - lastFireTime < fireRate)
            return;

        lastFireTime = Time.time;
        ShootServerRpc();
    }

    [ServerRpc]
    private void ShootServerRpc()
    {
        if (ballMax <= 0) return;

        GameObject bullet = Instantiate(balls, zoneToInstanciate.position, zoneToInstanciate.rotation);
        bullet.GetComponent<NetworkObject>().Spawn();
        
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(zoneToInstanciate.forward * shootingForce, ForceMode.Impulse);
        }
        ballMax--;
    }

    private void RotateBallToCamera()
    {
        if (Camera.main == null) return;

        float targetYaw = Camera.main.transform.eulerAngles.y;
        Quaternion targetRotation = Quaternion.Euler(0, targetYaw, 0);
        zoneToInstanciate.transform.rotation = Quaternion.Slerp(zoneToInstanciate.transform.rotation, targetRotation, Time.fixedDeltaTime);
    }
}