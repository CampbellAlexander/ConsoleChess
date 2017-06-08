using ConsoleChess.Source.Game;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ConsoleChess.Source.AI
{
    /// <summary>
    /// *THE* ALGORITHM, all others are derivations of this correct one
    /// An algorithm to solve min-max zero-sum game problems. Using optimizations:
    /// alpha-beta cutoffs, transposition table, IDS, and always trying the best move first
    /// from previous iterations of IDS.
    /// 
    /// This particular version is the same as THE ALGORITHM, but it limites the amount
    /// of entries in the transpose table
    /// </summary>
    public sealed class MiniMaxAlphaBetaTransTableBestMoveFirstMemoryControlled
    {
        private int maxDepth;
        private int aiPlyr;

        private Dictionary<long, GamestateData> trans = new Dictionary<long, GamestateData>(transSize);
        private int transCutoffs = 0;
        private int alphaBetaCutoffs = 0;
        private const int transSize = 15000000; //change to suit memory needs

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
            public int highestMaxDepth;
            public int score;
            public bool scoreFinalized;
            public Move bestMove;

            public GamestateData(int shallowestDepthFound, int highestMaxDepth)
            {
                this.shallowestDepthFound = shallowestDepthFound;
                this.highestMaxDepth = highestMaxDepth;
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
            this.aiPlyr = game.GetCurrentPlayer();
            long hash = game.GetHashKey();

            trans.Clear();
            transCutoffs = 0;
            alphaBetaCutoffs = 0;
            trans.Add(hash, new GamestateData(0, 1));


            MoveScore currentBest = new MoveScore(null, NEGINFINITY); //reference value

            for (int i = 1; i <= maxDepth; i++)
            {
                this.maxDepth = i;
                int alpha = NEGINFINITY;
                List<Move> legalMoves = game.GetLegalMoves();
                if (trans[hash].bestMove != null)
                {
                    ReorderToBestFirst(legalMoves, trans[hash].bestMove);
                }
                currentBest = new MoveScore(null, NEGINFINITY); //reset each iteration
                foreach (Move move in legalMoves)
                {
                    game.DoMove(move);
                    currentBest = GetBest(currentBest, MinMove(game, 1, alpha, POSINFINITY));
                    game.UndoMove();
                    alpha = System.Math.Max(alpha, currentBest.score);
                }
                trans[hash].bestMove = currentBest.move;
            }



            Console.WriteLine("The value of the move the AI picked is..........." + currentBest.score);
            Console.WriteLine("the number of transpose table cuttoffs is........" + transCutoffs);
            Console.WriteLine("The number of entries in the transpose table is.." + trans.Count);
            Console.WriteLine("The number of alpha-beta cutoffs is.............." + alphaBetaCutoffs);

            return currentBest.move;
        }




        private MoveScore MaxMove(MiniMaxProblem game, int currDepth, int alpha, int beta)
        {
            long hash = game.GetHashKey();
            Move lastMove = game.GetLastMove();


            if (trans.ContainsKey(hash))
            {
                if (trans[hash].highestMaxDepth == maxDepth)
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
                else
                {
                    trans[hash].highestMaxDepth = maxDepth;
                    trans[hash].scoreFinalized = false;
                    trans[hash].shallowestDepthFound = currDepth;
                }
            }
            else if (trans.Count < transSize)
            {
                trans.Add(hash, new GamestateData(currDepth, maxDepth));
            }

            if (trans.Count < transSize)
            {
                if (game.IsGameOver() || currDepth == maxDepth)
                {
                    int utility = game.Utility(aiPlyr);
                    trans[hash].score = utility;
                    trans[hash].scoreFinalized = true;
                    return new MoveScore(lastMove, utility);
                }

                List<Move> legalMoves = game.GetLegalMoves();
                if (trans[hash].bestMove != null)
                {
                    ReorderToBestFirst(legalMoves, trans[hash].bestMove);
                }
                MoveScore currentBest = new MoveScore(null, NEGINFINITY); //reference value
                foreach (Move move in legalMoves)
                {
                    game.DoMove(move);
                    currentBest = GetBest(currentBest, MinMove(game, currDepth + 1, alpha, beta));
                    game.UndoMove();

                    alpha = System.Math.Max(alpha, currentBest.score);
                    if (alpha >= beta)
                    {
                        alphaBetaCutoffs++;
                        trans[hash].bestMove = currentBest.move;
                        currentBest.move = lastMove;
                        return currentBest;
                    }
                }
                //set the best move for this boardstate BEFORE switching currentBest.move to the 
                //move which got us to the current state, which is required for the calling method
                //but not appropriate for the bestMove (it needs the best move looking from here, not the 
                //move which got us here)
                trans[hash].bestMove = currentBest.move;
                currentBest.move = lastMove;

                trans[hash].score = currentBest.score;
                trans[hash].scoreFinalized = true;

                return currentBest;
            }
            else
            {
                if (game.IsGameOver() || currDepth == maxDepth)
                {
                    return new MoveScore(lastMove, game.Utility(aiPlyr));
                }
                
                MoveScore currentBest = new MoveScore(null, NEGINFINITY); //reference value
                foreach (Move move in game.GetLegalMoves())
                {
                    game.DoMove(move);
                    currentBest = GetBest(currentBest, MinMove(game, currDepth + 1, alpha, beta));
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

                return currentBest;
            }

            
        }




        private MoveScore MinMove(MiniMaxProblem game, int currDepth, int alpha, int beta)
        {
            long hash = game.GetHashKey();
            Move lastMove = game.GetLastMove();

            if (trans.ContainsKey(hash))
            {
                if (trans[hash].highestMaxDepth == maxDepth)
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
                else
                {
                    trans[hash].highestMaxDepth = maxDepth;
                    trans[hash].scoreFinalized = false;
                    trans[hash].shallowestDepthFound = currDepth;
                }
            }
            else if (trans.Count < transSize)
            {
                trans.Add(hash, new GamestateData(currDepth, maxDepth));
            }

            if (trans.Count < transSize)
            {
                if (game.IsGameOver() || currDepth == maxDepth)
                {
                    int utility = game.Utility(aiPlyr);
                    trans[hash].score = utility;
                    trans[hash].scoreFinalized = true;
                    return new MoveScore(lastMove, utility);
                }

                List<Move> legalMoves = game.GetLegalMoves();
                if (trans[hash].bestMove != null)
                {
                    ReorderToBestFirst(legalMoves, trans[hash].bestMove);
                }
                MoveScore currentWorst = new MoveScore(null, POSINFINITY); //reference value
                foreach (Move move in legalMoves)
                {
                    game.DoMove(move);
                    currentWorst = GetWorst(currentWorst, MaxMove(game, currDepth + 1, alpha, beta));
                    game.UndoMove();

                    beta = System.Math.Min(beta, currentWorst.score);
                    if (alpha >= beta)
                    {
                        alphaBetaCutoffs++;
                        trans[hash].bestMove = currentWorst.move;
                        currentWorst.move = lastMove;
                        return currentWorst;
                    }
                }

                trans[hash].bestMove = currentWorst.move;
                currentWorst.move = lastMove;

                trans[hash].score = currentWorst.score;
                trans[hash].scoreFinalized = true;

                return currentWorst;
            }
            else
            {
                if (game.IsGameOver() || currDepth == maxDepth)
                {
                    return new MoveScore(lastMove, game.Utility(aiPlyr));
                }

                MoveScore currentWorst = new MoveScore(null, POSINFINITY); //reference value
                foreach (Move move in game.GetLegalMoves())
                {
                    game.DoMove(move);
                    currentWorst = GetWorst(currentWorst, MaxMove(game, currDepth + 1, alpha, beta));
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

                return currentWorst;
            }

            
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



        private static void ReorderToBestFirst(List<Move> legalMoves, Move bestMove)
        {
            if (legalMoves.Contains(bestMove))
            {
                legalMoves.Remove(bestMove);
                legalMoves.Insert(0, bestMove);
            }
        }
    }
}
