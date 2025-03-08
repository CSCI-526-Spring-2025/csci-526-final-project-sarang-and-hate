using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvisiblePowerUp : MonoBehaviour
{
    public float powerUpDuration = 3f; // Duration the player can go through walls

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //other.GetComponent<PlayerController>().ActivateInvisibility(powerUpDuration);
            Destroy(gameObject); // Destroy the power-up after being collected
        }
    }
}
