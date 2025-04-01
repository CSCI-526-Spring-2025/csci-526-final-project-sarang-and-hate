using UnityEngine;


/// <summary>
/// Represents an individual tile in the 6x6 maze grid.
/// Each tile can have up to 4 walls (North, West, East, and South).
/// </summary>
public class Tile : MonoBehaviour
{
    // Boolean values to indicate whether this tile has a wall in each direction.
    public bool hasNorthWall;
    public bool hasWestWall;
    public bool hasEastWall;
    public bool hasSouthWall;

    //for flashing animation for level 1 
    public Color originalColor;
    public Renderer tileRenderer;

    // Stores the tile's position in the grid (X, Y coordinates).
    public Vector2Int gridPosition;


        void Start()
    {
        tileRenderer = GetComponent<Renderer>();
        if (tileRenderer != null)
        {
            originalColor = tileRenderer.material.color;
        }
    }

    /// <summary>
    /// Initializes the tile with a specific position in the grid and its wall placements.
    /// </summary>
    /// <param name="position">The (x, y) coordinates of the tile in the grid.</param>
    /// <param name="north">True if there is a wall on the north side of this tile.</param>
    /// <param name="west">True if there is a wall on the west side of this tile.</param>
    /// <param name="east">True if there is a wall on the east side of this tile.</param>
    /// <param name="south">True if there is a wall on the south side of this tile.</param>
    public void Initialize(Vector2Int position, bool north, bool west, bool east, bool south)
    {
        gridPosition = position; // Store tile's grid location

        // Assign wall values based on the parameters
        hasNorthWall = north;
        hasWestWall = west;
        hasEastWall = east;
        hasSouthWall = south;

        tileRenderer = GetComponent<Renderer>();
        if (tileRenderer != null)
        {
            // Cache the original color as early as possible
            originalColor = tileRenderer.material.color;
        }

        // Debugging: Print tile info to ensure correct wall setup
        Debug.Log($"Tile {gridPosition}: North-{hasNorthWall}, West-{hasWestWall}, East-{hasEastWall}, South-{hasSouthWall}");
    }
}

