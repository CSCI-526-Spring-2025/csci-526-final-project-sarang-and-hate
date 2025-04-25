using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinPowerUp : MonoBehaviour
{
    public float rotationSpeed = 45f;
    void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
    }
}
