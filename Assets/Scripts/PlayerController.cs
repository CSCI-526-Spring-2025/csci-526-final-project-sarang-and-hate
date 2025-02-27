using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f; // Speed of movement
    private float moveX = 0f;
    private float moveZ = 0f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Prevents external rotation changes
    }

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

        if (moveX != 0f || moveZ != 0f)
        {
            // Set velocity for physics-based movement, keeping y velocity unchanged
            Vector3 moveDirection = new Vector3(moveX, 0f, moveZ).normalized;
            rb.velocity = new Vector3(moveDirection.x * moveSpeed, 0f, moveDirection.z * moveSpeed);
        }
        else
        {
            // Stop movement when no keys are pressed
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            // Maintain player's original rotation even if hit by rotating walls
            transform.rotation = Quaternion.identity;
        }
    }
}
