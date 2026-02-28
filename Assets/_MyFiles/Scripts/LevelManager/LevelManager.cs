using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    // Using an Enum makes our grid much easier to read than just numbers
    public enum Tile { Wall, Floor, Start, End }

    [Header("Grid Settings")]
    [SerializeField] private int gridWidth = 30;
    [SerializeField] private int gridHeight = 30;

    [Header("Generation Rules")]
    [SerializeField] private int maxSteps = 500; // How many tiles the drunkard will carve
    [SerializeField] private Vector2Int startPos = new Vector2Int(1, 1); // Manually set Start
    [SerializeField] private Vector2Int endPos = new Vector2Int(25, 25); // Manually set End

    // This is the "brain" of our level. A 2D array of Tiles.
    [SerializeField] private Tile[,] grid;

    [Header("Prefabs")]
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private float tileSize = 10f; // Each tile is 10 units wide

    void Start()
    {
        // Kick off the generation when the game starts
        StartCoroutine(GenerateLevelCoroutine());
    }
    public System.Collections.IEnumerator GenerateLevelCoroutine()
    {
       
        // 0. CLEANUP: Delete all existing walls and floors before building new ones
        // We use a while loop because destroying objects while looping through them 
        // with 'foreach' can sometimes skip items.
        while (transform.childCount > 0)
        {
            Transform child = transform.GetChild(0);

            // Option A: Make it invisible immediately so the player sees it's gone
            child.gameObject.SetActive(false);

            // THIS IS THE FIX: Instantly remove it from the parent so childCount goes down
            child.SetParent(null);

            // Option B: Rename it so you don't get confused in the hierarchy
            child.name = "DELETING...";

            Destroy(child.gameObject);
        }

        bool levelIsFinished = false;
        int attempts = 0;

        while (!levelIsFinished && attempts < 1000)
        {
            attempts++;
            InitializeGrid();

            grid[startPos.x, startPos.y] = Tile.Start;
            grid[endPos.x, endPos.y] = Tile.End;

            StampRoom(startPos);
            StampRoom(endPos);

            CarveDrunkardPath();
            CreateLoops();

            if (IsLevelPossible())
            {
                levelIsFinished = true;
            }
            else
            {
                // Only pause every 20 attempts. 
                // This makes generation 20x faster while still preventing crashes!
                if (attempts % 20 == 0)
                {
                    yield return null;
                }
            }
        }

        // 3. FINALIZATION (Only happens once the loop is done)
        if (levelIsFinished)
        {
            Spawn3DModels();
            Debug.Log($"Map successful on attempt {attempts}");

            GameObject vrRig = GameObject.FindWithTag("Player");
            if (vrRig != null)
            {
                // tileSize should be the size of your floor prefabs (e.g., 4 or 10)
                Vector3 spawnPos = transform.position + new Vector3(startPos.x * tileSize, 1.0f, startPos.y * tileSize);
                vrRig.transform.position = spawnPos;
            }
        }
        else
        {
            Debug.LogError("FAILED: Max attempts reached without finding a path.");
        }
    }

    void InitializeGrid()
    {
        grid = new Tile[gridWidth, gridHeight];

        // Loop through every X and Y coordinate and set it to a Wall
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                grid[x, y] = Tile.Wall;
            }
        }
    }

    void CarveDrunkardPath()
    {
        // 1. The "Spine" (Ensures the level is beatable)
        // Uses the full maxSteps (e.g., 200) to get from Start to End
        RunDrunkard(startPos, maxSteps, 0.75f);

        // 2. The "Chaos Maker" (Builds the maze)
        // Starts in the middle, but we only give it HALF the steps so it doesn't eat the whole map
        Vector2Int midPoint = new Vector2Int(gridWidth / 2, gridHeight / 2);
        RunDrunkard(midPoint, maxSteps / 2, 0.10f);

        // 3. The "Flanker" (Builds side routes)
        // Starts at a random point, gets HALF the steps
        Vector2Int randomPoint = new Vector2Int(Random.Range(5, gridWidth - 5), Random.Range(5, gridHeight - 5));
        while (randomPoint == startPos || randomPoint == endPos)
        {
            randomPoint = new Vector2Int(Random.Range(5, gridWidth - 5), Random.Range(5, gridHeight - 5));
        }
        RunDrunkard(randomPoint, maxSteps / 2, 0.20f);

        // 4. CONTROLLED ROOM STAMPING
        // Always ensure the Start and End rooms are large enough to spawn in
        StampRoom(startPos);
        StampRoom(endPos);

        // Explicitly stamp exactly 3 random "Arenas" on the map
        for (int i = 0; i < 3; i++)
        {
            Vector2Int randomArena = new Vector2Int(Random.Range(3, gridWidth - 3), Random.Range(3, gridHeight - 3));

            // EDGE CASE PREVENTION: Check the distance!
            // If the arena is too close to the Start OR the End, roll the dice again.
            while (Vector2.Distance(randomArena, startPos) < 5f || Vector2.Distance(randomArena, endPos) < 5f)
            {
                randomArena = new Vector2Int(Random.Range(3, gridWidth - 3), Random.Range(3, gridHeight - 3));
            }

            StampRoom(randomArena);
        }
    }

    void RunDrunkard(Vector2Int start, int steps, float bias)
    {
        Vector2Int currentPos = start;

        for (int i = 0; i < steps; i++)
        {
            if (Random.value < bias)
            {
                if (Random.value < 0.5f && currentPos.x != endPos.x)
                    currentPos.x += (endPos.x > currentPos.x) ? 1 : -1;
                else if (currentPos.y != endPos.y)
                    currentPos.y += (endPos.y > currentPos.y) ? 1 : -1;
            }
            else
            {
                int rand = Random.Range(0, 4);
                if (rand == 0) currentPos.y += 1;
                else if (rand == 1) currentPos.y -= 1;
                else if (rand == 2) currentPos.x -= 1;
                else if (rand == 3) currentPos.x += 1;
            }

            currentPos.x = Mathf.Clamp(currentPos.x, 1, gridWidth - 2);
            currentPos.y = Mathf.Clamp(currentPos.y, 1, gridHeight - 2);

            if (grid[currentPos.x, currentPos.y] == Tile.Wall)
            {
                grid[currentPos.x, currentPos.y] = Tile.Floor;
            }

            // NOTICE: I removed the 1% StampRoom from here! 
            // All rooms are now handled safely in CarveDrunkardPath.
        }
    }

    bool IsLevelPossible()
    {
        // A HashSet is just a list that doesn't allow duplicates—perfect for "Visited" tiles
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Queue<Vector2Int> checkNext = new Queue<Vector2Int>();

        // Start the flood fill at the Start position
        checkNext.Enqueue(startPos);
        visited.Add(startPos); // Mark start as visited immediately

        bool foundEnd = false;

        while (checkNext.Count > 0)
        {
            Vector2Int current = checkNext.Dequeue();

            // We found the end! BUT we do NOT return true yet. 
            // We must let the flood fill keep going to map the entire main island.
            if (current == endPos)
            {
                foundEnd = true;
            }

            // Check the 4 neighbors
            Vector2Int[] neighbors = {
                current + Vector2Int.up,    // (x, y+1)
                current + Vector2Int.down,  // (x, y-1)
                current + Vector2Int.left,  // (x-1, y)
                current + Vector2Int.right  // (x+1, y)
            };

            foreach (Vector2Int neighbor in neighbors)
            {
                // Only add to the check list if it's within bounds
                if (neighbor.x >= 0 && neighbor.x < gridWidth && neighbor.y >= 0 && neighbor.y < gridHeight)
                {
                    // If it's a Floor/Start/End and we haven't visited it yet
                    if (grid[neighbor.x, neighbor.y] != Tile.Wall && !visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        checkNext.Enqueue(neighbor);
                    }
                }
            }
        }

        // If the flood fill finished and we NEVER saw the End, the map is broken. Restart.
        if (!foundEnd) return false;

        // --- THE ISLAND CULLER ---
        // If we made it here, the main path is valid. 
        // Now we scan the grid and delete any islands that the flood fill couldn't reach!
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] == Tile.Floor)
                {
                    // If this floor tile isn't in our 'visited' list, it's an isolated island!
                    if (!visited.Contains(new Vector2Int(x, y)))
                    {
                        grid[x, y] = Tile.Wall; // Erase it!
                    }
                }
            }
        }

        return true;
    }

    //Fixes Deadends
    void CreateLoops()
    {
        // Start at 2 to avoid breaking the outer boundary walls
        for (int x = 2; x < gridWidth - 2; x++)
        {
            for (int y = 2; y < gridHeight - 2; y++)
            {
                if (grid[x, y] == Tile.Floor)
                {
                    int wallCount = 0;
                    if (grid[x + 1, y] == Tile.Wall) wallCount++;
                    if (grid[x - 1, y] == Tile.Wall) wallCount++;
                    if (grid[x, y + 1] == Tile.Wall) wallCount++;
                    if (grid[x, y - 1] == Tile.Wall) wallCount++;

                    // If it's a dead end (3 walls surrounding 1 floor)
                    if (wallCount >= 3)
                    {
                        // Check if breaking a wall connects to another path nearby
                        // This creates a "loop" instead of just a deeper dead end
                        if (x + 2 < gridWidth && grid[x + 2, y] == Tile.Floor) grid[x + 1, y] = Tile.Floor;
                        else if (x - 2 > 0 && grid[x - 2, y] == Tile.Floor) grid[x - 1, y] = Tile.Floor;
                        else if (y + 2 < gridHeight && grid[x, y + 2] == Tile.Floor) grid[x, y + 1] = Tile.Floor;
                        else if (y - 2 > 0 && grid[x, y - 2] == Tile.Floor) grid[x, y - 1] = Tile.Floor;
                    }
                }
            }
        }
    }

    void Spawn3DModels()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                // Calculate the 3D position based on grid coordinates
                // Math: Grid Index * Tile Size = World Position
                Vector3 pos = new Vector3(x * tileSize, 0, y * tileSize);

                if (grid[x, y] == Tile.Floor || grid[x, y] == Tile.Start || grid[x, y] == Tile.End)
                {
                    // Always spawn a floor
                    Instantiate(floorPrefab, pos, Quaternion.identity, this.transform);

                    // Check neighbors to see if we need to spawn a wall
                    CheckAndSpawnWall(x, y, Vector2Int.right, 90);  // Right
                    CheckAndSpawnWall(x, y, Vector2Int.left, 270); // Left
                    CheckAndSpawnWall(x, y, Vector2Int.up, 0);     // Forward
                    CheckAndSpawnWall(x, y, Vector2Int.down, 180); // Backward
                }
            }
        }
    }

    void CheckAndSpawnWall(int x, int y, Vector2Int direction, float rotationY)
    {
        int checkX = x + direction.x;
        int checkY = y + direction.y;

        // 1. Boundary Check: If the neighbor is outside the array, it's effectively a wall
        bool isOutOfBounds = (checkX < 0 || checkX >= gridWidth || checkY < 0 || checkY >= gridHeight);

        // 2. Logic: If it's out of bounds OR the neighbor is a Wall tile, spawn a wall
        if (isOutOfBounds || grid[checkX, checkY] == Tile.Wall)
        {
            // Calculate position: Start at floor center, then move half-tile in the direction
            Vector3 floorPos = new Vector3(x * tileSize, 0, y * tileSize);

            // If tileSize is 10, the edge is at 5. 
            // We move 5 units in the direction of the wall (Right, Left, Up, or Down).
            Vector3 wallOffset = new Vector3(direction.x * (tileSize / 2f), 2.5f, direction.y * (tileSize / 2f));
            Vector3 wallPos = floorPos + wallOffset;

            // The 'transform' at the end makes the level manager the parent of all walls/floors
            Instantiate(wallPrefab, wallPos, Quaternion.Euler(0, rotationY, 0), this.transform);
        }
    }
    void StampRoom(Vector2Int center)
    {
        // Loop from -1 to 1 on both axes to create a 3x3 square
        for (int xOffset = -1; xOffset <= 1; xOffset++)
        {
            for (int yOffset = -1; yOffset <= 1; yOffset++)
            {
                int targetX = center.x + xOffset;
                int targetY = center.y + yOffset;

                // Safety check: Don't stomp outside the grid or over the Start/End
                if (targetX > 0 && targetX < gridWidth - 1 && targetY > 0 && targetY < gridHeight - 1)
                {
                    if (grid[targetX, targetY] == Tile.Wall)
                    {
                        grid[targetX, targetY] = Tile.Floor;
                    }
                }
            }
        }
    }
}