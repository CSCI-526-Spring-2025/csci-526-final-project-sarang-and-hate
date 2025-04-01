using UnityEngine;
using UnityEngine.UI;  // Required for UI.Text
using System.Collections;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody rb;
    private GridManager gridManager;

    private float moveX = 0f;
    private float moveZ = 0f;
    public float rotationSpeed = 5f;

    private bool canPassThroughWalls = false;
    private float invisibilityTime = 0f;
    private int tilesMoved = 0;

    private float minX, maxX, minZ, maxZ;

    // 🆕 UI Text for displaying power-up count
    // public Text powerUpUIText;
    public TMP_Text powerUpUIText;
    private int powerUpCount = 0;

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

        UpdatePowerUpUI();
    }

    void Update()
    {
        if (gridManager != null && gridManager.IsWallRotating && !canPassThroughWalls)
        {
            rb.velocity = Vector3.zero;
            return;
        }

        moveX = moveZ = 0f;
        if (Input.GetKey(KeyCode.W)) moveZ = -1f;
        if (Input.GetKey(KeyCode.S)) moveZ = 1f;
        if (Input.GetKey(KeyCode.A)) moveX = 1f;
        if (Input.GetKey(KeyCode.D)) moveX = -1f;

        if (Input.GetKey(KeyCode.UpArrow)) moveZ = -1f;
        if (Input.GetKey(KeyCode.DownArrow)) moveZ = 1f;
        if (Input.GetKey(KeyCode.LeftArrow)) moveX = 1f;
        if (Input.GetKey(KeyCode.RightArrow)) moveX = -1f;

        if (moveX != 0f || moveZ != 0f)
        {
            Vector3 moveDirection = new Vector3(moveX, 0f, moveZ).normalized;

            if (gridManager != null && gridManager.currentMazeLevel == GridManager.MazeLevel.Level3)
            {
                Vector3 camForward = Camera.main.transform.forward;
                Vector3 camRight = Camera.main.transform.right;
                camForward.y = camRight.y = 0f;
                moveDirection = (camForward * -moveZ + camRight * -moveX).normalized;
            }

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDirection), rotationSpeed * Time.deltaTime);

            rb.velocity = new Vector3(moveDirection.x * moveSpeed, rb.velocity.y, moveDirection.z * moveSpeed);
            if (canPassThroughWalls) tilesMoved++;
        }
        else
        {
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
        }

        if (Input.GetKeyDown(KeyCode.C) && powerUpCount > 0 && !canPassThroughWalls)
        {
            ActivateInvisibility(3f);
            powerUpCount--;
            UpdatePowerUpUI();
        }
    }

    void FixedUpdate()
    {
        if (gridManager != null)
        {
            if (gridManager.currentMazeLevel == GridManager.MazeLevel.Level3)
            {
                minX = 0f;
                maxX = gridManager.gridSize - 0.5f;
                minZ = 0f;
                maxZ = gridManager.gridSize - 0.5f;
            }
            else
            {
                float halfTile = gridManager.tileSize / 2f;
                minX = 0f;
                maxX = gridManager.gridSize * gridManager.tileSize - gridManager.tileSize;
                minZ = 0f;
                maxZ = gridManager.gridSize * gridManager.tileSize - gridManager.tileSize;
            }
        }

        Vector3 clampedPosition = rb.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX, maxX);
        clampedPosition.z = Mathf.Clamp(clampedPosition.z, minZ, maxZ);
        rb.position = clampedPosition;

        if (invisibilityTime > 0)
        {
            invisibilityTime -= Time.deltaTime;
            if (invisibilityTime <= 0)
            {
                if (gridManager != null)
                {
                    foreach (GameObject wall in gridManager.wallList)
                    {
                        Collider wallCollider = wall.GetComponent<Collider>();
                        if (wallCollider != null) wallCollider.enabled = true;

                        Renderer rend = wall.GetComponent<Renderer>();
                        if (rend != null) rend.material.color = new Color(rend.material.color.r, rend.material.color.g, rend.material.color.b, 1f);
                    }
                }

                canPassThroughWalls = false;
                tilesMoved = 0;
            }
        }

        CheckVictoryCondition();
    }

    public void CollectPowerUp()
    {
        powerUpCount++;
        UpdatePowerUpUI();
    }

    private void UpdatePowerUpUI()
    {
        if (powerUpUIText != null)
        {
            powerUpUIText.text = "Collectibles: " + powerUpCount;
        }
    }

    public void ActivateInvisibility(float duration)
    {
        canPassThroughWalls = true;
        invisibilityTime = duration;
        tilesMoved = 0;

        if (gridManager != null)
        {
            foreach (GameObject wall in gridManager.wallList)
            {
                Collider wallCollider = wall.GetComponent<Collider>();
                if (wallCollider != null) wallCollider.enabled = false;

                Renderer rend = wall.GetComponent<Renderer>();
                if (rend != null)
                {
                    Color faded = rend.material.color;
                    faded.a = 0.3f;
                    rend.material.color = faded;
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!canPassThroughWalls && collision.gameObject.CompareTag("Wall"))
        {
            rb.velocity = Vector3.zero;
            transform.rotation = Quaternion.identity;
        }
    }

    private void CheckVictoryCondition()
    {
        Vector3 playerPos = transform.position;
        if (Mathf.RoundToInt(playerPos.x) == 0 && Mathf.RoundToInt(playerPos.z) == 0)
        {
            TriggerWinCondition();
        }
    }

    public void TriggerWinCondition()
    {
        GameTimer gameTimer = FindObjectOfType<GameTimer>();
        if (gameTimer != null)
        {
            gameTimer.ShowVictoryPanel();
        }
    }
}
