// Adapted from: https://github.com/SebLague/Pathfinding-2D

using UnityEngine;
using System.Collections.Generic;

public class AStarGrid : MonoBehaviour
{
    public bool displayGridGizmos;
    public LayerMask unwalkableMask;
    public LayerMask mudMask;
    public LayerMask waterMask;
    public Vector2 gridWorldSize;
    public float gridSize;
    public float overlapCircleRadius;

    // TODO: This is unused, and can be added as part of the workshop
    public bool includeDiagonalNeighbours;

    Node[,] grid;
    float nodeDiameter;
    static int gridSizeX, gridSizeY;

    

    void Awake()
    {
        nodeDiameter = gridSize * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        unwalkableMask = LayerMask.GetMask("Obstacle");
        mudMask = LayerMask.GetMask("Mud");
        waterMask = LayerMask.GetMask("Water");

        CreateGrid();
    }

    public int MaxSize
    {
        get
        {
            return gridSizeX * gridSizeY;
        }
    }

    public void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];
        Vector2 worldBottomLeft = (Vector2)transform.position - Vector2.right * gridWorldSize.x / 2 - Vector2.up * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector2 worldPoint = worldBottomLeft + Vector2.right * (x * nodeDiameter + gridSize) + Vector2.up * (y * nodeDiameter + gridSize);

                bool walkable = Physics2D.OverlapCircle(worldPoint, overlapCircleRadius, unwalkableMask) == null;
                bool isMud = Physics2D.OverlapPoint(worldPoint, mudMask) != null;
                bool isWater = Physics2D.OverlapPoint(worldPoint, waterMask) != null;

                grid[x, y] = new Node(walkable, isMud, isWater, worldPoint, x, y);

                if (grid[x,y].walkable)
                {
                    FruitsBehaviour.fruitNodes.Add(grid[x, y]);
                }
            }
        }
    }
    public void UpdateDynamicObstacles(List<Transform> dynamicObstacles, float radius)
    {
        foreach (Node node in grid)
        {
            if (Physics2D.OverlapCircle(node.worldPosition, overlapCircleRadius, unwalkableMask) != null)
                node.walkable = false;
            else
                node.walkable = true;
        }

        foreach (Transform obstacle in dynamicObstacles)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    Node node = grid[x, y];
                    if (Vector2.Distance(obstacle.position, node.worldPosition) < radius)
                    {
                        node.walkable = false;
                    }
                }
            }
        }
    }

    public List<Node> GetNeighbours(Node node)
    {
        // TODO: Update this method to include diagonal neighbours if includeDiagonalNeighbours == true.

        List<Node> neighbours = new List<Node>();

        // Diagonal
        if (includeDiagonalNeighbours)
        {
            // Up-Left
            if (node.gridX - 1 > 0 && node.gridY - 1 > 0)
            {
                neighbours.Add(grid[node.gridX - 1, node.gridY - 1]);
            }

            // Up-Right
            if (node.gridX + 1 < gridSizeX && node.gridY - 1 > 0)
            {
                neighbours.Add(grid[node.gridX + 1, node.gridY - 1]);
            }

            // Down-Left
            if (node.gridX - 1 > 0 && node.gridY + 1 < gridSizeY)
            {
                neighbours.Add(grid[node.gridX - 1, node.gridY + 1]);
            }

            // Down-Right
            if (node.gridX + 1 < gridSizeX && node.gridY + 1 < gridSizeY)
            {
                neighbours.Add(grid[node.gridX + 1, node.gridY + 1]);
            }
        }
        else
        {
            // Left
            if (node.gridX - 1 > 0)
            {
                neighbours.Add(grid[node.gridX - 1, node.gridY]);
            }

            // Right
            if (node.gridX + 1 < gridSizeX)
            {
                neighbours.Add(grid[node.gridX + 1, node.gridY]);
            }

            // Up
            if (node.gridY - 1 > 0)
            {
                neighbours.Add(grid[node.gridX, node.gridY - 1]);
            }

            // Down
            if (node.gridY + 1 < gridSizeY)
            {
                neighbours.Add(grid[node.gridX, node.gridY + 1]);
            }
        }

        return neighbours;
    }

    public Node NodeFromWorldPoint(Vector2 worldPosition)
    {
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.y + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        return grid[x, y];
    }

    public Node ClosestWalkableNode(Node node)
    {
        int maxRadius = Mathf.Max(gridSizeX, gridSizeY) / 2;
        for (int i = 1; i < maxRadius; i++)
        {
            Node n = FindWalkableInRadius(node.gridX, node.gridY, i);
            if (n != null)
            {
                return n;
            }
        }
        return null;
    }

    Node FindWalkableInRadius(int centreX, int centreY, int radius)
    {
        for (int i = -radius; i <= radius; i++)
        {
            int verticalSearchX = i + centreX;
            int horizontalSearchY = i + centreY;

            // Top
            if (InBounds(verticalSearchX, centreY + radius))
            {
                if (grid[verticalSearchX, centreY + radius].walkable)
                {
                    return grid[verticalSearchX, centreY + radius];
                }
            }

            // Bottom
            if (InBounds(verticalSearchX, centreY - radius))
            {
                if (grid[verticalSearchX, centreY - radius].walkable)
                {
                    return grid[verticalSearchX, centreY - radius];
                }
            }

            // Right
            if (InBounds(centreY + radius, horizontalSearchY))
            {
                if (grid[centreX + radius, horizontalSearchY].walkable)
                {
                    return grid[centreX + radius, horizontalSearchY];
                }
            }

            // Left
            if (InBounds(centreY - radius, horizontalSearchY))
            {
                if (grid[centreX - radius, horizontalSearchY].walkable)
                {
                    return grid[centreX - radius, horizontalSearchY];
                }
            }

        }

        return null;
    }

    bool InBounds(int x, int y)
    {
        return x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY;
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector2(gridWorldSize.x, gridWorldSize.y));

        if (grid != null && displayGridGizmos)
        {
            foreach (Node n in grid)
            {
                if (!n.walkable)
                {
                    Gizmos.color = Color.red;
                }
                else
                {
                    if (n.isMud)
                    {
                        Gizmos.color = Color.yellow;
                    }
                    else if (n.isWater)
                    {
                        Gizmos.color = Color.blue;
                    }
                    else
                    {
                        Gizmos.color = Color.white;
                    }
                }

                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
            }
        }
    }
}
