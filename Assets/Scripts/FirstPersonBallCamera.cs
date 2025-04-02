using UnityEngine;

public class FirstPersonBallCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 0.4f, -0.9f);
    public float smoothSpeed = 10f;

    void LateUpdate()
    {
        if (target == null) return;

        // Always stay behind the player (based on current facing direction)
        Vector3 desiredPosition = target.position + target.TransformDirection(offset);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Look in the direction the player is facing
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(target.forward), smoothSpeed * Time.deltaTime);
    }

}
