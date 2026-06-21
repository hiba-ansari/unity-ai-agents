using UnityEngine;
using System.Collections.Generic;

public class MCTSNode
{
    public Connect4State state;
    public MCTSNode parent;
    public List<MCTSNode> children;
    public int visits;
    public float score;
    public List<int> possibleMoves;

    public MCTSNode(Connect4State state, MCTSNode parent = null)
    {
        this.state = state;
        this.parent = parent;
        this.children = new List<MCTSNode>();
        this.visits = 0;
        this.score = 0.0f;
        this.possibleMoves = state.GetPossibleMoves();
    }

    // Check if the node is fully expanded
    public bool IsFullyExpanded()
    {
        return possibleMoves.Count == 0;
    }

    // Expand the node by creating a child for a possible move
    public MCTSNode Expand()
    {
        if (possibleMoves.Count == 0)
        {
            return null;
        }

        int move = possibleMoves[0];
        possibleMoves.RemoveAt(0);

        Connect4State nextState = state.Clone();
        nextState.MakeMove(move);

        MCTSNode childNode = new MCTSNode(nextState, this);
        children.Add(childNode);

        return childNode;
    }

    // Calculate the UCB1 value
    public float GetUCB1(float explorationConstant)
    {
        if (visits == 0)
        {
            return float.MaxValue;
        }

        float exploitation = score / visits;
        float exploration = explorationConstant * Mathf.Sqrt(Mathf.Log(parent.visits) / visits);

        return exploitation + exploration;
    }

    // Select the best child node based on UCB1
    public MCTSNode SelectBestChild(float explorationConstant)
    {
        MCTSNode bestChild = null;
        float bestValue = float.MinValue;

        foreach (MCTSNode child in children)
        {
            float ucb1Value = child.GetUCB1(explorationConstant);
            if (ucb1Value > bestValue)
            {
                bestValue = ucb1Value;
                bestChild = child;
            }
        }

        return bestChild;
    }

    // Random simulation of the game from a given state
    public float SimulateRandomGame(Connect4State simState, int originalPlayer)
    {
        Connect4State state = simState.Clone();

        while (state.GetResult() == Connect4State.Result.Undecided)
        {
            List<int> moves = state.GetPossibleMoves();
            int randomMove = moves[Random.Range(0, moves.Count)];
            state.MakeMove(randomMove);
        }

        float score = Connect4State.ResultToFloat(state.GetResult());
        if (originalPlayer == 0)
        {
            score = 1f - score;
        }

        return score;
    }
}
