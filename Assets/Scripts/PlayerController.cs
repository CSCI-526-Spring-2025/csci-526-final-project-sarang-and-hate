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


    private bool canPassThroughWalls = false; // Whether the player can pass through walls
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
        // 1. If walls are rotating and the player has not picked up invisibility, stop movement.
        if (gridManager != null && gridManager.IsWallRotating && !canPassThroughWalls)
        {
            rb.velocity = Vector3.zero;
            return;
        }

        // 2. Reset movement inputs every frame.
        moveX = 0f;
        moveZ = 0f;

        // -- WASD Keys --
        if (Input.GetKey(KeyCode.W)) moveZ = -1f;
        if (Input.GetKey(KeyCode.S)) moveZ = 1f;
        if (Input.GetKey(KeyCode.A)) moveX = 1f;
        if (Input.GetKey(KeyCode.D)) moveX = -1f;

        // -- Arrow Keys --
        if (Input.GetKey(KeyCode.UpArrow)) moveZ = -1f;    
        if (Input.GetKey(KeyCode.DownArrow)) moveZ = 1f;   
        if (Input.GetKey(KeyCode.LeftArrow)) moveX = 1f;   
        if (Input.GetKey(KeyCode.RightArrow)) moveX = -1f; 

        // 3. If we have any movement input, handle rotation and velocity.
        if (moveX != 0f || moveZ != 0f)
        {
            Vector3 moveDirection = new Vector3(moveX, 0f, moveZ).normalized;

        // Only adjust movement direction in Level 3
        if (gridManager != null && gridManager.currentMazeLevel == GridManager.MazeLevel.Level3)
        {
            Vector3 camForward = Camera.main.transform.forward;
            Vector3 camRight = Camera.main.transform.right;

            camForward.y = 0f;
            camRight.y = 0f;

            camForward.Normalize();
            camRight.Normalize();

            moveDirection = (camForward * -moveZ + camRight * -moveX).normalized;


        }


            // Smoothly rotate the player to face the movement direction
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(moveDirection),
                rotationSpeed * Time.deltaTime
            );

            // 4. Now handle whether we can pass through walls or not, and the tile limit
            if (canPassThroughWalls && tilesMoved < 500)
            {
                // Invisible + Under the move limit
                rb.velocity = new Vector3(moveDirection.x * moveSpeed, rb.velocity.y, moveDirection.z * moveSpeed);
                tilesMoved++;
            }
            else if (canPassThroughWalls && tilesMoved >= 500)
            {
                // Invisible but tile limit has been reached
                // Decide how you want to handle this scenario:
                //  - You could forcibly turn off invisibility here
                //  - You could reduce moveSpeed or do something else

                canPassThroughWalls = false; 
                // Potentially re-enable colliders, or do it wherever else you'd prefer:

                rb.velocity = new Vector3(moveDirection.x * moveSpeed, rb.velocity.y, moveDirection.z * moveSpeed);
            }
            else // !canPassThroughWalls
            {
                // Normal movement if we are not invisible
                rb.velocity = new Vector3(moveDirection.x * moveSpeed, rb.velocity.y, moveDirection.z * moveSpeed);
            }
        }
        else
        {
            // If no input, keep vertical velocity (e.g., gravity) but zero out horizontal movement
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
