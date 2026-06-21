using UnityEngine;
using System.Collections.Generic;
using ConnectFour;

public class MCTSAgent : Agent
{
    public int totalSims = 2500;
    public float c = Mathf.Sqrt(2.0f);

    public override int GetMove(Connect4State state)
    {
        // TODO: Override this method with an MCTS implementation.
        // Currently, it just returns a random move.
        // You can add other methods to the class if you like.
        List<int> moves = state.GetPossibleMoves();

        MCTSNode root = new MCTSNode(state);
        int originalPlayer = state.GetPlayerTurn();

        for (int i = 0; i < totalSims; i++)
        {
            MCTSNode node = root;

            // Selection
            while (node.IsFullyExpanded() && node.children.Count > 0)
            {
                node = node.SelectBestChild(c);
            }

            // Expansion
            if (!node.IsFullyExpanded())
            {
                node = node.Expand();
            }

            // Simulation
            float result = node.SimulateRandomGame(node.state, originalPlayer);

            // Backpropagation
            while (node != null)
            {
                node.visits++;
                node.score += result;
                node = node.parent;
            }
        }

        // Choose best move
        MCTSNode bestChild = null;
        int mostVisits = -1;
        foreach (MCTSNode child in root.children)
        {
            if (child.visits > mostVisits)
            {
                bestChild = child;
                mostVisits = child.visits;
            }
        }

        // Match move that created bestChild
        foreach (int move in moves)
        {
            Connect4State simState = state.Clone();
            simState.MakeMove(move);
            if (CheckState(simState, bestChild.state))
            {
                return move;
            }
        }

        //Return a random move if no match found
        return moves[Random.Range(0, moves.Count)];
    }

    // Compare state fields
    private bool CheckState(Connect4State a, Connect4State b)
    {
        for (int x = 0; x < GameController.numColumns; x++)
        {
            for (int y = 0; y < GameController.numRows; y++)
            {
                if (a.GetFieldValue(x, y) != b.GetFieldValue(x, y))
                    return false;
            }
        }

        return a.GetPlayerTurn() == b.GetPlayerTurn();
    }
}
