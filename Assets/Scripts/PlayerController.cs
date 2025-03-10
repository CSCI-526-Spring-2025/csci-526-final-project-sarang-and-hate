using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private float moveX = 0f;
    private float moveZ = 0f;
    private Rigidbody rb;
    private GridManager gridManager;

    public float rotationSpeed = 5f;  // Add this public variable for rotation speed

    private float minX, maxX, minZ, maxZ;
    // private bool reachDest = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        gridManager = FindObjectOfType<GridManager>();

        if (gridManager != null)
        {
            float halfTile = gridManager.tileSize / 2f;
            minX = 0f;
            maxX = gridManager.gridSize * gridManager.tileSize - gridManager.tileSize;
            minZ = 0f;
            maxZ = gridManager.gridSize * gridManager.tileSize - gridManager.tileSize;
        }
    }

    void Update()
    {
        if (gridManager != null && gridManager.IsWallRotating)
        {
            rb.velocity = Vector3.zero;
            return;
        }

        moveX = 0f;
        moveZ = 0f;

        if (Input.GetKey(KeyCode.W)) moveZ = -1f;
        if (Input.GetKey(KeyCode.S)) moveZ = 1f;
        if (Input.GetKey(KeyCode.A)) moveX = 1f;
        if (Input.GetKey(KeyCode.D)) moveX = -1f;

        // Handle movement input (Arrow keys)
        if (Input.GetKey(KeyCode.UpArrow)) moveZ = -1f;    // Move forward (Up Arrow)
        if (Input.GetKey(KeyCode.DownArrow)) moveZ = 1f;   // Move backward (Down Arrow)
        if (Input.GetKey(KeyCode.LeftArrow)) moveX = 1f;   // Move left (Left Arrow)
        if (Input.GetKey(KeyCode.RightArrow)) moveX = -1f; // Move right (Right Arrow)

        if (moveX != 0f || moveZ != 0f)
        {
            Vector3 moveDirection = new Vector3(moveX, 0f, moveZ).normalized;

            transform.rotation = Quaternion.Slerp(
            transform.rotation,  // Current rotation
            Quaternion.LookRotation(moveDirection),  // Target rotation
            rotationSpeed * Time.deltaTime  // The rotation speed, scaled by deltaTime
            );
            rb.velocity = new Vector3(moveDirection.x * moveSpeed, rb.velocity.y, moveDirection.z * moveSpeed);
        }
        else
        {
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
        }
    }

    void FixedUpdate()
    {
        Vector3 clampedPosition = rb.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX, maxX);
        clampedPosition.z = Mathf.Clamp(clampedPosition.z, minZ, maxZ);
        rb.position = clampedPosition;

        CheckVictoryCondition();
    }

    private void CheckVictoryCondition()
    {
        Vector3 playerPos = transform.position;
        if (Mathf.RoundToInt(playerPos.x) == 0 && Mathf.RoundToInt(playerPos.z) == 0)
        {
            TriggerWinCondition();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            rb.velocity = Vector3.zero;
            transform.rotation = Quaternion.identity;
        }
    }

    public void TriggerWinCondition()
    {
        GameTimer gameTimer = FindObjectOfType<GameTimer>();
        if (gameTimer != null)
        {
            gameTimer?.ShowVictoryPanel();
        }
    }
}
