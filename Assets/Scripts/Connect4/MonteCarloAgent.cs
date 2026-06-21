using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MonteCarloAgent : Agent
{
    public int totalSims = 2500;
    public override int GetMove(Connect4State state)
    {
        // TODO: Override this method with the logic described in class.
        // Currently, it just returns a random move.
        // You can add other methods to the class if you like.
        List<int> moves = state.GetPossibleMoves();
        int simsPerMove = totalSims / moves.Count;
        float[] opponentScores = new float[moves.Count];

        for (int i = 0; i < moves.Count; i++)
        {
            Connect4State playerState = state.Clone();
            playerState.MakeMove(moves[i]);
        
            float totalOpponentScore = 0f;
            for (int j = 0; j < simsPerMove; j++)
            {
                Connect4State simState = playerState.Clone();

                //simulate until game ends
                while (simState.GetResult() == Connect4State.Result.Undecided)
                {
                    List<int> possibleMoves = simState.GetPossibleMoves();
                    if (possibleMoves.Count == 0)
                    {
                        break;
                    } 

                    int randomMove = possibleMoves[Random.Range(0, possibleMoves.Count)];
                    simState.MakeMove(randomMove);
                }

                //store result from opponent's view
                totalOpponentScore += 1f - Connect4State.ResultToFloat(simState.GetResult());
            }

            //average out scores
            opponentScores[i] = totalOpponentScore / simsPerMove;
        }

        // return move with the lowest opponent score
        int bestMoveIndex = argMin(opponentScores);
        return moves[bestMoveIndex];
    }
}
