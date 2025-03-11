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


    private bool canPassThroughWalls = true; // Whether the player can pass through walls
    private float invisibilityTime = 30f; // Time remaining for the invisibility power-up
    private int tilesMoved = 0; // Keep track of how many tiles the player has moved through

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
        // If the walls are rotating and the player has not picked up the invisible power-up, stop movement
        if (gridManager != null && gridManager.IsWallRotating && !canPassThroughWalls)
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
            //This is checking whether player can pass through walls due to the collectible pickup 
            // also checking if the tiles moved is under 2
            // If invisibility is active, the player can move through walls
            if (canPassThroughWalls && tilesMoved < 500)
            {
                rb.velocity = new Vector3(moveDirection.x * moveSpeed, rb.velocity.y, moveDirection.z * moveSpeed);
                tilesMoved++; // Increment movements moved instead of tiles.. it doesnt accuractely count tiles so I set initial amount to 500
            }
            else if (!canPassThroughWalls)
            {
                rb.velocity = new Vector3(moveDirection.x * moveSpeed, rb.velocity.y, moveDirection.z * moveSpeed);
            }
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

        if (invisibilityTime > 0)
        {
            invisibilityTime -= Time.deltaTime; // Reduce the invisibility duration over time
            if (invisibilityTime <= 0)
            {
                // Re-enable all wall colliders after the invisibility duration ends
                if (gridManager != null)
                {
                    foreach (GameObject wall in gridManager.wallList)
                    {
                        Collider wallCollider = wall.GetComponent<Collider>();
                        if (wallCollider != null)
                        {
                            wallCollider.enabled = true; // Re-enable the wall's collider
                        }
                    }
                }

                canPassThroughWalls = false; // Disable invisibility after the duration
                tilesMoved = 0; // Reset the tiles moved count
            }
        }

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

    public void ActivateInvisibility(float duration)
    {
        canPassThroughWalls = true; // Enable invisibility
        invisibilityTime = duration; // Set the duration
        tilesMoved = 0; // Reset tiles moved

        // Disable all wall colliders so the player can pass through them
        if (gridManager != null) // Ensure gridManager is set
        {
            foreach (GameObject wall in gridManager.wallList)
            {
                Collider wallCollider = wall.GetComponent<Collider>();
                if (wallCollider != null)
                {
                    wallCollider.enabled = false; // Disable the wall's collider
                }
            }
        }

        Debug.Log("Invisibility Activated. Can Pass Through Walls: " + canPassThroughWalls);
    }





    private void OnCollisionEnter(Collision collision)
    {
        if (!canPassThroughWalls && collision.gameObject.CompareTag("Wall"))
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
