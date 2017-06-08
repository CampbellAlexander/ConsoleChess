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
    /// TESTER BYTE IMPLEMENTATION
    /// An algorithm to solve min-max zero-sum game problems. Using optimizations:
    /// alpha-beta cutoffs, transposition table, IDS, and always trying the best move first
    /// from previous iterations of IDS
    /// </summary>
    public sealed class MiniMaxAlphaBetaTransTableBestMoveFirstByte
    {
        private byte maxDepth;
        private int aiPlyr;

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
            public byte shallowestDepthFound;
            public byte highestMaxDepth;
            public int score;
            public bool scoreFinalized;
            public Move bestMove;

            public GamestateData(byte shallowestDepthFound, byte highestMaxDepth)
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
        public Move GetBestMove(MiniMaxProblem game, byte maxDepth)
        {
            this.aiPlyr = game.GetCurrentPlayer();
            long hash = game.GetHashKey();

            trans.Clear();
            transCutoffs = 0;
            alphaBetaCutoffs = 0;
            trans.Add(hash, new GamestateData(0, 1));


            MoveScore currentBest = new MoveScore(null, NEGINFINITY); //reference value

            for (byte i = 1; i <= maxDepth; i++)
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



            //Console.WriteLine("The value of the move the AI picked is..........." + currentBest.score);
            //Console.WriteLine("the number of transpose table cuttoffs is........" + transCutoffs);
            //Console.WriteLine("The number of entries in the transpose table is.." + trans.Count);
            //Console.WriteLine("The number of alpha-beta cutoffs is.............." + alphaBetaCutoffs);

            return currentBest.move;
        }




        private MoveScore MaxMove(MiniMaxProblem game, byte currDepth, int alpha, int beta)
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
            else trans.Add(hash, new GamestateData(currDepth, maxDepth));


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
                currentBest = GetBest(currentBest, MinMove(game, plusOne(currDepth), alpha, beta));
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




        private MoveScore MinMove(MiniMaxProblem game, byte currDepth, int alpha, int beta)
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
            else trans.Add(hash, new GamestateData(currDepth, maxDepth));





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
                currentWorst = GetWorst(currentWorst, MaxMove(game, plusOne(currDepth), alpha, beta));
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


        private static byte plusOne(byte b)
        {
            switch (b)
            {
                case 0: return 1;
                case 1: return 2;
                case 2: return 3;
                case 3: return 4;
                case 4: return 5;
                case 5: return 6;
                case 6: return 7;
                case 7: return 8;
                case 8: return 9;
                case 9: return 10;
                case 10: return 11;
                case 11: return 12;
                case 12: return 13;
                case 13: return 14;
            }
            return 0;
        }

    }
}
