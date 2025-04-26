using UnityEngine;
using UnityEngine.UI;  // Required for UI.Text
using System.Collections;
using TMPro;

public class PlayerController : MonoBehaviour
{
    // private float lastTeleportTime = -10f;
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

    public Image invisibilityBar; // drag in Inspector
    public Image invisibilityBarBackground;   // black background object

    private float invisibilityDuration = 3f; // you can customize this

    public GameObject tutorialVictoryPanel; // Drag from Inspector

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

        else if (tutorialScript != null && rotationUIText != null)
        {
            rotationUIText.text = $"Rotations: {tutorialScript.GetRotationsUsed()} / {tutorialScript.GetMaxRotations()}";
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
            if (invisibilityBar != null)
            {
                invisibilityBar.fillAmount = invisibilityTime / 3f; // assuming 3s duration
            }
            if (invisibilityTime <= 0f)
            {
                invisibilityTime = 0f;
                if (invisibilityBar != null)
                {
                    invisibilityBar.gameObject.SetActive(false);
                }
                if (invisibilityBarBackground != null)
                {
                    invisibilityBarBackground.gameObject.SetActive(false);
                }
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
                if (tutorialScript != null)
                {
                    foreach (GameObject wall in tutorialScript.GetTutorialWallList())
                    {
                        Collider wallCollider = wall.GetComponent<Collider>();
                        if (wallCollider != null) wallCollider.enabled = true;
                        // Renderer playerRenderer = GetComponent<Renderer>();
                        // if (playerRenderer != null)
                        // {
                        //     Material mat = playerRenderer.material;
                        //     Color color = mat.color;
                        //     color.a = 1f;  // fully visible
                        //     mat.color = color;
                        // }
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
        invisibilityDuration = duration;
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

                //Code for Making Player inivisble 
                // Renderer playerRenderer = GetComponent<Renderer>();
                // if (playerRenderer != null)
                // {
                //     Material mat = playerRenderer.material;
                //     mat.SetFloat("_Mode", 2); // Set to transparent mode if using Standard Shader

                //     // Enable transparency keywords
                //     mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                //     mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                //     mat.SetInt("_ZWrite", 0);
                //     mat.DisableKeyword("_ALPHATEST_ON");
                //     mat.EnableKeyword("_ALPHABLEND_ON");
                //     mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                //     mat.renderQueue = 3000;

                //     Color color = mat.color;
                //     color.a = 0.3f;  // 30% visible
                //     mat.color = color;
                // }
                Renderer rend = wall.GetComponent<Renderer>();
                if (rend != null)
                {
                    Color faded = rend.material.color;
                    faded.a = 0.3f;
                    rend.material.color = faded;
                }
            }
        }

        if (invisibilityBar != null)
        {
            invisibilityBar.fillAmount = 1f;
            invisibilityBar.gameObject.SetActive(true);
        }
        if (invisibilityBarBackground != null)
        {
            invisibilityBarBackground.gameObject.SetActive(true);
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
        int px = Mathf.RoundToInt(playerPos.x);
        int pz = Mathf.RoundToInt(playerPos.z);

        bool reachedDestination = false;

        if (tutorialScript != null)
        {
            // In Tutorial Scene, destination is always (0, 0)
            reachedDestination = (px == 0 && pz == 0);
        }
        else if (gridManager != null)
        {
            if (gridManager.currentMazeLevel == GridManager.MazeLevel.Level4)
            {
                reachedDestination = (px == 0 && pz == 9);
            }
            else
            {
                reachedDestination = (px == 0 && pz == 0);
            }
        }

        if (reachedDestination)
        {
            TriggerWinCondition();

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
        else if (tutorialVictoryPanel != null)
        {
            tutorialVictoryPanel.SetActive(true);
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

    // IEnumerator TemporarilyShowMinimap()
    // {
    //     float duration = 3f;

    //     // If called right after teleport, extend the duration
    //     if (Time.time - lastTeleportTime < 0.1f)
    //     {
    //         duration = 5f;
    //     }

    //     if (minimapCamera != null)
    //     {
    //         minimapCamera.SetActive(true);
    //         isMinimapActive = true;
    //         yield return new WaitForSeconds(duration);
    //         minimapCamera.SetActive(false);
    //         isMinimapActive = false;
    //     }
    // }

    public void TriggerTemporaryMinimap()
    {
        StartCoroutine(TemporarilyShowMinimap());
    }

    //Add this coroutine to PlayerController.cs
    public IEnumerator SmoothTeleport(Vector3 fromPosition, Vector3 toPosition, float duration)
    {
        float elapsed = 0f;
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        float yHeight = 0.15f; // consistent height above the grid

        Vector3 start = new Vector3(fromPosition.x, yHeight, fromPosition.z);
        Vector3 end = new Vector3(toPosition.x, yHeight, toPosition.z);

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(start, end, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = end;

        if (cc != null) cc.enabled = true;
    }

    // public IEnumerator SmoothTeleport(Vector3 fromPosition, Vector3 toPosition, float duration)
    // {
    //     float elapsed = 0f;
    //     CharacterController cc = GetComponent<CharacterController>();
    //     if (cc != null) cc.enabled = false;

    //     float yHeight = 0.15f; // consistent height above the grid

    //     Vector3 start = new Vector3(fromPosition.x, yHeight, fromPosition.z);
    //     Vector3 end = new Vector3(toPosition.x, yHeight, toPosition.z);

        // while (elapsed < duration)
        // {
        //     transform.position = Vector3.Lerp(start, end, elapsed / duration);
        //     elapsed += Time.deltaTime;
        //     yield return null;
        // }
        // transform.position = end;

    //     if (cc != null) cc.enabled = true;

    //     transform.position = end;
    //     lastTeleportTime = Time.time; // 🟢 mark teleport end time      
    // }

    public bool InvisibilityIsActive()
    {
        return canPassThroughWalls && invisibilityTime > 0f;
    }

    public int GetPowerUpCount()
    {
        return powerUpCount;
    }


}
