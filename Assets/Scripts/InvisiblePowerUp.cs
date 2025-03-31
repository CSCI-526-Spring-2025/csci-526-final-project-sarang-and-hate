using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvisiblePowerUp : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.CollectPowerUp(); // Add to inventory
                Destroy(gameObject);
            }
        }
    }
}
