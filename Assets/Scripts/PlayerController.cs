using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f; // Speed of movement
    private float moveX = 0f;
    private float moveZ = 0f;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        // Reset movement direction at the start of each frame
        moveX = 0f;
        moveZ = 0f;

        // WASD Key Movement
        if (Input.GetKey(KeyCode.W)) moveZ = -1f; // Move forward
        if (Input.GetKey(KeyCode.S)) moveZ = 1f;  // Move backward
        if (Input.GetKey(KeyCode.A)) moveX = 1f;  // Move left
        if (Input.GetKey(KeyCode.D)) moveX = -1f; // Move right

        // Arrow Key Movement
        if (Input.GetKey(KeyCode.UpArrow)) moveZ = -1f;    // Move forward (Up Arrow)
        if (Input.GetKey(KeyCode.DownArrow)) moveZ = 1f;    // Move backward (Down Arrow)
        if (Input.GetKey(KeyCode.LeftArrow)) moveX = 1f;    // Move left (Left Arrow)
        if (Input.GetKey(KeyCode.RightArrow)) moveX = -1f;   // Move right (Right Arrow)

        // Move the player based on the input
        Vector3 moveDirection = new Vector3(moveX, 0f, moveZ);
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
    }
}
