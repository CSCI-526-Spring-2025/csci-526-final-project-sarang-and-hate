using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the creation of a 6x6 grid-based maze. 
/// Handles tile instantiation, wall placements, and automated wall movement.
/// 
/// NEW FEATURES ADDED:
/// - Tracks all walls in a list with unique IDs.
/// - Moves and rotates selected walls in a loop every 5 seconds.
/// - Implements smooth transitions for movement and rotation.
/// </summary>

/// Uncomment following line to open the maze in editable mode
/// [ExecuteInEditMode]
public class GridManager : MonoBehaviour
{
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

    public GameObject collectibleInvisible; // Assign your collectible prefab in the inspector

    private bool hasSpawnedCollectibleAtStart = false;


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
    }
    void Update(){
        CheckPlayerZone();
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
                tile.originalColor = renderer.material.color; // Store original color


                

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


            }
        }
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

        //Flash the zone tiles before rotation
        if (currentMazeLevel == MazeLevel.Level1)
        {
            Color zoneFlashColor = (sequenceIndex == 1) ? new Color(1f, 0.95f, 0.5f) : new Color(1f, 0.75f, 0.4f);

            // Flash ALL matching zones
            List<int> zonesToFlash = new List<int>();
            foreach (var kvp in tileZones)
            {
                int zoneId = kvp.Value;
                if ((sequenceIndex == 1 && zoneId % 2 == 1) || (sequenceIndex == 2 && zoneId % 2 == 0))
                {
                    if (!zonesToFlash.Contains(zoneId))
                        zonesToFlash.Add(zoneId);
                }
            }

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
        Collider wallCollider = wall.GetComponent<Collider>();
        if (wallCollider != null)
        {
            wallCollider.enabled = false;
        }

        float elapsedTime = 0;
        Vector3 startPos = wall.transform.localPosition;
        Quaternion startRot = wall.transform.localRotation;

        while (elapsedTime < 1.5f)
        {
            wall.transform.localPosition = Vector3.Lerp(startPos, targetPos, elapsedTime / 1.5f);
            wall.transform.localRotation = Quaternion.Slerp(startRot, targetRot, elapsedTime / 1.5f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        wall.transform.localPosition = targetPos;
        wall.transform.localRotation = targetRot;

        // Re-enable the collider once the rotation is complete
        if (wallCollider != null)
        {
            wallCollider.enabled = true;
        }

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

                if (rotationSequencesDict.ContainsKey(dictionaryKey))
                {
                    StartCoroutine(DelayedRotationSequence(dictionaryKey, 0.25f));
                    Debug.Log($"Player moved to Zone {currentZone} (Dictionary Key = {dictionaryKey}). Triggering wall movement.");
                }
            }
        }

        if (SceneManager.GetActiveScene().name == "Scene2")
        {
            if ((playerTileX == 3 && playerTileY == 3) || (playerTileX == 1 && playerTileY == 0))
            {
                HandleTrap(currentTile);
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
    private void HandleTrap(Vector2Int currentTile)
    {
        // Move player back to start
        player.transform.position = new Vector3(5, 0, 5);
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
        }
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


    IEnumerator FlashZones(List<int> zoneIds, Color flashColor, float flashDuration = 0.6f)
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




}
