using UnityEngine;

public class GridManager : MonoBehaviour
{
    public GameObject tilePrefab; // Assign a basic cube in Unity
    public int gridSize = 6;
    public float tileSize = 1.1f; // Spacing between tiles
    public GameObject wallPrefab;  // Assign in Inspector

    private Tile[,] tiles;

    //Basically you have to read the maze from the top left to the bottom right 
    // Define walls for each tile (Manually set these values for your custom maze)
    // North, West,East, South 
    // Look from the top left that start as the first array element 
    /// <summary>
    /// 3D Boolean array representing walls in a 6x6 maze grid.
    /// Each tile in the grid has four possible walls: North, West, East, and South.
    /// A value of 'true' means a wall is present, and 'false' means no wall.
    /// The array is structured as gridWalls[x, y, direction], where:
    ///   - x: X-coordinate (horizontal position in the grid)
    ///   - y: Y-coordinate (vertical position in the grid)
    ///   - direction: 
    ///       0 → North wall (above the tile)
    ///       1 → West wall (left of the tile)
    ///       2 → East wall (right of the tile)
    ///       3 → South wall (below the tile)
    ///
    /// Example:
    ///   gridWalls[0,0,2] = true → Tile at (0,0) has a wall on its east side.
    ///   gridWalls[2,3,0] = false → Tile at (2,3) has no wall to its north.
    ///
    /// The maze is manually designed, read from top-left to bottom-right.
    /// </summary>
    private bool[,,] gridWalls = new bool[6,6,4] {
        { { false, false, true,false}, { false, false, true, true }, { true, false, false, false }, { false, false, false, true }, { false, false, true, false }, { false, true, false, true } },
        { { false, true, false, true }, { false, false, false, false }, { false, false, false, false }, { true, true, true, false }, { false, false, false, true }, {false, false, false, false } },
        { { true, false, false, false }, { true, false, true, false }, { false, false, true, true }, { false, false, true, false }, { false, false, false, false }, { false, false, false, true } },
        { { true, false, true, false }, { false, false, false, true }, { false, false, false, false }, { false, false, true, true }, { false, false, false, false }, { true, true, false,true } },
        { { false, false, false, true }, { false, false, true, false }, { false, false, false, true }, { false, false, true, false }, { false, false, false, true }, { false, false, true, false } },
        { { true, false, true, false }, { false, false, false, false }, { false, false, false, false }, { false, true, false, false }, { false, false, false, false }, { false, true, false, false } }
    };


    //In the beginnning of scene, we add a grid of 6x
    void Start()
    {
        GenerateGrid();
    }
    // #if UNITY_EDITOR
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            GenerateGrid();
        }
    }
#endif



    /// <summary>
    /// Generates a grid of tiles and places walls based on the predefined gridWalls array.
    /// Each tile is instantiated at a specific (x, y) position, and walls are added according to 
    /// the boolean values in gridWalls[x, y, direction].
    /// </summary>
    void GenerateGrid()
    {
        ClearGrid(); // Remove old tiles before creating new ones

        tiles = new Tile[gridSize, gridSize];

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                // Instantiate Tile and Set Parent
                GameObject tileGO = Instantiate(tilePrefab, new Vector3(x, 0, y), Quaternion.identity, transform);
                tileGO.name = $"Tile ({x}, {y})";
                Tile tile = tileGO.AddComponent<Tile>();

                // Set tile properties
                tile.Initialize(new Vector2Int(x, y),
                    gridWalls[x, y, 0], // North wall
                    gridWalls[x, y, 1], // West wall
                    gridWalls[x, y, 2], // East wall
                    gridWalls[x, y, 3]  // South wall
                );

                tiles[x, y] = tile;

                // Add walls and set them as children of the tile
                if (gridWalls[x, y, 0]) // North Wall (top edge)
                    AddWall(tileGO, new Vector3(-0.5f, 2.5f, 0), Quaternion.identity);

                if (gridWalls[x, y, 3]) // South Wall (bottom edge)
                    AddWall(tileGO, new Vector3(0.5f, 2.5f, 0), Quaternion.identity);

                if (gridWalls[x, y, 1]) // West Wall (left edge)
                    AddWall(tileGO, new Vector3(0, 2.5f, -0.5f), Quaternion.Euler(0, 90, 0));

                if (gridWalls[x, y, 2]) // East Wall (right edge)
                    AddWall(tileGO, new Vector3(0, 2.5f, 0.5f), Quaternion.Euler(0, 90, 0));
            }
        }
    }

    /// <summary>
    /// Adds a wall as a child object of the given tile and positions it based on local coordinates.
    /// The local position is defined relative to the tile, meaning walls will be placed correctly 
    /// regardless of where the tile is in the grid.
    /// </summary>
    /// <param name="parent">The tile GameObject that the wall should be attached to.</param>
    /// <param name="localPosition">The position of the wall relative to the tile's local origin.</param>
    /// <param name="rotation">The rotation of the wall, used to align walls correctly.</param>
    void AddWall(GameObject parent, Vector3 localPosition, Quaternion rotation)
    {
        GameObject wall = Instantiate(wallPrefab, parent.transform);
        wall.transform.localPosition = localPosition;
        wall.transform.localRotation = rotation;

        // Log local position relative to parent
        Debug.Log($"Wall Local Position: {localPosition} relative to {parent.name}");

        // Log global position after setting local position
        Debug.Log($"Wall Global Position: {wall.transform.position}");
    }

    /// <summary>
    /// Clears the existing maze by destroying all children under GridManager.
    /// This ensures we don't duplicate tiles when regenerating.
    /// </summary>
    void ClearGrid()
    {
        Debug.Log("Clearing old grid...");

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;
            Debug.Log($"Destroyinggg: {child.name}");

            if (Application.isPlaying)
            {
                Destroy(child);  // Safe to use in Play Mode
            }
            else
            {
                DestroyImmediate(child);  // Required for Edit Mode
            }
        }

        Debug.Log("Grid cleared.");
    }



}