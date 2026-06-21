// Adapted from: https://github.com/SebLague/Pathfinding-2D

using UnityEngine;
using System.Collections.Generic;
using System;


public enum HeuristicTypes
{
    Manhattan = 0,
    Diagonal = 1,
    Euclidean = 2
}

public class Pathfinding : MonoBehaviour
{
    public static AStarGrid grid;
    static Pathfinding instance;
    public HeuristicTypes heuristicNow = HeuristicTypes.Euclidean;

    // This is used instead of Start() so that the
    // A* grid is only greated once when the game is launched
    void Awake()
    {
        grid = GetComponent<AStarGrid>();
        instance = this;
    }

    // Public callable method
    public static Node[] RequestPath(Vector2 from, Vector2 to)
    {
        return instance.FindPath(from, to);
    }


    // Internal private implementation
    Node[] FindPath(Vector2 from, Vector2 to)
    {
        // A* Waypoints to return
        Node[] waypoints = new Node[0];
        Node[] smoothed = new Node[0];

        // Set to true if a path is found
        bool pathSuccess = false;

        // Starting node point - selected from the A* Grid
        Node startNode = grid.NodeFromWorldPoint(from);

        // Goal node point - selected from the A* Grid
        Node targetNode = grid.NodeFromWorldPoint(to);

        // Ensure the starting node's parent is not null
        // Also let's us detect the start node if needed
        startNode.parent = startNode;

        // Niceity check to ensure the start and target nodes are walkable
        // by the frog (such as if you clock on a object)
        // If not, we find the closest walkable point in the grid
        if (!startNode.walkable)
        {
            startNode = grid.ClosestWalkableNode(startNode);
        }
        if (!targetNode.walkable)
        {
            targetNode = grid.ClosestWalkableNode(targetNode);
        }

        if (startNode.walkable && targetNode.walkable)
        {
            // A* Starts here!!!
            // TODO: Your job is to fill in the missing code below the marked comments

            // Track the open set of nodes to explore, as a heap sorted by the A* Cost
            Heap<Node> openSet = new Heap<Node>(grid.MaxSize);

            // Track closed set of all visited nodes
            HashSet<Node> closedSet = new HashSet<Node>();

            // TODO: Commence A* by adding the start node to the open set
            openSet.Add(startNode);

            // Stop if we have a path or run out of nodes to explore (means no path can be found!)
            while (!pathSuccess && openSet.Count > 0)
            {
                // TODO: Get the node with the lowest F cost from the open set
                //     and add it to the closed set
                Node lowestFCostNode = openSet.RemoveFirst();
                closedSet.Add(lowestFCostNode);

                // TODO: If we have reached the target node, we have found a path! (repalce false)
                if (lowestFCostNode == targetNode)
                {
                    pathSuccess = true;
                }
                else
                {
                    // TODO: Otherwise, explore the neighbours of the current node
                    //       You'll need to get all of the neighbours of the current node
                    //       and then loop through them to find the best path
                    Node[] neighbours = grid.GetNeighbours(lowestFCostNode).ToArray();
                    foreach (Node node in neighbours)
                    {
                        // TODO:If we can reach the neighbour and it is not in the closed set (repalce false)
                        if (node.walkable && !closedSet.Contains(node))
                        {
                            // TODO: Calculate the G Cost of the neighbour node
                            float G_Cost = lowestFCostNode.gCost + GCost(lowestFCostNode, node, heuristicNow);

                            // TODO: If the neighbour is not in the open set OR
                            //    the neighour was previously checked and the new G Cost is less than the previous G Cost 
                            //    (repalce false)
                            if (!openSet.Contains(node) ||
                                (!node.walkable && G_Cost < node.gCost))
                            {
                                // TODO: Set neightbour G Cost
                                node.gCost = G_Cost;

                                // TODO: Compute and set the H Cost for the neighbour
                                node.hCost = Heuristic(node, targetNode);

                                // TODO: Set the parent of the neighbour to the current node
                                node.parent = lowestFCostNode;

                                // TODO: Add neighbour to the open set, but need to check if the neighbour is already in the open set
                                // If not in the open set, then add to the heap
                                // If in the open set, then UDPATE the neighbour in the heap
                                if (!openSet.Contains(node))
                                {
                                    // TODO: (see above comment)
                                    openSet.Add(node);
                                }
                                else
                                {
                                    // TODO: (see above comment)
                                    openSet.UpdateItem(node);
                                }
                            }
                        }
                    }
                }
            }
        }

        // If we have a path, then actually get the path from the start to goal
        if (pathSuccess)
        {
            waypoints = RetracePath(startNode, targetNode);
            smoothed = GetSmoothPath(waypoints);
        }

        return smoothed;
    }

    Node[] GetSmoothPath(Node[] rawPath)
    {
        if (rawPath == null || rawPath.Length < 2)
        {
            return rawPath;
        }

        List<Node> smoothenedPath = new List<Node>();
        int currentNode = 0;

        smoothenedPath.Add(rawPath[currentNode]);

        while (currentNode < rawPath.Length - 1)
        {
            int furthestSafeNode = currentNode + 1;

            for (int i = currentNode + 1; i < rawPath.Length; i++)
            {
                Vector2 start = rawPath[currentNode].worldPosition;
                Vector2 end = rawPath[i].worldPosition;
                Vector2 direction = (end - start).normalized;
                float distance = Vector2.Distance(start, end);

                RaycastHit2D hit = Physics2D.CircleCast(start, 0.5f, direction, distance, LayerMask.GetMask("Obstacle"));

                if (!hit)
                {
                    furthestSafeNode = i;
                }
                else
                {
                    break;
                }
            }

            currentNode = furthestSafeNode;
            smoothenedPath.Add(rawPath[currentNode]);
        }

        return smoothenedPath.ToArray();
    }
    // Creates the actual A* Path from the start to the goal
    // TODO: Your job is to fill in the missing code below the marked comments
    Node[] RetracePath(Node startNode, Node endNode)
    {
        // Store the computed path
        List<Node> path = new List<Node>();

        // TODO: Commence retracing the path from the end node
        Node currentNode = endNode;

        // TODO: Loop while the current node isn't the start node (replace false)
        while (currentNode != startNode)
        {
            // TODO: Add the current node to the path
            path.Add(currentNode);

            // TODO: Set the current node to the parent of the current node
            currentNode = currentNode.parent;
        }

        // Convert this list to an array and reverse it
        Node[] waypoints = path.ToArray();
        Array.Reverse(waypoints);
        return waypoints;
    }

    public float GCost(Node nodeA, Node nodeB, HeuristicTypes heuristicType)
    {
        float cost = 1.0f;

        if (nodeB.isMud)
        {
            cost += 5.0f;
        }
        else if (nodeB.isWater)
        {
            cost -= 0.75f;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(nodeB.worldPosition, 1.0f, LayerMask.GetMask("Snake"));
        if (hits.Length > 0)
        {
            cost += 10.0f;
        }

        if (heuristicNow == HeuristicTypes.Manhattan)
        {
            cost *= 1.0f;

        }
        else if (heuristicNow == HeuristicTypes.Diagonal)
        {
            if (nodeA.gridX != nodeB.gridX && nodeA.gridY != nodeB.gridY)
            {
                cost *= 1.4f;
            }
        }
        else if (heuristicNow == HeuristicTypes.Euclidean)
        {
            cost *= 1.1f;
        }

        // Debug.Log(cost);
        return cost;

    }

    private float Heuristic(Node nodeA, Node nodeB)
    {
        int dx = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dy = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        // TODO: Implement A* heuristics, such as Manhattan distance or Euclidean distance
        if (heuristicNow == HeuristicTypes.Manhattan)
        {
            float distance = dx + dy;
            return distance;
        }
        else if (heuristicNow == HeuristicTypes.Diagonal)
        {
            float distance = Mathf.Max(dx, dy);
            return distance;
        }
        else
        {
            float distance = Vector2.Distance(nodeA.worldPosition, nodeB.worldPosition);
            return distance;
        }
    }
}