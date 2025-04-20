using UnityEngine;
using UnityEngine.UI;  // Required for UI.Text
using System.Collections;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody rb;
    private GridManager gridManager;
    private TutorialScript tutorialScript;

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

    public float rotationSpeedCamera = 30f;

    public TMP_Text rotationUIText; // drag in from Inspector

    //Make mini map optional and pressed by M key 
    public GameObject minimapCamera; // drag your minimap camera here in the Inspector
    public int mapUsesRemaining = 3; // adjustable per level

    public TMP_Text mapUsesUIText; //Map Uses
    public float minimapDuration = 5f;
    private bool isMinimapActive = false;

    public static bool hasPlayerRotatedWalls = false; // track analytics for E key pressed
    public static bool hasPlayerOpenedMap = false; // track analytics for M key pressed
    public static int mapViewedNum = 0; // track analytics for M key pressed
    public static int playerUsedPowerups = 0; // track analytics for how many times E key pressed

    void Start()
    {
        transform.rotation = Quaternion.Euler(0, 180, 0);
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
        UpdateMapUsesUI();  // <-- Add this
        playerUsedPowerups = 0;
        mapViewedNum = 0;

        if (gridManager != null && gridManager.currentMazeLevel == GridManager.MazeLevel.Level3)
        {
            StartCoroutine(TemporarilyShowMinimap());
        }

        //Tutorial new implementation
        tutorialScript = FindObjectOfType<TutorialScript>();

        if (gridManager != null)
        {
            float halfTile = gridManager.tileSize / 2f;
            minX = 0f;
            maxX = gridManager.gridSize * gridManager.tileSize - gridManager.tileSize;
            minZ = 0f;
            maxZ = gridManager.gridSize * gridManager.tileSize - gridManager.tileSize;
        }
        else if (tutorialScript != null)
        {
            // Use tutorial grid bounds
            minX = tutorialScript.minX;
            maxX = tutorialScript.maxX;
            minZ = tutorialScript.minZ;
            maxZ = tutorialScript.maxZ;

        }


    }

    // void Update()
    // {
    //     if (gridManager != null && gridManager.IsWallRotating && !canPassThroughWalls)
    //     {
    //         rb.velocity = Vector3.zero;
    //         return;
    //     }

    //     moveX = moveZ = 0f;
    //     if (Input.GetKey(KeyCode.W)) moveZ = -1f;
    //     if (Input.GetKey(KeyCode.S)) moveZ = 1f;
    //     if (Input.GetKey(KeyCode.A)) moveX = 1f;
    //     if (Input.GetKey(KeyCode.D)) moveX = -1f;

    //     if (Input.GetKey(KeyCode.UpArrow)) moveZ = -1f;
    //     if (Input.GetKey(KeyCode.DownArrow)) moveZ = 1f;
    //     if (Input.GetKey(KeyCode.LeftArrow)) moveX = 1f;
    //     if (Input.GetKey(KeyCode.RightArrow)) moveX = -1f;

    //     if (moveX != 0f || moveZ != 0f)
    //     {
    //         Vector3 moveDirection = new Vector3(moveX, 0f, moveZ).normalized;

    //         if (gridManager != null && gridManager.currentMazeLevel == GridManager.MazeLevel.Level3)
    //         {
    //             Vector3 camForward = Camera.main.transform.forward;
    //             Vector3 camRight = Camera.main.transform.right;
    //             camForward.y = camRight.y = 0f;
    //             moveDirection = (camForward * -moveZ + camRight * -moveX).normalized;
    //         }

    //         transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDirection), rotationSpeed * Time.deltaTime);

    //         rb.velocity = new Vector3(moveDirection.x * moveSpeed, rb.velocity.y, moveDirection.z * moveSpeed);
    //         if (canPassThroughWalls) tilesMoved++;
    //     }
    //     else
    //     {
    //         rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
    //     }

    //     if (Input.GetKeyDown(KeyCode.C) && powerUpCount > 0 && !canPassThroughWalls)
    //     {
    //         ActivateInvisibility(3f);
    //         powerUpCount--;
    //         UpdatePowerUpUI();
    //     }
    // }

    void Update()
    {
        /*if (gridManager != null && gridManager.IsWallRotating && !canPassThroughWalls)
        {
            rb.velocity = Vector3.zero;
            return;
        }*/

        float moveInput = Input.GetAxis("Vertical");   // W/S for forward/back
        float turnInput = Input.GetAxis("Horizontal"); // A/D for turning

        // Rotate player (A/D)
        transform.Rotate(Vector3.up * turnInput * rotationSpeedCamera * Time.deltaTime);

        // Move forward in facing direction (W/S)
        Vector3 moveDirection = transform.forward * moveInput;
        rb.velocity = new Vector3(moveDirection.x * moveSpeed, rb.velocity.y, moveDirection.z * moveSpeed);

        // Power-up
        if (Input.GetKeyDown(KeyCode.C) && powerUpCount > 0 && !canPassThroughWalls)
        {
            ActivateInvisibility(3f);
            powerUpCount--;
            UpdatePowerUpUI();
            playerUsedPowerups++;
        }

        //Playing with E Key for Wall Rotation 
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (gridManager != null)
            {
                gridManager.TryPlayerRotateMaze(transform.position);
            }
            else if (tutorialScript != null)  // For Tutorial Scene 
            {
                tutorialScript.TryPlayerRotateMaze(transform.position);
            }

            hasPlayerRotatedWalls = true;
        }


        if (gridManager != null && rotationUIText != null)
        {
            rotationUIText.text = $"Rotations: {gridManager.GetRotationsUsed()} / {gridManager.GetMaxRotations()}";
        }

        if (Input.GetKeyDown(KeyCode.M) && !isMinimapActive && mapUsesRemaining > 0)
        {
            minimapCamera.SetActive(true);
            isMinimapActive = true;
            mapUsesRemaining--;
            StartCoroutine(HideMinimapAfterSeconds(minimapDuration));
            hasPlayerOpenedMap = true;
            mapViewedNum++;
            UpdateMapUsesUI();
        }

    }

    IEnumerator HideMinimapAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        minimapCamera.SetActive(false);
        isMinimapActive = false;
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
        if (tutorialScript != null)
        {
            minX = tutorialScript.minX;
            maxX = tutorialScript.maxX;
            minZ = tutorialScript.minZ;
            maxZ = tutorialScript.maxZ;
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
            powerUpUIText.text = "Power ups: " + powerUpCount;
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
        if (tutorialScript != null)
        {
            foreach (GameObject wall in tutorialScript.GetTutorialWallList())
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
            //transform.rotation = Quaternion.identity;
        }
    }

    private void CheckVictoryCondition()
    {
        Vector3 playerPos = transform.position;
        if (Mathf.RoundToInt(playerPos.x) == 0 && Mathf.RoundToInt(playerPos.z) == 0)
        {
            TriggerWinCondition();

            // Send Data to Google Forms and Spreadsheet for Analytics
            sendToGoogle sendGoogle = FindObjectOfType<sendToGoogle>();
            sendGoogle.Send();
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

    private void UpdateMapUsesUI()
    {
        if (mapUsesUIText != null)
        {
            mapUsesUIText.text = $"Map Uses Left: {mapUsesRemaining}";
        }
    }


    IEnumerator TemporarilyShowMinimap()
    {
        if (minimapCamera != null)
        {
            minimapCamera.SetActive(true);
            isMinimapActive = true;
            yield return new WaitForSeconds(3f); // Show map for 3 seconds at start
            minimapCamera.SetActive(false);
            isMinimapActive = false;
        }
    }

    public void TriggerTemporaryMinimap()
    {
        StartCoroutine(TemporarilyShowMinimap());
    }


}
