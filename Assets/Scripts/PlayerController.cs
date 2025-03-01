using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f; // Speed of the player's movement
    private float moveX = 0f;
    private float moveZ = 0f;
    private Rigidbody rb;
    private GridManager gridManager;

    // Boundaries for movement within the maze
    private float minX, maxX, minZ, maxZ;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Prevents external rotation changes

        // Find and assign the GridManager instance
        gridManager = FindObjectOfType<GridManager>();

        // Set movement boundaries based on the grid size
        if (gridManager != null)
        {
            float halfTile = gridManager.tileSize / 2f; // Half tile size for centering
            minX = 0f; // Leftmost boundary at 0
            maxX = gridManager.gridSize * gridManager.tileSize - gridManager.tileSize; // Rightmost boundary

            minZ = 0f; // Bottom boundary at 0
            maxZ = gridManager.gridSize * gridManager.tileSize - gridManager.tileSize; // Top boundary
        }
    }

    void Update()
    {
        // Prevent movement if walls are rotating
        if (gridManager != null && gridManager.IsWallRotating)
        {
            rb.velocity = Vector3.zero; // Stop player movement while walls rotate
            return;
        }

        // Reset movement direction at the start of each frame
        moveX = 0f;
        moveZ = 0f;

        // Handle movement input (WASD keys)
        if (Input.GetKey(KeyCode.W)) moveZ = -1f; // Move forward
        if (Input.GetKey(KeyCode.S)) moveZ = 1f;  // Move backward
        if (Input.GetKey(KeyCode.A)) moveX = 1f;  // Move left
        if (Input.GetKey(KeyCode.D)) moveX = -1f; // Move right

        // Handle movement input (Arrow keys)
        if (Input.GetKey(KeyCode.UpArrow)) moveZ = -1f;    // Move forward (Up Arrow)
        if (Input.GetKey(KeyCode.DownArrow)) moveZ = 1f;   // Move backward (Down Arrow)
        if (Input.GetKey(KeyCode.LeftArrow)) moveX = 1f;   // Move left (Left Arrow)
        if (Input.GetKey(KeyCode.RightArrow)) moveX = -1f; // Move right (Right Arrow)

        // Apply movement only if a key is pressed
        if (moveX != 0f || moveZ != 0f)
        {
            // Normalize movement direction to maintain consistent speed in diagonal movement
            Vector3 moveDirection = new Vector3(moveX, 0f, moveZ).normalized;
            rb.velocity = new Vector3(moveDirection.x * moveSpeed, rb.velocity.y, moveDirection.z * moveSpeed);
        }
        else
        {
            // Stop movement when no keys are pressed
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
        }
    }

    void FixedUpdate()
    {
        // Clamp the player's position within the defined maze boundaries
        Vector3 clampedPosition = rb.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX, maxX);
        clampedPosition.z = Mathf.Clamp(clampedPosition.z, minZ, maxZ);
        rb.position = clampedPosition;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // If the player collides with a wall, stop its movement
        if (collision.gameObject.CompareTag("Wall"))
        {
            rb.velocity = Vector3.zero; // Stop movement
            transform.rotation = Quaternion.identity; // Reset rotation to prevent unintended changes
        }
    }
}
