using UnityEngine;
using TMPro;

public class HeuristicSelector : MonoBehaviour
{
    public TMP_Dropdown dropdown;
    public Pathfinding pathfindingScript;
    public Frog frogScript;

    private HeuristicTypes lastHeuristic = HeuristicTypes.Euclidean;

    void Update()
    {
        UpdateHeuristicState();
    }

    private void UpdateHeuristicState()
    {
        if (frogScript.controlType == Frog.ControlType.AI)
        {

            Vector2 currentPos = frogScript.transform.position;
            Vector2 targetPos = frogScript.GetCurrentTarget();

            HeuristicTypes[] allHeuristics = { HeuristicTypes.Manhattan, HeuristicTypes.Diagonal, HeuristicTypes.Euclidean };

            float bestLength = float.MaxValue;
            HeuristicTypes best = pathfindingScript.heuristicNow;

            foreach (HeuristicTypes h in allHeuristics)
            {
                pathfindingScript.heuristicNow = h;
                Node[] testPath = Pathfinding.RequestPath(currentPos, targetPos);

                float length = CalculatePathLength(testPath, h);
                if (testPath.Length > 0 && length < bestLength)
                {
                    bestLength = length;
                    best = h;
                }
            }
            if (best != lastHeuristic)
            {
                lastHeuristic = best;
            }

            pathfindingScript.heuristicNow = best;

        }
        else if (frogScript.controlType == Frog.ControlType.Human)
        {
            pathfindingScript.heuristicNow = (HeuristicTypes)dropdown.value;
            if (pathfindingScript.heuristicNow != lastHeuristic)
            {
                // Debug.Log($"Heuristic set to: {pathfindingScript.heuristicNow}");
                lastHeuristic = (HeuristicTypes)dropdown.value;
            }


            //lastHeuristic = (HeuristicTypes)dropdown.value;
        }
    }

    private float CalculatePathLength(Node[] path, HeuristicTypes h)
    {
        float total = 0f;
        for (int i = 1; i < path.Length; i++)
        {
            total += pathfindingScript.GCost(path[i - 1], path[i], h);
        }
        return total;
    }
}