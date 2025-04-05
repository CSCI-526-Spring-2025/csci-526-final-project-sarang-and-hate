using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro; 


public class GridManager : MonoBehaviour
{
    private bool tutorialCompleted = false; // ✅ Track magic tile tutorial
    public GameObject tilePrefab; // Assign a basic cube in Unity
    public int gridSize = 6;
    public float tileSize = 1.0f; // Spacing between tiles

    public GameObject wallPrefab;  // Assign in Inspector

    private Tile[,] tiles;
    private Dictionary<Vector2Int, int> tileZones = new Dictionary<Vector2Int, int>(); // NEW: Store tile zone mapping
    
    private int lastPlayerZone = -1; // Track player's last zone
    public GameObject player; // Assign in Inspector
    public Material startingMaterial;   // Assign in the Inspector
    public Material destinationMaterial; // Assign in the Inspector

    public Material defaultTileMaterial; //default color of tiles 

    // NEW: List to store all wall objects for tracking
    public List<GameObject> wallList = new List<GameObject>();

    // NEW: Dictionary to track each wall's rotation state
    private Dictionary<GameObject, int> wallRotationState = new Dictionary<GameObject, int>();

    private int wallCounter = 1; // NEW: Unique ID counter for each wall

    private Dictionary<int, List<string>> rotationSequencesDict; // declaring dict for rotation sequences\


    private Vector2Int lastPlayerTile = new Vector2Int(-1, -1); // Stores last tile position
    private HashSet<Vector2Int> triggeredTiles = new HashSet<Vector2Int>(); // Stores triggered tiles

    private bool isWallRotating = false; // Track if walls are currently rotating
    public bool IsWallRotating => isWallRotating; // Public getter for player script
    private HashSet<GameObject> wallsCurrentlyRotating = new HashSet<GameObject>();

    public GameObject collectibleInvisible; // Assign your collectible prefab in the inspector

    private bool hasSpawnedCollectibleAtStart = false;

    //For directional arrows 
    public GameObject arrowPrefab; // assign in Inspector
    private List<GameObject> tutorialArrows = new List<GameObject>();

    //For tracking flashed zones 
     private HashSet<int> flashedZones = new HashSet<int>(); // 🔹 Track which zones have flashed
     //Boolean
     private bool zone8ArrowTriggered = false;

     //Arrows for zone 8 and zone 9 
     private GameObject arrowToZone8;
    private GameObject arrowToZone9;

    private GameObject arrowToDestination; // Final arrow to destination
    private bool destinationArrowInitialized = false; // 🟢 Flag to prevent early arrow activation

    private Vector3 zone9Center;

    //Provide text for zone entering 
    public TMP_Text zoneMessageText; // Assign in inspector

    //Variables for trap tile tutorial 
    private GameObject arrowToTrapTile; // 
    private bool trapTileArrowShown = false; // 🟢 Flag
    private bool trapTileTriggered = false; // 🟢 Flag to only allow once
    private bool zone9VisitedAfterZone8 = false; // 🟢 Track path logic


    bool zone8Entered = false;
    bool returnedToZone9 = false;
    bool trapTileArmed = false;

    bool powerUpAvailable = false;
    bool powerUpCollected = false;
    GameObject arrowToPowerUp;
    GameObject powerUpObject; // Optional reference if you want to deactivate it later


    //To set up manual wall rotation for player 
    private HashSet<int> zonesPlayerRotated = new HashSet<int>(); // to track per-zone usage
    private int maxRotations = 3;
    private int rotationsUsed = 0;

    public int GetRotationsUsed() => rotationsUsed;
    public int GetMaxRotations() => maxRotations;

    //Camera for Mini Map 
    public Camera minimapCamera; // Assign in Inspector

    //ENUM for levels
    public enum MazeLevel
    {
        Level1 = 1,
        Level2 = 2,

        Level3 = 3
    }
    public MazeLevel currentMazeLevel = MazeLevel.Level1;

    [SerializeField] private bool[,,] gridWallsLevel1 = new bool[6,6,4]
    {
        { { false, false, false,false}, { false, true, false, true }, { false, false, true, true}, { false, false, false,false }, { false, false,false, false }, { false, true, false, true } },
        { { false, true, false, true }, { false, false, false, false }, { false, false, false, false }, {false, false, true, true}, { false, true, false,true }, {false, false, false, false } },
        { { false, false, false, false }, { false, true, false, true }, { false, false, false, false }, { false, true, false, true }, { false, false, false, false }, {true, false, true,false } },
        { { false, true, false,true}, { false, false, false,false }, { false,true, false,true }, { false, false,false,false }, {true,false,true, false }, {false,false, false,false } },
        { { true,true, false,false}, { false, false,false, false }, { false, false, false,false }, { false, true, false,true}, { false, false, false,false}, { false, true, false,true} },
        { {false, false,false, false }, {true,true, false, false }, { false, true, false,true }, { false, false, false, false }, { false,true, false,true }, { false,false, false, false } }
    };

    //N, W, E, S
    [SerializeField] private bool[,,] gridWallsLevel2 = new bool[6,6,4]
    {
        { { false, false, false,false}, { false, true, false, true }, { false, false, true, true}, { false, false, false,false }, {true,true,false, false }, { false,false, false,false } },
        { { false, true, false, true }, { false, false, false, false }, { false, false, false, false }, {false, false, true, true}, { false,false, false,false }, {true, true, false, false } },
        { { false, false, false, false }, { true, false,true,false}, { false, false, false, false }, { false, true, false, true }, { false, false, false, false }, {true, false, true,false } },
        { { true,false,true,false}, { false, false, false,false }, { false,true, false,true }, { false, false,false,false }, {true,false,true, false }, {false,false, false,false } },
        { {false,false,true,true}, { false, false,false, false },  { false,false,false, false }, { true,false,true,false}, { false, false, false,false}, {true,false,true,false} },
        { {false, false,false, false }, {false,false,true,true}, { true,false,true,false}, { false, false, false, false }, {true,false,true,false}, { false,false, false, false} }
    };

    // N W E S 
    [SerializeField] private bool[,,] gridWallsLevel3 = new bool[8,8,4]
    {
        { { false, false, true, true }, { false, false, false, false }, { false, false, false, false }, { false, true, false,true},
        {true,true, false, false }, { false, false, false, false }, { false, true, false,true}, { false, false, false, false } },
        
        { { false, false, false, false }, { false, false, true, true }, { false, true, false,true}, { false, false, false, false },
        { false, false, false, false }, { true,true, false, false }, { false, false, false, false }, { false,true, false,true} },

        { {true,true, false, false }, { false, false, false, false }, { false,true, false,true}, { false, false, false, false },
        { false, false, false, false }, {true, false,true, false }, {false, false,true,true}, { false, false, false, false } },

        { { false, false, false, false }, {true,true, false, false }, { false, false, false, false }, { false,true, false,true},
        {true, false,true, false }, { false, false, false, false }, { false, false, false, false }, { false, false,true,true} },

        { { false, false, false, false }, { false,true, false,true}, {true, false,true, false }, { false, false, false, false },
        { false, false, false, false }, {true, false,true, false }, { false,true, false,true}, { false, false, false, false } },

        { { false,true, false,true}, { false, false, false, false }, { false, false, false, false }, {true, false,true, false },
        { true, false,true, false }, { false, false, false, false }, { false, false, false, false }, { false,true, false,true } },

        { {true, false,true, false }, { false, false, false, false }, { false, false,true,true}, { false, false, false, false },
        { false, false, false, false }, {true, false,true, false }, { false, false, false, false }, {true, false,true, false } },

        { { false, false, false, false }, {true, false,true, false }, { false, false, false, false }, { false, false,true,true },
        {true, false,true, false }, { false, false, false, false }, {true, false,true, false }, { false, false, false, false } }
    };


    private bool[,,] gridWalls; // this was your internal “active” array

    // Maze layout: Specifies which walls exist for each tile.
    // N, W, E, S
    // private bool[,,] gridWalls = new bool[6, 6, 4] {
    //     { { false, false, false,false}, { false, true, false, true }, { false, false, true, true}, { false, false, false,false }, { false, false,false, false }, { false, true, false, true } },
    //     { { false, true, false, true }, { false, false, false, false }, { false, false, false, false }, {false, false, true, true}, { false, true, false,true }, {false, false, false, false } },
    //     { { false, false, false, false }, { false, true, false, true }, { false, false, false, false }, { false, true, false, true }, { false, false, false, false }, {true, false, true,false } },
    //     { { false, true, false,true}, { false, false, false,false }, { false,true, false,true }, { false, false,false,false }, {true,false,true, false }, {false,false, false,false } },
    //     { { true,true, false,false}, { false, false,false, false }, { false, false, false,false }, { false, true, false,true}, { false, false, false,false}, { false, true, false,true} },
    //     { {false, false,false, false }, {true,true, false, false }, { false, true, false,true }, { false, false, false, false }, { false,true, false,true }, { false,false, false, false } }
    // };

    void Start()
    {
        //this checks what maze leveel and which walls set up to use 
        switch (currentMazeLevel)
        {
            case MazeLevel.Level1:
                gridWalls = gridWallsLevel1;
                break;
            case MazeLevel.Level2:
                gridWalls = gridWallsLevel2;
                break;
            case MazeLevel.Level3:
                gridSize = 8;
                gridWalls = gridWallsLevel3;

                //Set position to 7,0,0 to bottom right of the grid
                player.transform.position = new Vector3(7f, 0f, 7f);

                break;
        }
        GenerateGrid();
        if (currentMazeLevel == MazeLevel.Level3)
        {
            StartCoroutine(ShowMagicTileTutorial()); // ✅ Show tutorial on start
        }
        else
        {
            tutorialCompleted = true; // ✅ For other levels, start immediately
        }

        if (currentMazeLevel == MazeLevel.Level1)
        {
            //Vector3 zone8Center = new Vector3(4.5f, 0.2f, 2.5f); // actual Zone 8 center
            Vector3 zone8Center = new Vector3(4f, 0.2f, 3f); // ✔️ actual Zone 8 center
            zone9Center = new Vector3(4f, 0.2f, 4f);          // ✔️ promote to class level
            arrowToZone8 = CreateTutorialArrow(zone8Center);
            StartCoroutine(BounceArrow(arrowToZone8));
        }

        // List<Vector2Int> tutorialPath = new List<Vector2Int>
        // {
        //     new Vector2Int(5,5), new Vector2Int(4,5), new Vector2Int(4,4), // Zone 8
        //     new Vector2Int(5,4), new Vector2Int(5,5)  // Back to Zone 9
        // };
        // StartCoroutine(HighlightPathTiles(tutorialPath, Color.cyan, 2.5f));

        Vector3 playerPos = player.transform.position;
        int playerTileX = Mathf.RoundToInt(playerPos.x);
        int playerTileY = Mathf.RoundToInt(playerPos.z);
        Vector2Int startTile = new Vector2Int(playerTileX, playerTileY);
        if (tileZones.ContainsKey(startTile))
        {
            lastPlayerZone = tileZones[startTile];
        }
        //StartRotatingWalls(); // NEW: Start automatic wall movement
        //I commented the random rotating walls for now so that walls only trigger at certain checkpoints 
        SetupRotationSequences(); 
        //Adjust Minicam depending on grid size
        AdjustMinimapViewport();
    }
    void Update()
    {
        if (!tutorialCompleted) return; // ⛔ Pause all logic until tutorial ends
        CheckPlayerZone();
    }


    IEnumerator ShowMagicTileTutorial()
    {
        List<Vector2Int> magicTiles = new List<Vector2Int>
        {
            new Vector2Int(6, 5),
            new Vector2Int(3, 6),
            new Vector2Int(2, 2)
        };

        List<Tile> tilesToFlash = new List<Tile>();

        // Highlight each tile and save reference
        foreach (var pos in magicTiles)
        {
            Tile tile = tiles[pos.x, pos.y];
            tile.tileRenderer.material.color = new Color(0.5f, 0f, 1f); // Purple glow
            tilesToFlash.Add(tile);
        }

        if (zoneMessageText != null)
        {
            zoneMessageText.text = "Purple tiles boost you forward!";
            zoneMessageText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(5f);


        // Reset tile colors to original
        foreach (Tile tile in tilesToFlash)
        {
            tile.tileRenderer.material.color = defaultTileMaterial.color;
            tile.originalColor = defaultTileMaterial.color; // update original color so its not purple basically doesnt fonclit with zone flashes
        }

        // Hide message
        if (zoneMessageText != null)
        {
            zoneMessageText.gameObject.SetActive(false);
        }

        tutorialCompleted = true;
    }

    /// <summary>
    /// Generates a grid of tiles and places walls based on the predefined gridWalls array.
    /// </summary>
    void GenerateGrid()
    {
        ClearGrid();
        tiles = new Tile[gridSize, gridSize];
        AssignZones();


        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                // Instantiate Tile
                GameObject tileGO = Instantiate(tilePrefab, new Vector3(x, 0, y), Quaternion.identity, transform);
                tileGO.name = $"Tile ({x}, {y}) - Zone {tileZones[new Vector2Int(x, y)]}";
                Tile tile = tileGO.AddComponent<Tile>();
                tiles[x, y] = tile;
                Renderer renderer = tileGO.GetComponent<Renderer>();
                //For flashing animation
                tile.tileRenderer = renderer; // Give it the renderer reference
                


                

                // Apply color: Start tile (Blue), Finish tile (Green)
                if (renderer != null)
                {
                    if (x == 0 && y == 0) // Start Tile (Bottom-Left)
                    {
                        renderer.material.color = Color.blue; // Set Blue for Start
                        tileGO.tag = "Destination"; // Set tag for player destination
                    }
                    else if (x == gridSize - 1 && y == gridSize - 1) // Finish Tile (Top-Right)
                    {
                        renderer.material.color = Color.green; // Set Green for Finish
                    }
                }
                tile.originalColor = renderer.material.color; // Store original color

                tiles[x, y] = tile;

                // Add walls based on gridWalls array
                if (gridWalls[x, y, 0]) AddWall(tileGO, new Vector3(-0.5f, 2.5f, 0), Quaternion.identity);
                if (gridWalls[x, y, 3]) AddWall(tileGO, new Vector3(0.5f, 2.5f, 0), Quaternion.identity);
                if (gridWalls[x, y, 1]) AddWall(tileGO, new Vector3(0, 2.5f, -0.5f), Quaternion.Euler(0, 90, 0));
                if (gridWalls[x, y, 2]) AddWall(tileGO, new Vector3(0, 2.5f, 0.5f), Quaternion.Euler(0, 90, 0));

                // For Level 2, add the collectible at a specific tile location (3,3)
                if (currentMazeLevel == MazeLevel.Level2)
                {
                    // If the tile is one of these (4,5), (5,4), or (3,2)
                    if ((x == 4 && y == 5) ||
                        (x == 5 && y == 4) ||
                        (x == 3 && y == 2))
                    {
                        Instantiate(collectibleInvisible, new Vector3(x, 0.25f, y), Quaternion.identity, transform);
                    }
                }
                else if (currentMazeLevel == MazeLevel.Level3)
                {
                    if ((x == 1 && y == 6) ||
                        (x == 2 && y == 2) ||
                        (x == 6 && y == 3) ||
                        (x == 4 && y == 6))
                    {
                        Instantiate(collectibleInvisible, new Vector3(x, 0.25f, y), Quaternion.identity, transform);
                    }
                }


            }
        }
        DrawGridLines();
    }

    void AssignZones()
    {
        // Define 9 zones as 2x2 tile groups
        int zoneCounter = 1;
        for (int x = 0; x < gridSize; x += 2)
        {
            for (int y = 0; y < gridSize; y += 2)
            {
                tileZones[new Vector2Int(x, y)] = zoneCounter;
                tileZones[new Vector2Int(x, y + 1)] = zoneCounter;
                tileZones[new Vector2Int(x + 1, y)] = zoneCounter;
                tileZones[new Vector2Int(x + 1, y + 1)] = zoneCounter;
                zoneCounter++;
            }
        }
    }

    void ClearGrid()
    {
        Debug.Log("Clearing old grid...");

        wallList.Clear(); // Clear stored walls
        wallRotationState.Clear(); // Reset rotation states
        wallCounter = 1; // Reset wall ID counter

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;
            Debug.Log($"Destroying: {child.name}");

            if (Application.isPlaying)
            {
                Destroy(child);  // Safe to use in Play Mode
            }
            else
            {
                DestroyImmediate(child);  // Required for Edit Mode
            }
        }
        tileZones.Clear(); // Clear previous zone data

        if (arrowToDestination != null)
        {
            Destroy(arrowToDestination);
        }


        Debug.Log("Grid cleared.");
    }

    /// <summary>
    /// Adds a wall to the scene, assigns it a unique ID, and stores it in the tracking list.
    /// </summary>
    void AddWall(GameObject parent, Vector3 localPosition, Quaternion rotation)
    {
        GameObject wall = Instantiate(wallPrefab, parent.transform);
        wall.transform.localPosition = localPosition;
        wall.transform.localRotation = rotation;
        wall.name = $"Wall {wallCounter}"; // NEW: Assign unique ID

        wallList.Add(wall);
        wallRotationState[wall] = 0; // NEW: Track rotation state
        wallCounter++;

        Debug.Log($"Created {wall.name} at {wall.transform.position}");
    }

void SetupRotationSequences()
    {
        rotationSequencesDict = new Dictionary<int, List<string>>();

        if (currentMazeLevel == MazeLevel.Level1 || currentMazeLevel == MazeLevel.Level2)
        {
            // Shared rotation setup for Level 1 and Level 2
            rotationSequencesDict = new Dictionary<int, List<string>>()
            {
                { 1, new List<string>
                    {
                        "Wall 1", "Wall 2", "Wall 5", "Wall 6", "Wall 7", "Wall 8",
                        "Wall 11", "Wall 12", "Wall 15", "Wall 16", "Wall 21", "Wall 22",
                        "Wall 25", "Wall 26", "Wall 29", "Wall 30", "Wall 31", "Wall 32",
                        "Wall 35", "Wall 36"
                    }
                },
                { 2, new List<string>
                    {
                        "Wall 3", "Wall 4", "Wall 9", "Wall 10", "Wall 13", "Wall 14",
                        "Wall 17", "Wall 18", "Wall 19", "Wall 20", "Wall 23", "Wall 24",
                        "Wall 27", "Wall 28", "Wall 33", "Wall 34"
                    }
                }
            };
        }
        else if (currentMazeLevel == MazeLevel.Level3)
        {
            // Custom sequences for Level 3
            rotationSequencesDict = new Dictionary<int, List<string>>()
            {
                { 1, new List<string>
                    {
                        "Wall 1", "Wall 2", "Wall 9", "Wall 10", "Wall 5", "Wall 6",
                        "Wall 13", "Wall 14", "Wall 19", "Wall 20", "Wall 27", "Wall 28",
                        "Wall 23", "Wall 24", "Wall 31", "Wall 32", "Wall 33", "Wall 34",
                        "Wall 41", "Wall 42", "Wall 37", "Wall 38", "Wall 45", "Wall 46",
                        "Wall 51", "Wall 52", "Wall 59", "Wall 60", "Wall 55", "Wall 56",
                        "Wall 63", "Wall 64"
                    }
                },
                { 2, new List<string>
                    {
                        "Wall 3", "Wall 4", "Wall 11", "Wall 12", "Wall 7", "Wall 8",
                        "Wall 15", "Wall 16", "Wall 17", "Wall 18", "Wall 25", "Wall 26",
                        "Wall 21", "Wall 22", "Wall 29", "Wall 30", "Wall 35", "Wall 36",
                        "Wall 43", "Wall 44", "Wall 39", "Wall 40", "Wall 47", "Wall 48",
                        "Wall 49", "Wall 50", "Wall 57", "Wall 58", "Wall 53", "Wall 54",
                        "Wall 61", "Wall 62"
                    }
                }
            };
        }
    }



    void TriggerRotationSequence(int sequenceIndex)
    {
        if (!rotationSequencesDict.ContainsKey(sequenceIndex))
        {
            Debug.LogWarning($"Rotation sequence {sequenceIndex} does not exist!");
            return;
        }

        List<string> selectedWalls = rotationSequencesDict[sequenceIndex];
        Debug.Log($"Triggering Rotation Sequence {sequenceIndex} due to player stepping on a tile.");

        // Determine if we should flash for Levels 2 or 3
        bool shouldFlash = false;
        Color zoneFlashColor = (sequenceIndex == 1) ? new Color(1f, 0.95f, 0.5f) : new Color(1f, 0.75f, 0.4f);
        List<int> zonesToFlash = new List<int>();

        //Flash the zone tiles before rotation
        foreach (var kvp in tileZones)
        {
            int zoneId = kvp.Value;
            bool isMatching = false;

            if (currentMazeLevel == MazeLevel.Level3)
            {
                HashSet<int> group1Zones = new HashSet<int> { 1, 3, 6, 8, 9, 11, 14, 16 };
                HashSet<int> group2Zones = new HashSet<int> { 2, 4, 5, 7, 10, 12, 13, 15 };

                isMatching = (sequenceIndex == 1 && group1Zones.Contains(zoneId)) ||
                            (sequenceIndex == 2 && group2Zones.Contains(zoneId));
            }
            else
            {
                isMatching = (sequenceIndex == 1 && zoneId % 2 == 1) ||
                            (sequenceIndex == 2 && zoneId % 2 == 0);
            }

            // 💡 For Level 1, always flash matching zones (no tracking)
            if (isMatching)
            {
                if (currentMazeLevel == MazeLevel.Level1)
                {
                    zonesToFlash.Add(zoneId);
                    shouldFlash = true;
                }
                else if (!flashedZones.Contains(zoneId)) // ✅ For Level 2 & 3 only
                {
                    zonesToFlash.Add(zoneId);
                    flashedZones.Add(zoneId);
                    shouldFlash = true;
                }
            }
        }


        if (currentMazeLevel == MazeLevel.Level1)
        {
            // always flash zones on Level 1 (for tutorial)
            StartCoroutine(FlashZones(zonesToFlash, zoneFlashColor));
        }
        else if ((currentMazeLevel == MazeLevel.Level2 || currentMazeLevel == MazeLevel.Level3) && shouldFlash)
        {
            // Flash only the first time in Level 2 or 3
            StartCoroutine(FlashZones(zonesToFlash, zoneFlashColor));
        }


        foreach (GameObject wall in wallList)
        {
            if (selectedWalls.Contains(wall.name))
            {
                RotateAndMoveWall(wall);
            }
        }
    }


    /// <summary>
    /// Rotates and moves an individual wall in a clockwise loop.
    /// </summary>
    void RotateAndMoveWall(GameObject wall)
    {

        if (wallsCurrentlyRotating.Contains(wall))
            return; // Already rotating

        wallsCurrentlyRotating.Add(wall);
        // Get the current local position relative to the tile's center (parent's origin).
        Vector3 currentLocalPos = wall.transform.localPosition;
    
    // Calculate the new local position by rotating the current position 90° around the Y-axis.
    Vector3 newLocalPos = Quaternion.Euler(0, 90, 0) * currentLocalPos;
    
    // Update the wall's local rotation by adding a 90° rotation.
    Quaternion newLocalRot = wall.transform.localRotation * Quaternion.Euler(0, 90, 0);
    
    // Smoothly interpolate to the new local position and rotation.
    StartCoroutine(SmoothMoveWall(wall, newLocalPos, newLocalRot));
    }


    IEnumerator SmoothMoveWall(GameObject wall, Vector3 targetPos, Quaternion targetRot)
    {
        isWallRotating = true; // Prevent player movement during wall rotation

        // Disable the wall's collider so it doesn't interact with the player
        //Collider wallCollider = wall.GetComponent<Collider>();
        
        /*if (wallCollider != null)
        {
            wallCollider.enabled = false;
        }*/

        float elapsedTime = 0;
        float duration = 1.5f;


        Vector3 startPos = wall.transform.localPosition;
        Quaternion startRot = wall.transform.localRotation;

        while (elapsedTime < duration)
        {
            float t = Mathf.Clamp01(elapsedTime / duration);
            wall.transform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            wall.transform.localRotation = Quaternion.Slerp(startRot, targetRot, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        wall.transform.localPosition = targetPos;
        wall.transform.localRotation = Quaternion.Euler(0, Mathf.Round(targetRot.eulerAngles.y / 90f) * 90f, 0);

        // Re-enable the collider once the rotation is complete
        /*if (wallCollider != null)
        {
            wallCollider.enabled = true;
        }*/

        wallsCurrentlyRotating.Remove(wall);

        isWallRotating = false; // Re-enable player movement
    }


    int GetSequenceIndex(Vector2Int tilePosition)
    {
        return (tilePosition.x + tilePosition.y) % rotationSequencesDict.Count + 1;
    }
    void CheckPlayerZone()
    {
        Vector3 playerPos = player.transform.position;
        int playerTileX = Mathf.RoundToInt(playerPos.x);
        int playerTileY = Mathf.RoundToInt(playerPos.z);
        Vector2Int currentTile = new Vector2Int(playerTileX, playerTileY);

        if (tileZones.ContainsKey(currentTile))
        {
            int currentZone = tileZones[currentTile];
            // 1. Player enters Zone 8 (Tile 4,3)
            if (currentMazeLevel == MazeLevel.Level1 && currentZone == 8 && playerTileX == 4 && playerTileY == 3 && !zone8Entered)
            {
                zone8Entered = true;

                if (arrowToZone8 != null)
                    Destroy(arrowToZone8);

                arrowToZone9 = CreateTutorialArrow(zone9Center);
                StartCoroutine(BounceArrow(arrowToZone9));

                if (zoneMessageText != null)
                {
                    zoneMessageText.text = "Tip: Entering or re-entering zones triggers wall rotations!";
                    zoneMessageText.gameObject.SetActive(true);
                    StartCoroutine(HideZoneMessageAfterDelay(3f));
                }
            }

            // 2. Player returns to Zone 9 AFTER visiting Zone 8
            if (currentMazeLevel == MazeLevel.Level1 && currentZone == 9 && zone8Entered && !returnedToZone9)
            {
                returnedToZone9 = true;
                trapTileArmed = true; // 3. Now we arm the trap at (4,3)

                if (arrowToZone9 != null)
                    Destroy(arrowToZone9);

                if (!trapTileArrowShown)
                {
                    trapTileArrowShown = true;
                    Vector3 trapTilePosition = new Vector3(4f, 0.2f, 3f);
                    arrowToTrapTile = CreateTutorialArrow(trapTilePosition);
                    StartCoroutine(BounceArrow(arrowToTrapTile));

                }
            }

            // 4. Player steps back onto (4,3) which is now a trap
            if (currentMazeLevel == MazeLevel.Level1 && playerTileX == 4 && playerTileY == 3 && trapTileArmed && !trapTileTriggered)
            {
                trapTileTriggered = true;

                if (arrowToTrapTile != null)
                    Destroy(arrowToTrapTile);

                if (zoneMessageText != null)
                {
                    zoneMessageText.text = "Oops! That tile was a trap!";
                    zoneMessageText.gameObject.SetActive(true);
                    StartCoroutine(HideZoneMessageAfterDelay(4f));
                }

                player.transform.position = zone9Center; // Teleport player back

                int trapZone = tileZones[currentTile];
                GameObject trapTile = GameObject.Find("Tile (4, 3) - Zone " + trapZone);
                if (trapTile != null)
                {
                    Renderer sr = trapTile.GetComponent<Renderer>();
                    sr.material.color = Color.red;
                    Tile tile = sr.GetComponent<Tile>();
                    if (tile != null)
                        tile.originalColor = Color.red;
                }

                // 👉 Now set up the power-up and guiding arrow
                powerUpAvailable = true;

                //Set up arrow to make player pick up power collectible 
                // Place arrow at (3, 4)
                Vector3 arrowPosition = new Vector3(4f, 0.2f, 5f);
                arrowToPowerUp = CreateTutorialArrow(arrowPosition);
                StartCoroutine(BounceArrow(arrowToPowerUp));


                    // Optionally instantiate an invisible power-up object at (4, 5)
                Vector3 powerUpPos = new Vector3(4f, 0.1f, 5f);
                if (collectibleInvisible != null)
                {
                    powerUpObject = Instantiate(collectibleInvisible, powerUpPos, Quaternion.identity, transform);
                }

                

            }

            // Player steps on (4, 5) to collect the power-up
            if (currentMazeLevel == MazeLevel.Level1 && playerTileX == 4 && playerTileY == 5 && powerUpAvailable && !powerUpCollected)
            {
                powerUpCollected = true;

                if (arrowToPowerUp != null)
                    Destroy(arrowToPowerUp);

                if (zoneMessageText != null)
                {
                    zoneMessageText.text = "Press C to go inivisible (:";
                    zoneMessageText.gameObject.SetActive(true);
                    StartCoroutine(HideZoneMessageAfterDelay(2f));
                }

                if (powerUpObject != null)
                {
                    Destroy(powerUpObject);
                }
                   
                // ✅ Actually grant the power-up to the player
                player.GetComponent<PlayerController>().CollectPowerUp();

                //Display the green arrow towards the final destination

                if (arrowToDestination == null)
                {
                    Vector3 finalTilePos = new Vector3(0f, 0.2f, 0f); // Adjust height as needed
                    arrowToDestination = CreateTutorialArrow(finalTilePos);
                    StartCoroutine(BounceArrow(arrowToDestination));
                }
            }






            if (currentZone != lastPlayerZone)
            {
                lastPlayerZone = currentZone;

                int dictionaryKey = 0;

                if (currentMazeLevel == MazeLevel.Level3)
                {
                    // Custom grouping for Level 3
                    HashSet<int> group1Zones = new HashSet<int> { 1, 3, 6, 8, 9, 11, 14, 16 };
                    HashSet<int> group2Zones = new HashSet<int> { 2, 4, 5, 7, 10, 12, 13, 15 };

                    if (group1Zones.Contains(currentZone)) dictionaryKey = 1;
                    else if (group2Zones.Contains(currentZone)) dictionaryKey = 2;
                }
                else
                {
                    // Default odd/even logic for Level 1 and 2
                    dictionaryKey = (currentZone % 2 == 1) ? 1 : 2;
                }
                //This is where the walls rotate based off of zones 
                // if (rotationSequencesDict.ContainsKey(dictionaryKey))
                // {
                //     StartCoroutine(DelayedRotationSequence(dictionaryKey, 0.25f));
                //     Debug.Log($"Player moved to Zone {currentZone} (Dictionary Key = {dictionaryKey}). Triggering wall movement.");
                // }
            }
        }

        if (SceneManager.GetActiveScene().name == "3DScene2")
        {
            if ((playerTileX == 3 && playerTileY == 3) || (playerTileX == 1 && playerTileY == 0))
            {
                HandleTrap(currentTile, playerTileX, playerTileY);
            }
        }
        if (SceneManager.GetActiveScene().name == "3DScene3"){
            if ((playerTileX == 5 && playerTileY == 4) || (playerTileX == 4 && playerTileY == 5) || (playerTileX == 1 && playerTileY == 2))
            {
                HandleTrap(currentTile, playerTileX, playerTileY);
            }
            if ((playerTileX == 6 && playerTileY == 5) || (playerTileX == 3 && playerTileY == 6) || (playerTileX == 2 && playerTileY == 2))
            {
                HandleMagicTile(currentTile);
            }
        }
    }

    private IEnumerator SpawnCollectibleAtStartWithDelay(float delay)
    {
        // 1. Wait for the delay so the player can be reset first
        Debug.Log("SpawnCollectibleAtStartWithDelay called... waiting " + delay + " seconds.");
        yield return new WaitForSeconds(delay);
        // 2. Spawn the collectible at (0,0)
        // spawn again *every* time they trigger a trap,
        Debug.Log("Spawning collectibleInvisible now...");
        Instantiate(collectibleInvisible, new Vector3(5.5f, 0.25f, 4.75f), Quaternion.identity, transform);
        hasSpawnedCollectibleAtStart = true;
    }
    private void HandleTrap(Vector2Int currentTile, int xTile, int yTile)
    {
        // Move player back a few tiles
        int replaceXTile = xTile + 1;
        int replaceYTile = yTile + 2;
        player.transform.position = new Vector3(replaceXTile, 0, replaceYTile);
        int currentZone = tileZones[currentTile];

        // Example: you only want to spawn the collectible the *first* time they hit a trap
        StartCoroutine(SpawnCollectibleAtStartWithDelay(1.0f)); 
        // 1 second delay 

        // Change the color of the tile at (3,3) to red
        GameObject trapTile = GameObject.Find("Tile " + currentTile + " - Zone " + currentZone); // Ensure this tile has a unique name
        if (trapTile != null)
        {
            Renderer sr = trapTile.GetComponent<Renderer>();
            sr.material.color = Color.red;
            Tile tile = sr.GetComponent<Tile>();
            if (tile != null) {
                tile.originalColor = Color.red; // 🔹 Store red as original
            }

        }
    }

    private void HandleMagicTile(Vector2Int currentTile)
    {
        Vector2Int teleportTile = GetMagicTileDestination(currentTile);
        player.transform.position = new Vector3(teleportTile.x, 0, teleportTile.y);
        Debug.Log("Magic tile triggered! Teleporting player to: " + teleportTile);

        int currentZone = tileZones[currentTile];

        // Highlight the tile with a special color to show it was activated
        GameObject magicTile = GameObject.Find("Tile " + currentTile + " - Zone " + currentZone);
        if (magicTile != null)
        {
            Renderer sr = magicTile.GetComponent<Renderer>();
            sr.material.color = new Color(0.5f, 0f, 1f); // Purple/magenta glow
        }
    }

    private Vector2Int GetMagicTileDestination(Vector2Int currentTile)
    {
        // Define fixed teleport destinations
        Dictionary<Vector2Int, Vector2Int> fixedTeleports = new Dictionary<Vector2Int, Vector2Int>
        {
            { new Vector2Int(6, 5), new Vector2Int(4, 3) },
            { new Vector2Int(3, 6), new Vector2Int(0, 4) },
            { new Vector2Int(2, 2), new Vector2Int(0, 1) }
        };

        if (fixedTeleports.ContainsKey(currentTile))
        {
            return fixedTeleports[currentTile];
        }

        // Default behavior: move one row ahead (or stay)
        int currentX = currentTile.x;
        int endX = gridSize - 1;
        return new Vector2Int(Mathf.Min(currentX + 1, endX - 1), currentTile.y);
    }

    void CheckPlayerTile()
    {
        Vector3 playerPos = player.transform.position;
        int playerTileX = Mathf.RoundToInt(playerPos.x);
        int playerTileY = Mathf.RoundToInt(playerPos.z);

        Vector2Int currentTile = new Vector2Int(playerTileX, playerTileY);

        // Define tiles that trigger wall rotations
        Dictionary<Vector2Int, int> tileRotationMapping = new Dictionary<Vector2Int, int>
        {
            { new Vector2Int(4,5), 1 }, // First major step forces a detour.
            { new Vector2Int(4,2), 2 },
            { new Vector2Int(2,4), 3 },
            { new Vector2Int(2,3), 4 },
            { new Vector2Int(1,2), 5 },
            { new Vector2Int(1,0), 6 }  // Goal tile rotation.
        };

        // If player moves to a new tile
        if (currentTile != lastPlayerTile)
        {
            triggeredTiles.Remove(lastPlayerTile); // Allow retriggering if they step off

            if (tileRotationMapping.ContainsKey(currentTile))
            {
                int sequenceIndex = tileRotationMapping[currentTile];

                if (!triggeredTiles.Contains(currentTile)) // Prevent multiple triggers
                {
                    TriggerRotationSequence(sequenceIndex);
                    triggeredTiles.Add(currentTile);
                    Debug.Log($"Rotation triggered for tile: {currentTile}");
                }
            }

            lastPlayerTile = currentTile;
        }
    }

    IEnumerator DelayedRotationSequence(int sequenceIndex, float delay)
    {
        yield return new WaitForSeconds(delay);
        TriggerRotationSequence(sequenceIndex);
    }


    IEnumerator FlashZones(List<int> zoneIds, Color flashColor, float flashDuration = 0.9f)
    {
        List<Tile> tilesToFlash = new List<Tile>();

        foreach (var kvp in tileZones)
        {
            if (zoneIds.Contains(kvp.Value))
            {
                Vector2Int pos = kvp.Key;
                Tile tile = tiles[pos.x, pos.y];
                tilesToFlash.Add(tile);

                // Ensure each tile has its own material instance
                tile.tileRenderer.material = new Material(tile.tileRenderer.material);
            }
        }

        // Flash color
        foreach (Tile tile in tilesToFlash)
        {
            tile.tileRenderer.material.color = flashColor;
        }

        yield return new WaitForSeconds(flashDuration);

        // Fade back
        float fadeTime = 1.0f;
        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            foreach (Tile tile in tilesToFlash)
            {
                Color current = tile.tileRenderer.material.color;
                tile.tileRenderer.material.color = Color.Lerp(current, tile.originalColor, elapsed / fadeTime);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        foreach (Tile tile in tilesToFlash)
        {
            tile.tileRenderer.material.color = tile.originalColor;
        }
    }

    void DrawGridLines()
    {
        GameObject gridLineParent = new GameObject("GridLines");
        gridLineParent.transform.parent = transform;

        float offset = -0.5f; // Align lines to tile edges
        float lineHeight = 0.051f; // Slightly above tile surface

        // Draw vertical lines
        for (int x = 0; x <= gridSize; x++)
        {
            GameObject lineObj = new GameObject($"VerticalLine_{x}");
            lineObj.transform.parent = gridLineParent.transform;

            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.SetPosition(0, new Vector3(x + offset, lineHeight, offset));
            lr.SetPosition(1, new Vector3(x + offset, lineHeight, gridSize + offset));
            lr.startWidth = 0.03f;
            lr.endWidth = 0.03f;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = Color.black;
            lr.endColor = Color.black;
        }

        // Draw horizontal lines
        for (int z = 0; z <= gridSize; z++)
        {
            GameObject lineObj = new GameObject($"HorizontalLine_{z}");
            lineObj.transform.parent = gridLineParent.transform;

            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.SetPosition(0, new Vector3(offset, lineHeight, z + offset));
            lr.SetPosition(1, new Vector3(gridSize + offset, lineHeight, z + offset));
            lr.startWidth = 0.03f;
            lr.endWidth = 0.03f;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = Color.black;
            lr.endColor = Color.black;
        }
    }


//This is to better tutorialize the level 1 game and how walls rotate... 
//Trying to create a path where it shows how it will trigger wall rotation based off of zones 
    IEnumerator HighlightPathTiles(List<Vector2Int> pathTiles, Color color, float fadeDuration)
    {
        List<Tile> highlightedTiles = new List<Tile>();
        foreach (var pos in pathTiles)
        {
            Tile tile = tiles[pos.x, pos.y];
            tile.tileRenderer.material = new Material(tile.tileRenderer.material);
            tile.tileRenderer.material.color = color;
            highlightedTiles.Add(tile);
        }

        yield return new WaitForSeconds(fadeDuration);

        float fadeTime = 1.0f;
        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            foreach (var tile in highlightedTiles)
            {
                Color current = tile.tileRenderer.material.color;
                tile.tileRenderer.material.color = Color.Lerp(current, tile.originalColor, elapsed / fadeTime);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        foreach (var tile in highlightedTiles)
        {
            tile.tileRenderer.material.color = tile.originalColor;
        }
    }


//Trying to do some arrow guiding for how the walls rotate for level 1 

GameObject CreateTutorialArrow(Vector3 position)
{
    GameObject arrow = Instantiate(arrowPrefab, position, Quaternion.identity);
    tutorialArrows.Add(arrow);
    return arrow;
}


IEnumerator BounceArrow(GameObject arrow)
{
    float bounceHeight = 0.25f;
    float bounceSpeed = 2f;
    Vector3 startPos = arrow.transform.position;

    while (arrow != null)
    {
        float yOffset = Mathf.Sin(Time.time * bounceSpeed) * bounceHeight;
        arrow.transform.position = new Vector3(startPos.x, startPos.y + yOffset, startPos.z);
        yield return null;
    }
}

//Delete message after displaying it
IEnumerator HideZoneMessageAfterDelay(float delay)
{
    yield return new WaitForSeconds(delay);
    if (zoneMessageText != null)
    {
        zoneMessageText.gameObject.SetActive(false);
    }
}


    public void TryPlayerRotateMaze(Vector3 playerPosition)
    {
        Vector2Int tilePos = new Vector2Int(Mathf.RoundToInt(playerPosition.x), Mathf.RoundToInt(playerPosition.z));

        if (!tileZones.ContainsKey(tilePos))
        {
            Debug.Log("Player not on a valid tile.");
            return;
        }

        int zoneId = tileZones[tilePos];

        // OPTIONAL: limit 1 use per zone
        if (zonesPlayerRotated.Contains(zoneId))
        {
            Debug.Log("You’ve already rotated this zone.");
            return;
        }

        // OPTIONAL: global limit
        if (rotationsUsed >= maxRotations)
        {
            Debug.Log("Out of rotations.");
            return;
        }

        int dictKey = (zoneId % 2 == 1) ? 1 : 2;

        if (rotationSequencesDict.ContainsKey(dictKey))
        {
            TriggerRotationSequence(dictKey); // your existing logic!
            zonesPlayerRotated.Add(zoneId);
            rotationsUsed++;
            Debug.Log($"Maze rotated by player in Zone {zoneId} (Group {dictKey}).");
        }
    }
    public float minimapPadding = 0;  // Adjust this number to add space around the maze
    public void AdjustMinimapViewport()
    {
        if (minimapCamera == null) return;

        float centerX = (gridSize - 1) * tileSize / 2f;
        float centerZ = (gridSize - 1) * tileSize / 2f;
        minimapCamera.transform.position = new Vector3(centerX, 30f, centerZ);
        minimapCamera.transform.rotation = Quaternion.Euler(90f, 270f, 0f); // Top-down

        // Match orthographic size exactly
        float mazeHeight = gridSize * tileSize;
        float mazeWidth = gridSize * tileSize;
        minimapCamera.orthographicSize = mazeHeight / 2f;

        // Match aspect ratio to avoid black bars
        Camera cam = minimapCamera.GetComponent<Camera>();
        if (cam != null)
        {
            float aspectRatio = mazeWidth / mazeHeight;
            cam.aspect = aspectRatio;
        }
        cam.aspect = 1f; // Force square if grid is square


    }


}
