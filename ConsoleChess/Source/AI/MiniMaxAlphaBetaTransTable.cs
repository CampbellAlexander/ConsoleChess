using ConsoleChess.Source.Game;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ConsoleChess.Source.AI
{
    public sealed class MiniMaxAlphaBetaTransTable
    {
        private int maxDepth;

        private Dictionary<long, GamestateData> trans = new Dictionary<long, GamestateData>(35000000);
        private int transCutoffs = 0;
        private int alphaBetaCutoffs = 0;

        private const int NEGINFINITY = -99999;
        private const int POSINFINITY = 99999;

        private class MoveScore
        {
            public Move move;
            public int score;

            public MoveScore(Move move, int score)
            {
                this.move = move;
                this.score = score;
            }
        }
        private class GamestateData
        {
            public int shallowestDepthFound;
            public int score;
            public bool scoreFinalized;

            public GamestateData(int shallowestDepthFound)
            {
                this.shallowestDepthFound = shallowestDepthFound;
            }
        }




        /// <summary>
        /// Calculates and returns the move yielding the highest
        /// overall utility value when looking maxDepth moves
        /// into the future.
        /// </summary>
        /// <param name="game">An instance of a MiniMax problem</param>
        /// <param name="maxDepth">The maximum depth to search, must be > 0</param>
        /// <returns></returns>
        public Move GetBestMove(MiniMaxProblem game, int maxDepth)
        {
            this.maxDepth = maxDepth;

            trans.Clear();
            transCutoffs = 0;
            alphaBetaCutoffs = 0;
            trans.Add(game.GetHashKey(), new GamestateData(0));

            int alpha = NEGINFINITY;
            int aiPlyr = game.GetCurrentPlayer();
            MoveScore currentBest = new MoveScore(null, NEGINFINITY); //reference value
            Stopwatch s = new Stopwatch();
            s.Start();
            foreach (Move move in game.GetLegalMoves())
            {
                game.DoMove(move);
                currentBest = GetBest(currentBest, MinMove(game, aiPlyr, 1, alpha, POSINFINITY));
                game.UndoMove();
                alpha = System.Math.Max(alpha, currentBest.score);
            }
            s.Stop();

            Console.WriteLine("Algorithm time: " + s.ElapsedTicks);
            Console.WriteLine("The value of the move the AI picked is..........." + currentBest.score);
            Console.WriteLine("the number of transpose table cuttoffs is........" + transCutoffs);
            Console.WriteLine("The number of entries in the transpose table is.." + trans.Count);
            Console.WriteLine("The number of alpha-beta cutoffs is.............." + alphaBetaCutoffs);
            return currentBest.move;
        }

        


        private MoveScore MaxMove(MiniMaxProblem game, int aiPlyr, int currDepth, int alpha, int beta)
        {
            long hash = game.GetHashKey();
            Move lastMove = game.GetLastMove();


            if (trans.ContainsKey(hash))
            {
                if (trans[hash].shallowestDepthFound > currDepth)
                {
                    trans[hash].shallowestDepthFound = currDepth;
                    trans[hash].scoreFinalized = false;
                }
                else if (trans[hash].scoreFinalized)
                {
                    transCutoffs++;
                    return new MoveScore(lastMove, trans[hash].score);
                }
                else
                {
                    transCutoffs++;
                    return new MoveScore(lastMove, POSINFINITY);
                }
            }
            else trans.Add(hash, new GamestateData(currDepth));


            if (game.IsGameOver() || currDepth == maxDepth)
            {
                int utility = game.Utility(aiPlyr);
                trans[hash].score = utility;
                trans[hash].scoreFinalized = true;
                return new MoveScore(lastMove, utility);
            }


            MoveScore currentBest = new MoveScore(null, NEGINFINITY); //reference value
            foreach (Move move in game.GetLegalMoves())
            {
                game.DoMove(move);
                currentBest = GetBest(currentBest, MinMove(game, aiPlyr, currDepth + 1, alpha, beta));
                game.UndoMove();

                alpha = System.Math.Max(alpha, currentBest.score);
                if (alpha >= beta)
                {
                    alphaBetaCutoffs++;
                    currentBest.move = lastMove;
                    return currentBest;
                }
            }
            currentBest.move = lastMove;
            trans[hash].score = currentBest.score;
            trans[hash].scoreFinalized = true;
            return currentBest;
        }




        private MoveScore MinMove(MiniMaxProblem game, int aiPlyr, int currDepth, int alpha, int beta)
        {
            long hash = game.GetHashKey();
            Move lastMove = game.GetLastMove();

            if (trans.ContainsKey(hash))
            {
                if (trans[hash].shallowestDepthFound > currDepth)
                {
                    trans[hash].shallowestDepthFound = currDepth;
                    trans[hash].scoreFinalized = false;
                }
                else if (trans[hash].scoreFinalized)
                {
                    transCutoffs++;
                    return new MoveScore(lastMove, trans[hash].score);
                }
                else
                {
                    transCutoffs++;
                    return new MoveScore(lastMove, NEGINFINITY);
                }
            }
            else trans.Add(hash, new GamestateData(currDepth));





            if (game.IsGameOver() || currDepth == maxDepth)
            {
                int utility = game.Utility(aiPlyr);
                trans[hash].score = utility;
                trans[hash].scoreFinalized = true;
                return new MoveScore(lastMove, utility);
            }


            MoveScore currentWorst = new MoveScore(null, POSINFINITY); //reference value
            foreach (Move move in game.GetLegalMoves())
            {
                game.DoMove(move);
                currentWorst = GetWorst(currentWorst, MaxMove(game, aiPlyr, currDepth + 1, alpha, beta));
                game.UndoMove();

                beta = System.Math.Min(beta, currentWorst.score);
                if (alpha >= beta)
                {
                    alphaBetaCutoffs++;
                    currentWorst.move = lastMove;
                    return currentWorst;
                }
            }
            currentWorst.move = lastMove;
            trans[hash].score = currentWorst.score;
            trans[hash].scoreFinalized = true;
            return currentWorst;
        }



        // out of two moves with the same score, the AI prefers the first one found. (arbitrary)
        private static MoveScore GetBest(MoveScore mv1, MoveScore mv2)
        {
            return mv2.score > mv1.score ? mv2 : mv1;
        }
        private static MoveScore GetWorst(MoveScore mv1, MoveScore mv2)
        {
            return mv2.score < mv1.score ? mv2 : mv1;
        }
    }
}
