using UnityEngine;
using UnityEngine;
using System.Collections;
using System.Collections.Generic; // ✅ Required for List<>


public class TutorialScript : MonoBehaviour
{
    [Header("Tile Grid Settings")]
    public GameObject tilePrefab;       // Assign in Inspector
    public int width = 6;
    public int height = 4;
    public float tileSize = 1.0f;

    private GameObject[,] tiles;

    [Header("Player Setup")]
    public GameObject player;           // Drag your player object in the Inspector

    [Header("Boundary Walls (Optional)")]
    public GameObject boundaryPrefab;   // Assign a wall prefab to create surrounding walls

    public float minX, maxX, minZ, maxZ; // Public for PlayerController to read


    public GameObject wallPrefab; // WALLLL CODDEEEE

    private List<GameObject> wallList = new List<GameObject>();

    // Directions: 0 = North, 1 = West, 2 = East, 3 = South
    [SerializeField] private bool[,,] tutorialWallGrid = new bool[6, 4, 4]
    {
        { { false, false, false, false }, { false, false, false, false }, { false, false, false, false }, { false, false, false, false } },
        { { false, false, false, false }, { true, false, false, false }, { false, true, false, false }, { false, false, false, false } },
        { { false, false, false, false }, { false, false, false, false }, { false, false, true, false }, { false, false, false, false } },
        { { false, false, true, false }, { false, false, false, false }, { false, false, false, false }, { false, false, false, false } },
        { { false, false, false, true }, { false, false, false, false }, { false, false, false, false }, { false, false, false, false } },
        { { false, false, false, false }, { false, false, false, false }, { false, false, false, false }, {true,true, false, false } }
    };

    private Dictionary<GameObject, int> tutorialWallRotationState = new Dictionary<GameObject, int>();
    private HashSet<GameObject> tutorialWallsCurrentlyRotating = new HashSet<GameObject>();
    private bool tutorialIsWallRotating = false;

    public bool IsWallRotating => tutorialIsWallRotating; // for PlayerController

    // You can also track rotations
    private int tutorialRotationsUsed = 0;
    private int tutorialMaxRotations = 5;
    public int GetRotationsUsed() => tutorialRotationsUsed;
    public int GetMaxRotations() => tutorialMaxRotations;


    //This is Text on Screen
    public TMPro.TMP_Text tutorialMessageText; // Drag into Inspector
    private bool hasRotatedOnce = false;
    private bool isWaitingForRotationInput = false;


    void Start()
    {
        GenerateTileGrid();
        SetupBoundaries();
        PlacePlayer();
        DrawGridLines();

        //Manually Added Walls
        GenerateWallsFromGrid();
        // AddWallToTile(1, 1, new Vector3(-0.5f, 2.5f, 0f), Quaternion.identity); // North
        // AddWallToTile(2, 2, new Vector3(0.5f, 2.5f, 0f), Quaternion.identity);  // South
        // AddWallToTile(3, 1, new Vector3(0f, 2.5f, -0.5f), Quaternion.Euler(0, 90, 0)); // West
        // AddWallToTile(5, 3, new Vector3(0f, 2.5f, -0.5f), Quaternion.Euler(0, 90, 0)); // West
        // AddWallToTile(4, 2, new Vector3(0f, 2.5f, 0.5f), Quaternion.Euler(0, 90, 0));  // East
        StartCoroutine(ShowWallRotationTutorial());
    }

    private readonly Vector2Int[] goalTilePositions = new Vector2Int[]
    {
        new Vector2Int(0, 3),
        new Vector2Int(2, 3),
        new Vector2Int(4, 3),
    };



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
                        rend.material.color = Color.blue;
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
    
    public void TryPlayerRotateMaze(Vector3 playerPosition)
    {
        if (tutorialIsWallRotating || tutorialRotationsUsed >= tutorialMaxRotations) return;

        foreach (GameObject wall in wallList)
        {
            RotateAndMoveWall(wall);
        }

        tutorialRotationsUsed++;
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
        if (tutorialMessageText != null)
        {
            tutorialMessageText.text = "This wall is blocking your path.\nPress E to rotate the walls!";
            tutorialMessageText.gameObject.SetActive(true);
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

        if (tutorialMessageText != null)
        {
            tutorialMessageText.text = "Great! You rotated the wall!";
            yield return new WaitForSeconds(2f);
            tutorialMessageText.gameObject.SetActive(false);
        }

        playerController.enabled = true;
    }

}
