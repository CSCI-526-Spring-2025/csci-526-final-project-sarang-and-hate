using UnityEngine;

public class TiltPlane : MonoBehaviour
{
    public float tiltSpeed = 20f; // How fast the plane tilts

    void Update()
    {
        float tiltX = Input.GetAxis("Vertical") * tiltSpeed;
        float tiltZ = -Input.GetAxis("Horizontal") * tiltSpeed;

        Quaternion targetRotation = Quaternion.Euler(tiltX, 0, tiltZ);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
    }
}
