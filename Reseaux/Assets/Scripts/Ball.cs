using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class Ball : MonoBehaviour
    {
        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject == gameObject)
                return;
            if (other.gameObject.CompareTag("Hidder"))
            {
                if (LifeManager.instance != null)
                {
                    LifeManager.instance.SetLife(LifeManager.instance.lifeValue - 1);
                }
            }
            Destroy(gameObject);
        }
    }
}