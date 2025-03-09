using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    public float tileSize = 1.1f; // Spacing between tiles
    public GameObject wallPrefab;  // Assign in Inspector

    private Tile[,] tiles;
    private Dictionary<Vector2Int, int> tileZones = new Dictionary<Vector2Int, int>(); // NEW: Store tile zone mapping
    
    private int lastPlayerZone = -1; // Track player's last zone
    public GameObject player; // Assign in Inspector
    public Material startingMaterial;   // Assign in the Inspector
    public Material destinationMaterial; // Assign in the Inspector

    // NEW: List to store all wall objects for tracking
    private List<GameObject> wallList = new List<GameObject>();

    // NEW: Dictionary to track each wall's rotation state
    private Dictionary<GameObject, int> wallRotationState = new Dictionary<GameObject, int>();

    private int wallCounter = 1; // NEW: Unique ID counter for each wall

    private Dictionary<int, List<string>> rotationSequencesDict; // declaring dict for rotation sequences\


    private Vector2Int lastPlayerTile = new Vector2Int(-1, -1); // Stores last tile position
    private HashSet<Vector2Int> triggeredTiles = new HashSet<Vector2Int>(); // Stores triggered tiles

    private bool isWallRotating = false; // Track if walls are currently rotating
    public bool IsWallRotating => isWallRotating; // Public getter for player script



    // Maze layout: Specifies which walls exist for each tile.
    // N, W, E, S
    private bool[,,] gridWalls = new bool[6, 6, 4] {
        { { false, false, false,false}, { false, true, false, true }, { false, false, true, true}, { false, false, false,false }, { false, false,false, false }, { false, true, false, true } },
        { { false, true, false, true }, { false, false, false, false }, { false, false, false, false }, {false, false, true, true}, { false, true, false,true }, {false, false, false, false } },
        { { false, false, false, false }, { false, true, false, true }, { false, false, false, false }, { false, true, false, true }, { false, false, false, false }, {true, false, true,false } },
        { { false, true, false,true}, { false, false, false,false }, { false,true, false,true }, { false, false,false,false }, {true,false,true, false }, {false,false, false,false } },
        { { true,true, false,false}, { false, false,false, false }, { false, false, false,false }, { false, true, false,true}, { false, false, false,false}, { false, true, false,true} },
        { {false, false,false, false }, {true,true, false, false }, { false, true, false,true }, { false, false, false, false }, { false,true, false,true }, { false,false, false, false } }
    };

    void Start()
    {
        GenerateGrid();
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
        rotationSequencesDict = new Dictionary<int, List<string>>()
    {
        { 1, new List<string> { "Wall 1", "Wall 2", "Wall 3", "Wall 39" } }, // Tile (5,3) blocks an easy path, forcing detour.
        { 2, new List<string> { "Wall 5", "Wall 9", "Wall 15" } }, // Tile (4,2) opens the next segment.
        { 3, new List<string> { "Wall 6", "Wall 10", "Wall 26" } }, // Tile (3,4) rotates mid-maze area.
        { 4, new List<string> { "Wall 3", "Wall 7", "Wall 18" } }, // Tile (2,3) alters access further.
        { 5, new List<string> { "Wall 2", "Wall 8" } }, // Tile (1,2) changes the last part of the maze.
        { 6, new List<string> { "Wall 1",} },  // Tile (0,0) allows access to the finish.
        { 7, new List<string> { "Wall 20", "Wall 22" } },
        { 8, new List<string> { "Wall 25", "Wall 30" } },
        { 9, new List<string> { "Wall 35", "Wall 40" } }
    };
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
        int currentState = wallRotationState[wall]; // Get current rotation state
        Vector3 newPosition = wall.transform.localPosition;

        // Get current rotation and calculate the next rotation (relative)
        Quaternion currentRotation = wall.transform.localRotation;
        Quaternion newRotation = currentRotation * Quaternion.Euler(0, 90, 0); // Rotate 90° right

        // Define movement based on the new direction (Imaginary box cycle)
        switch (currentState)
        {
            case 0: // Move to top-right corner (B)
                newPosition += new Vector3(tileSize / 2, 0, tileSize / 2);
                break;
            case 1: // Move to bottom-right corner (C)
                newPosition += new Vector3(-tileSize / 2, 0, tileSize / 2);
                break;
            case 2: // Move to bottom-left corner (D)
                newPosition += new Vector3(-tileSize / 2, 0, -tileSize / 2);
                break;
            case 3: // Move back to top-left corner (A)
                newPosition += new Vector3(tileSize / 2, 0, -tileSize / 2);
                break;
        }

        wallRotationState[wall] = (currentState + 1) % 4; // Update rotation state

        // Apply smooth movement and rotation
        StartCoroutine(SmoothMoveWall(wall, newPosition, newRotation));
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
                TriggerRotationSequence(currentZone);
                Debug.Log($"Player moved to Zone {currentZone}. Triggering wall movement.");
            }
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

}
