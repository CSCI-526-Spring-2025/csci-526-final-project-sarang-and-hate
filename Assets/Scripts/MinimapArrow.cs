using UnityEngine;

public class MinimapArrow : MonoBehaviour
{
    public Transform player;

    void LateUpdate()
    {
        // Make arrow rotate in 2D based on player Y rotation
        float playerY = player.eulerAngles.y;
        transform.rotation = Quaternion.Euler(0f, 0f, -playerY);
    }
}

