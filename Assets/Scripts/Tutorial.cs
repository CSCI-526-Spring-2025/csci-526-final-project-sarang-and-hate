using UnityEngine;
using UnityEngine;
using System.Collections;
using System.Collections.Generic; 
using TMPro;


public class TutorialScript : MonoBehaviour
{
    [Header("Tile Grid Settings")]
    [Header("Navigation Panel")]
    public GameObject navPanel; // Reference to NavPanel that holds all keys
    public GameObject tilePrefab;       // Assign in Inspector
    public int width = 6;
    public int height = 4;
    public float tileSize = 1.0f;
    private bool checkpoint2_1Reached = false;

    [Header("Glow Effect for UI Texts")]
    public TMP_Text rotationsRemainingText;
    public TMP_Text powerUpsRemainingText;
    public TMP_Text mapUsesLeftText;

    private GameObject[,] tiles;

    [Header("Player Setup")]
    public GameObject player;           // Drag your player object in the Inspector

    [Header("Boundary Walls (Optional)")]
    public GameObject boundaryPrefab;   // Assign a wall prefab to create surrounding walls

    public float minX, maxX, minZ, maxZ; // Public for PlayerController to read


    public GameObject wallPrefab; // WALLLL CODDEEEE

    public GameObject blackWallPrefab; // 🖤 Static black wall prefab



    private List<GameObject> wallList = new List<GameObject>();

    private Vector2Int lastPlayerTile = new Vector2Int(-1, -1); // to detect tile change

    private Dictionary<Vector2Int, Vector2Int> trapTileDestinations = new Dictionary<Vector2Int, Vector2Int>();
    private Dictionary<Vector2Int, Vector2Int> magicTileDestinations = new Dictionary<Vector2Int, Vector2Int>();

    public Camera minimapCamera; // Assign in Inspector

    private bool isTeleporting = false; // 🛡️ Prevents multiple tile triggers
    public Material goldenTileMaterial;


    // Directions: 0 = North, 1 = West, 2 = East, 3 = South
    [SerializeField] private bool[,,] tutorialWallGrid = new bool[6, 4, 4]
    {
        { { false, false, false, false }, { false, false, false, false }, { false, false, false, false }, { false, false, false, false } },
        { { false, false, false, false }, { false, false, false, false }, { false, false, false, false }, { false, false, false, false } },
        { { false, false, false, false }, { false, false, false, false }, { false, false, false, false }, { false, false, false, false } },
        { { false, false, false, false }, { false, false, false, false }, { false, false, false, false }, { false, false, false, false } },
        { { false, false, false, false }, { false, false, false, false }, { false, false, false, false }, { false, false, false, false } },
        { { true , false, false, false }, { true , true , false, true  }, { true , false, false, true  }, { true , true , false, false } }
    };

    // FOR BLACK WALLS !!!
    [SerializeField] private bool[,,] blackWallGrid = new bool[6, 4, 4]
    {
        { { false, false, false, true  }, { false, false, false, true  }, { false, false, false, true  }, { false, false, false, true  } },
        { { false, false, false, false }, { false, false, false, false }, { false, false, false, false }, { false, false, false, false } },
        { { true , true , false, false }, { true , false, true , false }, { false, false, false, false }, { false, false, false, false } },
        { { false, true , false, true  }, { false, false, true , true  }, { false, false, false, false }, { false, false, false, false } },
        { { false, false, false, false }, { false, false, false, false }, { false, false, false, false }, { false, false, false, false } },
        { { false, false, false, false }, { false, false, false, false }, { false, false, false, false }, { false, false, false, false } }
    };



    private Dictionary<GameObject, int> tutorialWallRotationState = new Dictionary<GameObject, int>();
    private HashSet<GameObject> tutorialWallsCurrentlyRotating = new HashSet<GameObject>();
    private bool tutorialIsWallRotating = false;

    public bool IsWallRotating => tutorialIsWallRotating; // for PlayerController

    // You can also track rotations
    private int tutorialRotationsUsed = 0;
    private int tutorialMaxRotations = 500;
    public int GetRotationsUsed() => tutorialRotationsUsed;
    public int GetMaxRotations() => tutorialMaxRotations;


    //This is Text on Screen
    private bool hasRotatedOnce = false;
    private bool isWaitingForRotationInput = false;

    //These are variables for tracking the powerup collectible; now we need to figure this collectible oout on how to let users know that they can go through walls 
    private bool collectibleInstructionShown = false;
    private bool collectiblePickedUp = false;
    private bool hasUsedPowerUp = false;

    public GameObject powerUpPrefab; // assign in Inspector
    public TMPro.TMP_Text tutorialText;    // assign the TMP UI text box in Inspector

    //to track and destroy the colletible 
    private GameObject currentCollectible;


    public static int playerTrapped = 0;
    public static int playerMagicallyMoved = 0;

    public GameObject arrowPrefab; // drag Arrow 1 prefab into this in the Inspector
    private GameObject collectibleArrow;

    private List<Vector3> orbSpawnPositions = new List<Vector3>()
    {
         // Existing orb spawn
        new Vector3(2f, 0.25f, 3f), // ✨ New spot
        new Vector3(2f, 0.25f, 1f), // ✨ New spot
        new Vector3(4f, 0.25f, 0f), // ✨ New spot
        // Add as many as you want!
    };


    void Start()
    {
        GenerateTileGrid();
        SetupBoundaries();
        PlacePlayer();
        DrawGridLines();

        //Manually Added Walls
        GenerateWallsFromGrid();
        StartCoroutine(ShowWallRotationTutorial());
        StartCoroutine(HideNavPanelAfterDelay(15f));

        // Trap: send player to tile (5, 3) — the green start tile
        trapTileDestinations[new Vector2Int(1, 0)] = new Vector2Int(5, 3);
        trapTileDestinations[new Vector2Int(1, 1)] = new Vector2Int(5, 3);
        trapTileDestinations[new Vector2Int(1, 3)] = new Vector2Int(5, 3);
        // Magic tiles: teleport to specific locations

        magicTileDestinations[new Vector2Int(1, 2)] = new Vector2Int(0, 3);
        AdjustMinimapViewport();
        SpawnAllCollectibles();


    }

    void Update()
    {
    if (player == null || tiles == null || isTeleporting) return; //  Prevent checking tiles while teleporting

        Vector3 playerPos = player.transform.position;
        int x = Mathf.RoundToInt(playerPos.x);
        int z = Mathf.RoundToInt(playerPos.z);
        Vector2Int currentTile = new Vector2Int(x, z);

        if (currentTile == lastPlayerTile) return;
        lastPlayerTile = currentTile;

        if (x < 0 || x >= width || z < 0 || z >= height) return;

        GameObject tileObj = tiles[x, z];
        if (tileObj.CompareTag("Trap"))
        {
            HandleTrapTile(x, z);
        }
        else if (tileObj.CompareTag("Magic"))
        {
            HandleMagicTile(x, z);
        }
    }


    private readonly Vector2Int[] goalTilePositions = new Vector2Int[]
    {
        new Vector2Int(0, 0),
        new Vector2Int(3, 0),
        new Vector2Int(5, 1),
    };

    private IEnumerator HideNavPanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (navPanel != null)
        {
            navPanel.SetActive(false);
        }
    }

    // This will toggle navPanel visibility each time the button is clicked
    public void ToggleNavPanel()
    {
        if (navPanel != null)
        {
            bool isCurrentlyActive = navPanel.activeSelf; // Check if panel is currently visible
            navPanel.SetActive(!isCurrentlyActive);        // Toggle it: if on → turn off, if off → turn on
        }
    }

    // SPAWN MORE COLLECTIBLES
    void SpawnAllCollectibles()
    {
        foreach (Vector3 pos in orbSpawnPositions)
        {
            GameObject collectible = Instantiate(powerUpPrefab, pos, Quaternion.Euler(270f, 0f, 0f), transform);
        }
    }

    void HandleTrapTile(int x, int z)
    {
        playerTrapped++;

        Vector2Int trigger = new Vector2Int(x, z);
        if (!trapTileDestinations.ContainsKey(trigger)) return;

        Vector2Int target = trapTileDestinations[trigger];
        Vector3 from = player.transform.position;
        Vector3 to = new Vector3(target.x, 0.25f, target.y);

        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.StartCoroutine(pc.SmoothTeleport(from, to, 3f));
            pc.TriggerTemporaryMinimap();
        }
        else
        {
            player.transform.position = to;
        }

        StartCoroutine(ZoomMinimap(3f));
        ShowDottedTrail(from, to, Color.red);

        if (tutorialText != null)
        {
            tutorialText.text = "You hit a trap! Be careful!";
            tutorialText.gameObject.SetActive(true);
            StartCoroutine(HideTutorialTextAfterSeconds(2f));
        }

        GameObject trapTile = tiles[x, z];
        Tile tile = trapTile.GetComponent<Tile>();
        if (tile != null)
        {
            tile.tileRenderer.material.color = Color.red;
            tile.originalColor = Color.red;
        }
    }




    public IEnumerator GlowUIText(TMP_Text textToGlow, float glowDuration = 2f)
    {
        if (textToGlow == null) yield break;

        textToGlow.fontMaterial.EnableKeyword("_OUTLINE_ON");
        textToGlow.outlineColor = Color.cyan;
        textToGlow.outlineWidth = 0.3f; // visibly thick

        yield return new WaitForSeconds(glowDuration);

        textToGlow.outlineWidth = 0f; // remove outline
    }

    void HandleMagicTile(int x, int z)
    {
        if (isTeleporting) return;
        
        // Block early teleport from (1,2) unless player has visited (2,1)
        if (x == 1 && z == 2 && !checkpoint2_1Reached)
        {
            if (tutorialText != null)
            {
                tutorialText.text = "Complete all tutorial checkpoints before this!";
                tutorialText.gameObject.SetActive(true);
                StartCoroutine(HideTutorialTextAfterSeconds(2.5f));
            }
            return;
        }

        isTeleporting = true;
        playerMagicallyMoved++;

        Vector2Int trigger = new Vector2Int(x, z);
        if (!magicTileDestinations.ContainsKey(trigger)) return;

        Vector2Int target = magicTileDestinations[trigger];
        Vector3 from = player.transform.position;
        Vector3 to = new Vector3(target.x, 0.25f, target.y);

        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.StartCoroutine(pc.SmoothTeleport(from, to, 3f));
            pc.TriggerTemporaryMinimap();
        }
        else
        {
            player.transform.position = to;
        }

        StartCoroutine(ZoomMinimap(3f));
        ShowDottedTrail(from, to, new Color(0.5f, 0f, 1f)); // 🟣 purple trail

        // Highlight the magic tile that was stepped on
        GameObject tileObj = tiles[x, z];
        Tile tile = tileObj.GetComponent<Tile>();
        if (tile != null)
        {
            tile.tileRenderer.material.color = new Color(0.5f, 0f, 1f);
            tile.originalColor = new Color(0.5f, 0f, 1f);
        }

        if (tutorialText != null)
        {
            tutorialText.text = "Magic tile activated! Boost you forward!";
            tutorialText.gameObject.SetActive(true);
            StartCoroutine(HideTutorialTextAfterSeconds(2f));
        }

        // 🕒 Wiat 0 seconds then allow other triggers again
        StartCoroutine(ResetTeleportingFlag(3f));
    }
    IEnumerator ResetTeleportingFlag(float delay)
    {
        yield return new WaitForSeconds(delay);
        isTeleporting = false;
    }





    void GenerateTileGrid()
    {
        // Clean up previous tiles
        // 🔥 Clear previous children
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject); // Use DestroyImmediate in Editor mode
        }


        tiles = new GameObject[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 spawnPosition = new Vector3(x * tileSize, 0, z * tileSize);
                GameObject tile = Instantiate(tilePrefab, spawnPosition, Quaternion.identity, transform);
                tile.name = $"Tile ({x}, {z})";
                tiles[x, z] = tile;


                Tile tileComp = tile.AddComponent<Tile>();
                tileComp.tileRenderer = tile.GetComponent<Renderer>();

                // Assign special tiles manually (hardcoded like GridManager)
                if ((x == 1 && z == 0) || (x == 1 && z == 1 ) || (x == 1 && z == 3) )  // Trap tile
                {
                    tileComp.tileRenderer.material.color = Color.red;
                    tileComp.originalColor = Color.red;
                    tile.tag = "Trap";
                }
                else if ((x == 1 && z == 2))  // Magic tiles
                {
                    // Color neonPink = new Color(1f, 0.1f, 0.9f); // Neon pink/purple
                    Color neonPink = new Color(0.5f, 0f, 1f); // Purple glow
                    tileComp.tileRenderer.material.color = neonPink;
                    tileComp.originalColor = neonPink;
                    tile.tag = "Magic";

                    // Optional: Glow effect
                    Material mat = tileComp.tileRenderer.material;
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", neonPink);


                }
                else
                {
                    tileComp.originalColor = tileComp.tileRenderer.material.color;
                }


                Renderer rend = tile.GetComponent<Renderer>();
                if (rend != null)
                {
                    // Start tile
                    // Starting tile should be green and ending tiles should be blue 
                    if (x == 5 && z == 3)
                    {
                        rend.material.color = Color.green;
                        tile.tag = "Start";
                    }
                    // Goal tile
                    else if (IsGoalTile(x, z))
                    {
                        // rend.material.color = Color.blue;
                        if (goldenTileMaterial != null)
                        {
                            rend.material = goldenTileMaterial;
                        }
                        tile.tag = "Goal";
                    }
                }
            }
        }
    }

    


    bool IsGoalTile(int x, int z)
    {
        foreach (Vector2Int pos in goalTilePositions)
        {
            if (pos.x == x && pos.y == z)
                return true;
        }
        return false;
    }


    //Add Wall code Here
    void AddWallToTile(int tileX, int tileZ, Vector3 localPosition, Quaternion rotation)
    {
        if (tiles == null || tiles[tileX, tileZ] == null) return;

        GameObject tile = tiles[tileX, tileZ];
        GameObject wall = Instantiate(wallPrefab, tile.transform);
        wall.transform.localPosition = localPosition;
        wall.transform.localRotation = rotation;
        wall.name = $"Wall_{tileX}_{tileZ}";
        wallList.Add(wall);


        tutorialWallRotationState[wall] = 0; // track rotation state
    }

    //Black Walls will be added using this 
    void AddBlackWallToTile(int tileX, int tileZ, Vector3 localPosition, Quaternion rotation)
    {
        if (tiles == null || tiles[tileX, tileZ] == null) return;

        GameObject tile = tiles[tileX, tileZ];
        GameObject blackWall = Instantiate(blackWallPrefab, tile.transform);
        blackWall.transform.localPosition = localPosition;
        blackWall.transform.localRotation = rotation;
        blackWall.name = $"BlackWall_{tileX}_{tileZ}";

        // ❌ NO rotation tracking needed for black walls
    }



    void GenerateWallsFromGrid()
    {
        float wallY = 2.5f;

        // Directions: 0 = North, 1 = West, 2 = East, 3 = South
        Vector3[] wallOffsets = {
        new Vector3(-0.5f, wallY,0f),  // North
        new Vector3(0f, wallY, -0.5f),  // West
        new Vector3(0f, wallY,0.5f),   // East
        new Vector3(0.5f, wallY,0f)    // South
        };

        Quaternion[] wallRotations = {
        Quaternion.identity,              // North
        Quaternion.Euler(0, 90, 0),       // West
        Quaternion.Euler(0, 90, 0),       // East
        Quaternion.identity               // South
        };

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                for (int dir = 0; dir < 4; dir++)
                {
                    if (tutorialWallGrid[x, z, dir])
                    {
                        AddWallToTile(x, z, wallOffsets[dir], wallRotations[dir]);
                    }
                    if (blackWallGrid[x, z, dir])
                    {
                        AddBlackWallToTile(x, z, wallOffsets[dir], wallRotations[dir]);
                    }

                }
            }
        }
    }

    



    void PlacePlayer()
    {
        if (player != null)
        {
            Vector3 startPos = new Vector3(5 * tileSize, 0.5f, 3 * tileSize);
            player.transform.position = startPos;
        }
        else
        {
            Debug.LogWarning("Player not assigned in TutorialScript!");
        }
    }

    void SetupBoundaries()
    {
        float ts = tileSize;
        float wallThickness = 0.1f;
        float wallHeight = 2f;

        // Movement limits for player
        minX = -ts / 2f;
        maxX = (width - 1) * ts + ts / 2f;
        minZ = -ts / 2f;
        maxZ = (height - 1) * ts + ts / 2f;

        if (boundaryPrefab == null) return;

        float gridWidth = width * ts;
        float gridHeight = height * ts;

        float xCenter = (width - 1) * ts / 2f;
        float zCenter = (height - 1) * ts / 2f;

        // LEFT wall — aligned to minX
        GameObject leftWall = Instantiate(boundaryPrefab, transform);
        leftWall.transform.position = new Vector3(minX - wallThickness / 2f, wallHeight / 2f, zCenter);
        leftWall.transform.localScale = new Vector3(wallThickness, wallHeight, gridHeight);

        // RIGHT wall — aligned to maxX
        GameObject rightWall = Instantiate(boundaryPrefab, transform);
        rightWall.transform.position = new Vector3(maxX + wallThickness / 2f, wallHeight / 2f, zCenter);
        rightWall.transform.localScale = new Vector3(wallThickness, wallHeight, gridHeight);

        // BOTTOM wall — aligned to minZ
        GameObject bottomWall = Instantiate(boundaryPrefab, transform);
        bottomWall.transform.position = new Vector3(xCenter, wallHeight / 2f, minZ - wallThickness / 2f);
        bottomWall.transform.localScale = new Vector3(gridWidth, wallHeight, wallThickness);

        // TOP wall — aligned to maxZ
        GameObject topWall = Instantiate(boundaryPrefab, transform);
        topWall.transform.position = new Vector3(xCenter, wallHeight / 2f, maxZ + wallThickness / 2f);
        topWall.transform.localScale = new Vector3(gridWidth, wallHeight, wallThickness);
    }

    void DrawGridLines()
    {
        GameObject gridLineParent = new GameObject("GridLines");
        gridLineParent.transform.parent = transform;

        float offset = -tileSize / 2f;
        float lineHeight = 0.05f; // Slightly above tile surface

        // Draw vertical lines
        for (int x = 0; x <= width; x++)
        {
            GameObject lineObj = new GameObject($"VerticalLine_{x}");
            lineObj.transform.parent = gridLineParent.transform;

            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.SetPosition(0, new Vector3(x * tileSize + offset, lineHeight, offset));
            lr.SetPosition(1, new Vector3(x * tileSize + offset, lineHeight, height * tileSize + offset));
            lr.startWidth = 0.03f;
            lr.endWidth = 0.03f;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = Color.black;
            lr.endColor = Color.black;
        }

        // Draw horizontal lines
        for (int z = 0; z <= height; z++)
        {
            GameObject lineObj = new GameObject($"HorizontalLine_{z}");
            lineObj.transform.parent = gridLineParent.transform;

            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.SetPosition(0, new Vector3(offset, lineHeight, z * tileSize + offset));
            lr.SetPosition(1, new Vector3(width * tileSize + offset, lineHeight, z * tileSize + offset));
            lr.startWidth = 0.03f;
            lr.endWidth = 0.03f;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = Color.black;
            lr.endColor = Color.black;
        }
    }

    //A lot of the wall rotation is duplicated from the gridManager code 
    //Right now for the wall rotation, only the walls on the tile that the player is rotating
    
    public void TryPlayerRotateMaze(Vector3 playerPosition)
    {
        if (tutorialIsWallRotating || tutorialRotationsUsed >= tutorialMaxRotations) return;

        HighlightWallsOnCurrentTile(); // 🟨 Hihglight walls 

        // Step 1: Get the tile position player is on
        int tileX = Mathf.RoundToInt(playerPosition.x);
        int tileZ = Mathf.RoundToInt(playerPosition.z);

        // Step 2: Check if within bounds
        if (tileX < 0 || tileX >= width || tileZ < 0 || tileZ >= height) return;

        GameObject currentTile = tiles[tileX, tileZ];

        // Step 3: Find all child walls and rotate only them
        foreach (Transform child in currentTile.transform)
        {
            if (child.CompareTag("Wall")) // Only rotate children tagged as "Wall"
            {
                RotateAndMoveWall(child.gameObject);
            }
        }

        tutorialRotationsUsed++;

        if (!hasRotatedOnce)
        {
            hasRotatedOnce = true;
            ShowTutorialText("Great! Now pick up the powerup to go invisible!");
            //SpawnCollectible(new Vector3(5f, 0.3f, 1f)); // Place collectible logically near player
        }

        if (rotationsRemainingText != null)
        {
            StartCoroutine(GlowUIText(rotationsRemainingText));
        }
    }

    void RotateAndMoveWall(GameObject wall)
    {
        if (tutorialWallsCurrentlyRotating.Contains(wall)) return;

        tutorialWallsCurrentlyRotating.Add(wall);

        Vector3 currentLocalPos = wall.transform.localPosition;
        Vector3 newLocalPos = Quaternion.Euler(0, 90, 0) * currentLocalPos;
        Quaternion newLocalRot = wall.transform.localRotation * Quaternion.Euler(0, 90, 0);

        StartCoroutine(SmoothMoveWall(wall, newLocalPos, newLocalRot));
    }

    IEnumerator SmoothMoveWall(GameObject wall, Vector3 targetPos, Quaternion targetRot)
    {
        tutorialIsWallRotating = true;

        float elapsedTime = 0f;
        float duration = 1.2f;

        Vector3 startPos = wall.transform.localPosition;
        Quaternion startRot = wall.transform.localRotation;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            wall.transform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            wall.transform.localRotation = Quaternion.Slerp(startRot, targetRot, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        wall.transform.localPosition = targetPos;
        wall.transform.localRotation = Quaternion.Euler(0, Mathf.Round(targetRot.eulerAngles.y / 90f) * 90f, 0);

        tutorialWallsCurrentlyRotating.Remove(wall);
        tutorialIsWallRotating = false;
    }

    //Wall Rotation Tutorial Step 1
    IEnumerator ShowWallRotationTutorial()
    {
        // Step 1: Lock player input
        PlayerController playerController = player.GetComponent<PlayerController>();
        //playerController.enabled = false;

        // Step 2: Show instruction
        if (tutorialText != null)
        {
            tutorialText.text = "This wall is blocking your path.\nPress E to rotate the walls!";
            tutorialText.gameObject.SetActive(true);
        }

        // Step 3: Bounce an arrow on a wall
        // GameObject targetWall = wallList.Count > 0 ? wallList[0] : null;
        // if (targetWall != null)
        // {
        //     GameObject arrow = CreateTutorialArrow(targetWall.transform.position);
        //     StartCoroutine(BounceArrow(arrow));
        // }

        // Step 4: Wait for E key
        isWaitingForRotationInput = true;

        // Step 5: Re-enable movement AFTER rotation
        while (!hasRotatedOnce)
        {
            yield return null;
        }

        if (tutorialText != null)
        {
            tutorialText.text = "Great! You rotated the wall!";
            yield return new WaitForSeconds(2f);
            tutorialText.gameObject.SetActive(false);
        }

        playerController.enabled = true;

        
        // Step 2: Trigger power-up tutorial
        StartCoroutine(ShowCollectibleTutorial());
    }

    void ShowTutorialText(string message)
    {
        if (tutorialText != null)
        {
            tutorialText.text = message;
            tutorialText.gameObject.SetActive(true);
        }
    }

    IEnumerator HideTutorialTextAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (tutorialText != null)
        {
            tutorialText.gameObject.SetActive(false);
        }
    }

    public bool HasPowerUp()
    {
        return true;
        //return powerUpCount > 0;
    }

    //Method to spawn a collectible depending on position and location 
    void SpawnCollectible(Vector3 position)
    {
        Instantiate(powerUpPrefab, position, Quaternion.identity, transform);
        collectibleInstructionShown = true;
    }



    IEnumerator ShowCollectibleTutorial()
    {
        Vector3 spawnPos = new Vector3(5f, 0.25f, 1f); // Desired orb location
        GameObject collectible = Instantiate(powerUpPrefab, spawnPos, Quaternion.Euler(270f, 0f, 0f), transform);

        // 🔽 Spawn the arrow above the orb
        Vector3 arrowPos = spawnPos + new Vector3(0f, 1.0f, 0f);
        collectibleArrow = Instantiate(arrowPrefab, arrowPos, Quaternion.identity, transform);
        StartCoroutine(BounceArrow(collectibleArrow));

        if (tutorialText != null)
        {
            tutorialText.text = "Now pick up the glowing orb!";
            tutorialText.gameObject.SetActive(true);
        }



        // Wait until player collects the orb
        PlayerController playerController = player.GetComponent<PlayerController>();
        int initialPowerUps = playerController.GetPowerUpCount();

        while (playerController.GetPowerUpCount() == initialPowerUps)
        {
            yield return null;
        }

        // destroy 1st arrow
        if (collectibleArrow != null)
        {
            Destroy(collectibleArrow);
        }

        //Place New Arrow in the 2nd checkpoint 
        spawnPos = new Vector3(3f, 0.25f, 0f); // Desired orb location
        arrowPos = spawnPos + new Vector3(0f, 1.0f, 0f);
        collectibleArrow = Instantiate(arrowPrefab, arrowPos, Quaternion.identity, transform);
        StartCoroutine(BounceArrow(collectibleArrow));

        if (tutorialText != null)
        {
            tutorialText.text = "Follow the Arrow to the next checkpoint! Press C to go through WALLS";
            yield return new WaitForSeconds(3f);
            tutorialText.gameObject.SetActive(false);
        }
        




        //Place New Arrow in the 3rd checkpoint 
        // Wait for player to reach second checkpoint
        // Wait for player to reach second checkpoint
        while (true)
        {
            Vector2Int tile = new Vector2Int(Mathf.RoundToInt(player.transform.position.x), Mathf.RoundToInt(player.transform.position.z));
            if (tile == new Vector2Int(3, 0))
            {
                Destroy(collectibleArrow);

                // 🟰 Now instead of final destination, first show arrow at (2,1)
                Vector3 powerUpPos = new Vector3(2f, 0.25f, 1f);
                GameObject powerUpArrow = Instantiate(arrowPrefab, powerUpPos + new Vector3(0, 1f, 0), Quaternion.identity, transform);
                StartCoroutine(BounceArrow(powerUpArrow));

                // Wait for player to reach (2,1)
                while (true)
                {
                    Vector2Int subTile = new Vector2Int(Mathf.RoundToInt(player.transform.position.x), Mathf.RoundToInt(player.transform.position.z));
                    if (subTile == new Vector2Int(2, 1))
                    {
                        Destroy(powerUpArrow);

                        // ✨ Now show the tutorial text when reaching (2,1)
                        if (tutorialText != null)
                        {
                            tutorialText.text = "Black tiles cannot be rotated!\nUse PowerUp!";
                            tutorialText.gameObject.SetActive(true);
                            StartCoroutine(HideTutorialTextAfterSeconds(4f)); // Show it for 4 seconds
                        }
                        checkpoint2_1Reached = true; 
                        // ➡️ After message, show arrow to final destination (0,0)
                        Vector3 finalPos = new Vector3(0f, 0.25f, 0f);
                        GameObject finalArrow = Instantiate(arrowPrefab, finalPos + new Vector3(0, 1f, 0), Quaternion.identity, transform);
                        StartCoroutine(BounceArrow(finalArrow));

                        break;
                    }
                    yield return null;
                }
                break;
            }
            yield return null;
        }
    }


    public List<GameObject> GetTutorialWallList()
    {
        return wallList;
    }


    IEnumerator ZoomMinimap(float duration)
    {
        if (minimapCamera == null) yield break;

        float originalSize = minimapCamera.orthographicSize;
        minimapCamera.orthographicSize = 3f; // zoom in
        yield return new WaitForSeconds(duration);
        minimapCamera.orthographicSize = originalSize;
    }

    void ShowDottedTrail(Vector3 from, Vector3 to, Color color)
    {
        GameObject trailObj = new GameObject("TeleportTrail");
        LineRenderer line = trailObj.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.SetPosition(0, new Vector3(from.x, 0.01f, from.z));
        line.SetPosition(1, new Vector3(to.x, 0.01f, to.z));
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = color;
        line.endColor = color;
        line.widthMultiplier = 0.05f;
        line.textureMode = LineTextureMode.Tile;

        line.material.mainTexture = Resources.Load<Texture2D>("DottedLine"); // optional
        Destroy(trailObj, 3f);
    }


    public void AdjustMinimapViewport()
    {
        if (minimapCamera == null) return;

        float centerX = (width - 1) * tileSize / 2f;
        float centerZ = (height - 1) * tileSize / 2f;

        // Reposition camera to center of the grid
        minimapCamera.transform.position = new Vector3(centerX, 30f, centerZ);
        minimapCamera.transform.rotation = Quaternion.Euler(90f, 270f, 0f); // Top-down

        // Dynamically scale to grid size with padding
        float largestDimension = Mathf.Max(width, height);
        minimapCamera.orthographicSize = (largestDimension * tileSize / 2f) + 0.5f; // extra padding
    }

    IEnumerator BounceArrow(GameObject arrow)
    {
        float bounceHeight = 0.3f;
        float bounceSpeed = 2f;
        float rotationSpeed = 45f;
        Vector3 startPos = arrow.transform.position;

        while (arrow != null)
        {
            float yOffset = Mathf.Sin(Time.time * bounceSpeed) * bounceHeight;
            arrow.transform.position = new Vector3(startPos.x, startPos.y + yOffset, startPos.z);
            arrow.transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
            yield return null;
        }
    }



    //Code for highlighting wall when it rotates

    void HighlightWallsOnCurrentTile()
    {
        int tileX = Mathf.RoundToInt(player.transform.position.x);
        int tileZ = Mathf.RoundToInt(player.transform.position.z);

        if (tileX < 0 || tileX >= width || tileZ < 0 || tileZ >= height) return;

        GameObject tile = tiles[tileX, tileZ];
        foreach (Transform child in tile.transform)
        {
            if (child.CompareTag("Wall"))
            {
                Renderer rend = child.GetComponent<Renderer>();
                if (rend != null)
                {
                    // Turn on emission glow
                    Material mat = rend.material;
                    mat.EnableKeyword("_EMISSION");
                    Color dimYellow = Color.yellow * 0.3f;  // Reduce brightness
                    mat.SetColor("_EmissionColor", dimYellow);
                    StartCoroutine(RemoveWallHighlightAfterDelay(mat, 1f));
                }
            }
        }
    }

    IEnumerator RemoveWallHighlightAfterDelay(Material mat, float delay)
    {
        yield return new WaitForSeconds(delay);
        mat.DisableKeyword("_EMISSION");
    }


}
