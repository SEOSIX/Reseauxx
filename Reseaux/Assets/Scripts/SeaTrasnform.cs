using System;
using UnityEngine;

public class SeaTrasnform : MonoBehaviour
{

    [SerializeField] private Transform relocate;
    private void OnCollisionEnter(Collision other)
    {
        other.transform.position = relocate.position;
    }
}
