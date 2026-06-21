using UnityEngine;
using ConnectFour;
using System.Collections.Generic;

public class RuleBasedAgent : Agent
{
    public bool EnableForks = true;

    public override int GetMove(Connect4State state)
    {
        int player = state.GetPlayerTurn();
        int opponent = 1 - player;
        int move;
        //Debug.Log("EnableForks: " + EnableForks);

        move = FindBlockingMove(state, opponent);
        if (move != -1)
        {
            // Debug.Log("Blocking opponent's winning move: " + move);
            return move;
        }


        move = FindWinningMove(state, player);
        if (move != -1)
        {
            // Debug.Log("Making winning move: " + move);
            return move;
        }


        if (EnableForks)
        {
            move = FindForkMove(state, player);
            if (move != -1)
            {
                // Debug.Log("Creating fork: " + move);
                return move;
            }
        }


        List<int> moves = state.GetPossibleMoves();
        move = moves[Random.Range(0, moves.Count)];
        // Debug.Log("Random move: " + move);
        return move;
    }

    // Find the winning move
    private int FindWinningMove(Connect4State state, int player)
    {
        List<int> moves = state.GetPossibleMoves();

        for (int i = 0; i < moves.Count; i++)
        {
            Connect4State tempState = state.Clone();
            tempState.SetPlayerTurn(player);
            tempState.MakeMove(moves[i]);

            if (tempState.GetResult() == (player == 0 ? Connect4State.Result.YellowWin : Connect4State.Result.RedWin))
            {
                return moves[i];
            }
        }

        return -1;
    }

    // Find a move that blocks the opponent's winning move
    private int FindBlockingMove(Connect4State state, int opponent)
    {
        List<int> moves = state.GetPossibleMoves();

        foreach (int move in moves)
        {

            Connect4State tempState = state.Clone();
            tempState.SetPlayerTurn(opponent);
            tempState.MakeMove(move);


            if (tempState.GetResult() == (opponent == 0 ? Connect4State.Result.YellowWin : Connect4State.Result.RedWin))
            {
                return move;
            }
        }

        return -1;
    }


    // Find a potential fork move (two winning threats)
    private int FindForkMove(Connect4State state, int player)
    {
        List<int> moves = state.GetPossibleMoves();

        for (int i = 0; i < moves.Count; i++)
        {
            Connect4State tempState = state.Clone();
            tempState.SetPlayerTurn(player);
            tempState.MakeMove(moves[i]);
            int winningCount = 0;

            List<int> tempMoves = tempState.GetPossibleMoves();
            for (int j = 0; j < tempMoves.Count; j++)
            {
                Connect4State tempState2 = tempState.Clone();
                tempState2.SetPlayerTurn(player);
                tempState2.MakeMove(tempMoves[j]);

                if (tempState2.GetResult() == (player == 0 ? Connect4State.Result.YellowWin : Connect4State.Result.RedWin))
                {
                    winningCount++;
                }
            }

            if (winningCount >= 2)
            {
                return moves[i];
            }
        }

        return -1;
    }
}
