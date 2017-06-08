using ConsoleChess.Source.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleChess.Source.AI
{
    public interface MiniMaxProblem
    {
        int GetCurrentPlayer();
        bool IsGameOver();
        int Utility(int plyr);
        List<Move> GetLegalMoves();
        void DoMove(Move m);
        void UndoMove();
        Move GetLastMove();
        long GetHashKey();
    }
}
