using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvisiblePowerUp : MonoBehaviour
{
    public float powerUpDuration = 30f; // Duration the player can go through walls

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Activate invisibility power-up on the player
            // This will allow the player to move across tiles freely up to only 2 tiles 
            other.GetComponent<PlayerController>().ActivateInvisibility(powerUpDuration);
            Destroy(gameObject); // Destroy the power-up after being collected
        }
    }
}
