using ConsoleChess.Source.AI;
using Custom.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleChess.Source.Game
{
    public sealed class Chess : MiniMaxProblem
    {
        public const int WHITE = 0;
        public const int BLACK = 1;

        public const int PAWN = 0;
        public const int ROOK = 1;
        public const int KNIGHT = 2;
        public const int BISHOP = 3;
        public const int QUEEN = 4;
        public const int KING = 5;



        Piece[][] board;
        Piece[][] pieces;
        int whosTurn;
        int blackScore;
        int whiteScore;
        List<Tuple<Move, Piece>> moveHistory;

        public long hash;
        private long[][][][] zobristSeeds; //8 rows by 8 cols by 2 players by 6 pieces
        private long switchPlayerZobristSeed;
        



        public long GetHashKey()
        {
            return hash;
        }



        public int GetCurrentPlayer()
        {
            return whosTurn;
        }



        public int Utility(int plyr)
        {
            int w = this.whiteScore;
            int b = this.blackScore;
            for (int i = 0; i < 8; i++)
            {
                if (pieces[WHITE][i].live) w += (6 - pieces[WHITE][i].row) * 5;
                if (pieces[BLACK][i].live) b += (pieces[BLACK][i].row - 1) * 5;
            }
            return plyr == WHITE ? w - b : b - w;
        }




        public Move GetLastMove()
        {
            return moveHistory[moveHistory.Count - 1].Item1;
        }






        public void DoMove(Move move)
        {
            Piece captured = GetPieceHere(move.rowTo, move.colTo);
            if (captured != null) Capture(captured);

            Piece mover = GetPieceHere(move.rowFrom, move.colFrom);
            MovePiece(mover, move);

            moveHistory.Add(new Tuple<Move, Piece>(move, captured));
            switchTurn();
        }

        private void Capture(Piece piece)
        {
            piece.live = false;
            UpdateHash(piece);
            switch (whosTurn)
            {
                case WHITE: whiteScore += piece.val; break;
                case BLACK: blackScore += piece.val; break;
            }
        }

        private void UpdateHash(Piece piece)
        {
            hash ^= zobristSeeds[piece.row][piece.col][piece.owner][piece.type];
        }

        private void MovePiece(Piece piece, Move move)
        {
            UpdateHash(piece);
            piece.MoveTo(move.rowTo, move.colTo);
            UpdateHash(piece);

            board[move.rowTo][move.colTo] = piece;
            board[move.rowFrom][move.colFrom] = null;
        }







        public void UndoMove()
        {
            Move lastMove = GetLastMove();
            Piece mover = GetPieceHere(lastMove.rowTo, lastMove.colTo);
            UnMovePiece(mover, lastMove);
            board[lastMove.rowFrom][lastMove.colFrom] = mover;


            //make the last taken piece live again, update score
            Piece lastDead = moveHistory[moveHistory.Count - 1].Item2;
            if (lastDead != null)
            {
                lastDead.live = true;
                UpdateHash(lastDead);
                switch (mover.owner)
                {
                    case WHITE:
                        whiteScore -= lastDead.val;
                        break;
                    case BLACK:
                        blackScore -= lastDead.val;
                        break;
                }
            }
            board[lastMove.rowTo][lastMove.colTo] = lastDead;

            switchTurn();
            moveHistory.RemoveAt(moveHistory.Count - 1);
        }

        private void UnMovePiece(Piece piece, Move lastMove)
        {
            UpdateHash(piece);
            piece.MoveTo(lastMove.rowFrom, lastMove.colFrom);
            UpdateHash(piece);
        }



        /// <summary>
        /// Returns true if either king is taken, false otherwise
        /// </summary>
        /// <returns></returns>
        public bool IsGameOver()
        {
            return (!pieces[WHITE][12].live || !pieces[BLACK][12].live);
        }








        public List<Move> GetLegalMoves()
        {
            List<Move> legalMoves = new List<Move>();

            for (int i = 15; i > -1; i--)
            {
                Piece piece = pieces[whosTurn][i];
                if (piece.live)
                {
                    int pieceRow = piece.row;
                    int pieceCol = piece.col;

                    switch (piece.type)
                    {
                        //PAWN
                        case PAWN:
                            int dy = piece.owner == WHITE ? -1 : 1;

                            // MOVE 0: move forward by 1
                            //  only if space is clear
                            if (pieceRow + dy < 8 &&
                                pieceRow + dy > -1 &&
                                !IsAnyHere(pieceRow + dy, pieceCol))
                            {
                                legalMoves.Add(new Move(pieceRow, pieceCol, pieceRow + dy, pieceCol));
                            }


                            // MOVE 1: move forward by 2
                            //  only if in starting location && space is clear
                            //black
                            if (piece.owner == BLACK)
                            {
                                if (pieceRow == 1)
                                {
                                    if (!IsAnyHere(pieceRow + dy, pieceCol) &&
                                        !IsAnyHere(pieceRow + (2 * dy), pieceCol))
                                    {
                                        legalMoves.Add(new Move(pieceRow, pieceCol, pieceRow + (2 * dy), pieceCol));
                                    }
                                }
                            }

                            //white
                            else if (piece.owner == WHITE)
                            {
                                if (pieceRow == 6)
                                {
                                    if (!IsAnyHere(pieceRow + dy, pieceCol) &&
                                        !IsAnyHere(pieceRow + (2 * dy), pieceCol))
                                    {
                                        legalMoves.Add(new Move(pieceRow, pieceCol, pieceRow + (2 * dy), pieceCol));
                                    }
                                }
                            }


                            //MOVE 2: move forward by 1 and left by 1
                            //  col can't = 0
                            //  enemy piece has to be in location
                            if (pieceCol != 0)
                            {
                                if (IsEnemyHere(pieceRow + dy, pieceCol - 1, piece.owner))
                                {
                                    legalMoves.Add(new Move(pieceRow, pieceCol, pieceRow + dy, pieceCol - 1));
                                }
                            }


                            //MOVE 3: forward by 1 and right by 1
                            //  col can't = 7
                            //  enemy piece has to be in location
                            if (pieceCol != 7)
                            {
                                if (IsEnemyHere(pieceRow + dy, pieceCol + 1, piece.owner))
                                {
                                    legalMoves.Add(new Move(pieceRow, pieceCol, pieceRow + dy, pieceCol + 1));
                                }
                            }

                            break;



                        //ROOK
                        case ROOK:

                            // MOVE 0: move up
                            //   if not in row 0

                            if (pieceRow != 0)
                            {
                                for (int k = pieceRow - 1; k > -1; k--)
                                {
                                    if (IsFriendlyHere(k, pieceCol, piece.owner)) { break; }
                                    legalMoves.Add(new Move(pieceRow, pieceCol, k, pieceCol));
                                    if (IsEnemyHere(k, pieceCol, piece.owner)) { break; }
                                }
                            }

                            // MOVE 1: move right
                            //  if not in col 7

                            if (pieceCol != 7)
                            {
                                for (int k = pieceCol + 1; k < 8; k++)
                                {
                                    if (IsFriendlyHere(pieceRow, k, piece.owner)) { break; }
                                    legalMoves.Add(new Move(pieceRow, pieceCol, pieceRow, k));
                                    if (IsEnemyHere(pieceRow, k, piece.owner)) { break; }
                                }
                            }

                            // MOVE 2: move down
                            //  if not in row 7

                            if (pieceRow != 7)
                            {
                                for (int k = pieceRow + 1; k < 8; k++)
                                {
                                    if (IsFriendlyHere(k, pieceCol, piece.owner)) { break; }
                                    legalMoves.Add(new Move(pieceRow, pieceCol, k, pieceCol));
                                    if (IsEnemyHere(k, pieceCol, piece.owner)) { break; }
                                }
                            }

                            // MOVE 3: move left
                            //  if not in col 0

                            if (pieceCol != 0)
                            {
                                for (int k = pieceCol - 1; k > -1; k--)
                                {
                                    if (IsFriendlyHere(pieceRow, k, piece.owner)) { break; }
                                    legalMoves.Add(new Move(pieceRow, pieceCol, pieceRow, k));
                                    if (IsEnemyHere(pieceRow, k, piece.owner)) { break; }
                                }
                            }
                            break;

                        //KNIGHT
                        case KNIGHT:

                            // up2 right1
                            if (pieceCol < 7 &&
                                pieceRow > 1 &&
                                !IsFriendlyHere(pieceRow - 2, pieceCol + 1, piece.owner))
                            {
                                legalMoves.Add(new Move(pieceRow, pieceCol, pieceRow - 2, pieceCol + 1));
                            }

                            //up1 right2
                            if (pieceCol < 6 &&
                                pieceRow > 0 &&
                                !IsFriendlyHere(pieceRow - 1, pieceCol + 2, piece.owner))
                            {
                                legalMoves.Add(new Move(pieceRow, pieceCol, pieceRow - 1, pieceCol + 2));
                            }

                            //down1 right2
                            if (pieceCol < 6 &&
                                pieceRow < 7 &&
                                !IsFriendlyHere(pieceRow + 1, pieceCol + 2, piece.owner))
                            {
                                legalMoves.Add(new Move(pieceRow, pieceCol, pieceRow + 1, pieceCol + 2));
                            }

                            //down2 right1
                            if (pieceCol < 7 &&
                                pieceRow < 6 &&
                                !IsFriendlyHere(pieceRow + 2, pieceCol + 1, piece.owner))
                            {
                                legalMoves.Add(new Move(pieceRow, pieceCol, pieceRow + 2, pieceCol + 1));
                            }

                            //down2 left1
                            if (pieceCol > 0 &&
                                pieceRow < 6 &&
                                !IsFriendlyHere(pieceRow + 2, pieceCol - 1, piece.owner))
                            {
                                legalMoves.Add(new Move(pieceRow, pieceCol, pieceRow + 2, pieceCol - 1));
                            }

                            //down1 left2
                            if (pieceCol > 1 &&
                                pieceRow < 7 &&
                                !IsFriendlyHere(pieceRow + 1, pieceCol - 2, piece.owner))
                            {
                                legalMoves.Add(new Move(pieceRow, pieceCol, pieceRow + 1, pieceCol - 2));
                            }

                            //up1 left 2
                            if (pieceCol > 1 &&
                                pieceRow > 0 &&
                                !IsFriendlyHere(pieceRow - 1, pieceCol - 2, piece.owner))
                            {
                                legalMoves.Add(new Move(pieceRow, pieceCol, pieceRow - 1, pieceCol - 2));
                            }

                            //up2 left 1
                            if (pieceCol > 0 &&
                                pieceRow > 1 &&
                                !IsFriendlyHere(pieceRow - 2, pieceCol - 1, piece.owner))
                            {
                                legalMoves.Add(new Move(pieceRow, pieceCol, pieceRow - 2, pieceCol - 1));
                            }
                            break;

                        //BISHOP
                        case BISHOP:
                            // up + right
                            for (int x = 1; x < System.Math.Min(7 - pieceCol, pieceRow) + 1; x++)
                            {
                                if (IsFriendlyHere(pieceRow - x, pieceCol + x, piece.owner)) { break; }
                                legalMoves.Add(new Move(pieceRow, pieceCol, pieceRow - x, pieceCol + x));
                                if (IsEnemyHere(pieceRow - x, pieceCol + x, piece.owner)) { break; }
                            }

                            // down + right
                            for (int x = 1; x < System.Math.Min(7 - pieceCol, 7 - pieceRow) + 1; x++)
                            {
                                if (IsFriendlyHere(pieceRow + x, pieceCol + x, piece.owner)) { break; }
                                legalMoves.Add(new Move(pieceRow, pieceCol, pieceRow + x, pieceCol + x));
                                if (IsEnemyHere(pieceRow + x, pieceCol + x, piece.owner)) { break; }
                            }

                            //down + left
                            for (int x = 1; x < System.Math.Min(7 - pieceRow, pieceCol) + 1; x++)
                            {
                                if (IsFriendlyHere(pieceRow + x, pieceCol - x, piece.owner)) { break; }
                                legalMoves.Add(new Move(pieceRow, pieceCol, pieceRow + x, pieceCol - x));
                                if (IsEnemyHere(pieceRow + x, pieceCol - x, piece.owner)) { break; }
                            }

                            //up and left
                            for (int x = 1; x < System.Math.Min(pieceRow, pieceCol) + 1; x++)
                            {
                                if (IsFriendlyHere(pieceRow - x, pieceCol - x, piece.owner)) { break; }
                                legalMoves.Add(new Move(pieceRow, pieceCol, pieceRow - x, pieceCol - x));
                                if (IsEnemyHere(pieceRow - x, pieceCol - x, piece.owner)) { break; }
                            }
                            break;

                        //QUEEN
                        case QUEEN:

                            // MOVE 0: move up
                            //   if not in row 0
                            if (pieceRow != 0)
                            {
                                for (int k = pieceRow - 1; k > -1; k--)
                                {
                                    if (IsFriendlyHere(k, pieceCol, piece.owner)) { break; }
                                    legalMoves.Add(new Move(pieceRow, pieceCol, k, pieceCol));
                                    if (IsEnemyHere(k, pieceCol, piece.owner)) { break; }
                                }
                            }

                            // MOVE 1: move right
                            //  if not in col 7
                            if (pieceCol != 7)
                            {
                                for (int k = pieceCol + 1; k < 8; k++)
                                {
                                    if (IsFriendlyHere(pieceRow, k, piece.owner)) { break; }
                                    legalMoves.Add(new Move(pieceRow, pieceCol, pieceRow, k));
                                    if (IsEnemyHere(pieceRow, k, piece.owner)) { break; }
                                }
                            }

                            // MOVE 2: move down
                            //  if not in row 7
                            if (pieceRow != 7)
                            {
                                for (int k = pieceRow + 1; k < 8; k++)
                                {
                                    if (IsFriendlyHere(k, pieceCol, piece.owner)) { break; }
                                    legalMoves.Add(new Move(pieceRow, pieceCol, k, pieceCol));
                                    if (IsEnemyHere(k, pieceCol, piece.owner)) { break; }
                                }
                            }

                            // MOVE 3: move left
                            //  if not in col 0
                            if (pieceCol != 0)
                            {
                                for (int k = pieceCol - 1; k > -1; k--)
                                {
                                    if (IsFriendlyHere(pieceRow, k, piece.owner)) { break; }
                                    legalMoves.Add(new Move(pieceRow, pieceCol, pieceRow, k));
                                    if (IsEnemyHere(pieceRow, k, piece.owner)) { break; }
                                }
                            }


                            // up + right
                            for (int x = 1; x < System.Math.Min(7 - pieceCol, pieceRow) + 1; x++)
                            {
                                if (IsFriendlyHere(pieceRow - x, pieceCol + x, piece.owner)) { break; }
                                legalMoves.Add(new Move(pieceRow, pieceCol, pieceRow - x, pieceCol + x));
                                if (IsEnemyHere(pieceRow - x, pieceCol + x, piece.owner)) { break; }
                            }

                            // down + right
                            for (int x = 1; x < System.Math.Min(7 - pieceCol, 7 - pieceRow) + 1; x++)
                            {
                                if (IsFriendlyHere(pieceRow + x, pieceCol + x, piece.owner)) { break; }
                                legalMoves.Add(new Move(pieceRow, pieceCol, pieceRow + x, pieceCol + x));
                                if (IsEnemyHere(pieceRow + x, pieceCol + x, piece.owner)) { break; }
                            }

                            //down + left
                            for (int x = 1; x < System.Math.Min(7 - pieceRow, pieceCol) + 1; x++)
                            {
                                if (IsFriendlyHere(pieceRow + x, pieceCol - x, piece.owner)) { break; }
                                legalMoves.Add(new Move(pieceRow, pieceCol, pieceRow + x, pieceCol - x));
                                if (IsEnemyHere(pieceRow + x, pieceCol - x, piece.owner)) { break; }
                            }

                            //up and left
                            for (int x = 1; x < System.Math.Min(pieceRow, pieceCol) + 1; x++)
                            {
                                if (IsFriendlyHere(pieceRow - x, pieceCol - x, piece.owner)) { break; }
                                legalMoves.Add(new Move(pieceRow, pieceCol, pieceRow - x, pieceCol - x));
                                if (IsEnemyHere(pieceRow - x, pieceCol - x, piece.owner)) { break; }
                            }
                            break;

                        //KING
                        case KING:

                            //move up
                            if (pieceRow > 0)
                            {
                                if (!IsFriendlyHere(pieceRow - 1, pieceCol, piece.owner))
                                {
                                    legalMoves.Add(new Move(pieceRow, pieceCol, pieceRow - 1, pieceCol));
                                }
                            }

                            //move down
                            if (pieceRow < 7)
                            {
                                if (!IsFriendlyHere(pieceRow + 1, pieceCol, piece.owner))
                                {
                                    legalMoves.Add(new Move(pieceRow, pieceCol, pieceRow + 1, pieceCol));
                                }
                            }

                            //move right
                            if (pieceCol < 7)
                            {
                                if (!IsFriendlyHere(pieceRow, pieceCol + 1, piece.owner))
                                {
                                    legalMoves.Add(new Move(pieceRow, pieceCol, pieceRow, pieceCol + 1));
                                }
                            }

                            //move left
                            if (pieceCol > 0)
                            {
                                if (!IsFriendlyHere(pieceRow, pieceCol - 1, piece.owner))
                                {
                                    legalMoves.Add(new Move(pieceRow, pieceCol, pieceRow, pieceCol - 1));
                                }
                            }

                            //move up and right
                            if (pieceRow > 0 && pieceCol < 7 && !IsFriendlyHere(pieceRow - 1, pieceCol + 1, piece.owner))
                            {
                                legalMoves.Add(new Move(pieceRow, pieceCol, pieceRow - 1, pieceCol + 1));
                            }

                            //move down and right
                            if (pieceRow < 7 && pieceCol < 7 && !IsFriendlyHere(pieceRow + 1, pieceCol + 1, piece.owner))
                            {
                                legalMoves.Add(new Move(pieceRow, pieceCol, pieceRow + 1, pieceCol + 1));
                            }
                            //move down and left
                            if (pieceRow < 7 && pieceCol > 0 && !IsFriendlyHere(pieceRow + 1, pieceCol - 1, piece.owner))
                            {
                                legalMoves.Add(new Move(pieceRow, pieceCol, pieceRow + 1, pieceCol - 1));
                            }
                            //move up and left
                            if (pieceRow > 0 && pieceCol > 0 && !IsFriendlyHere(pieceRow - 1, pieceCol - 1, piece.owner))
                            {
                                legalMoves.Add(new Move(pieceRow, pieceCol, pieceRow - 1, pieceCol - 1));
                            }
                            break;
                    }
                }
            }
            return legalMoves;
        }

        





        //===============================================================================//
        //                       PRIVATE HELPER METHODS                                  //
        //===============================================================================//


        public void print()
        {
            Console.WriteLine("Board:  ");
            Console.WriteLine("  1    2    3    4    5    6    7    8");
            Console.WriteLine("-----------------------------------------");
            for (int i = 0; i < 8; i++)
            {
                Console.Write("| ");
                for (int j = 0; j < 8; j++)
                {
                    Console.Write(toString(GetPieceHere(i, j)));
                    Console.Write(" | ");
                }
                Console.Write(i + 1);
                Console.WriteLine();
                Console.WriteLine("-----------------------------------------");
            }
        }

        private string toString(Piece p)
        {
            if (p == null) { return "  "; }
            int type = p.type;
            string owner;
            if (p.owner == WHITE) { owner = "w"; } else owner = "b";
            string output;
            switch (type)
            {
                case PAWN:
                    output = "p";
                    break;
                case ROOK:
                    output = "r";
                    break;
                case KNIGHT:
                    output = "n";
                    break;
                case BISHOP:
                    output = "b";
                    break;
                case QUEEN:
                    output = "q";
                    break;
                case KING:
                    output = "k";
                    break;
                default:
                    output = "  ";
                    break;
            }
            output = owner + output;
            return output;
        }




        private Piece GetPieceHere(int row, int col)
        {
            return board[row][col];
        }




        private bool IsFriendlyHere(int row, int col, int plyr)
        {
            Piece piece = GetPieceHere(row, col);
            return piece != null &&
                   piece.owner == plyr;
        }



        private bool IsAnyHere(int row, int col)
        {
            return board[row][col] != null;
        }


        private bool IsEnemyHere(int row, int col, int plyr)
        {
            Piece piece = GetPieceHere(row, col);
            return piece != null &&
                   piece.owner != plyr;
        }


        private void switchTurn()
        {
            if (whosTurn == WHITE) whosTurn = BLACK;
            else whosTurn = WHITE;
        }





        //=========================================================//
        //                 DEFAULT CONSTRUCTOR                     //
        //=========================================================//
        public Chess()
        {
            InitializeBoardAndPieces();
            InitializeZobristValues();
            InitializeHashkey();
            moveHistory = new List<Tuple<Move, Piece>>();
            whosTurn = WHITE;
            whiteScore = 0;
            blackScore = 0;
        }
        private void InitializeHashkey()
        {
            hash = 0;
            hash ^= zobristSeeds[6][0][WHITE][PAWN];
            hash ^= zobristSeeds[6][1][WHITE][PAWN];
            hash ^= zobristSeeds[6][2][WHITE][PAWN];
            hash ^= zobristSeeds[6][3][WHITE][PAWN];
            hash ^= zobristSeeds[6][4][WHITE][PAWN];
            hash ^= zobristSeeds[6][5][WHITE][PAWN];
            hash ^= zobristSeeds[6][6][WHITE][PAWN];
            hash ^= zobristSeeds[6][7][WHITE][PAWN];

            hash ^= zobristSeeds[7][0][WHITE][ROOK];
            hash ^= zobristSeeds[7][1][WHITE][KNIGHT];
            hash ^= zobristSeeds[7][2][WHITE][BISHOP];
            hash ^= zobristSeeds[7][3][WHITE][QUEEN];
            hash ^= zobristSeeds[7][4][WHITE][KING];
            hash ^= zobristSeeds[7][5][WHITE][BISHOP];
            hash ^= zobristSeeds[7][6][WHITE][KNIGHT];
            hash ^= zobristSeeds[7][7][WHITE][ROOK];

            hash ^= zobristSeeds[1][0][BLACK][PAWN];
            hash ^= zobristSeeds[1][1][BLACK][PAWN];
            hash ^= zobristSeeds[1][2][BLACK][PAWN];
            hash ^= zobristSeeds[1][3][BLACK][PAWN];
            hash ^= zobristSeeds[1][4][BLACK][PAWN];
            hash ^= zobristSeeds[1][5][BLACK][PAWN];
            hash ^= zobristSeeds[1][6][BLACK][PAWN];
            hash ^= zobristSeeds[1][7][BLACK][PAWN];

            hash ^= zobristSeeds[0][0][BLACK][ROOK];
            hash ^= zobristSeeds[0][1][BLACK][KNIGHT];
            hash ^= zobristSeeds[0][2][BLACK][BISHOP];
            hash ^= zobristSeeds[0][3][BLACK][QUEEN];
            hash ^= zobristSeeds[0][4][BLACK][KING];
            hash ^= zobristSeeds[0][5][BLACK][BISHOP];
            hash ^= zobristSeeds[0][6][BLACK][KNIGHT];
            hash ^= zobristSeeds[0][7][BLACK][ROOK];
        }

        private void InitializeZobristValues()
        {
            InitializeZobristArray();
            System.Random randy = new System.Random();
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        for (int l = 0; l < 6; l++)
                        {
                            zobristSeeds[i][j][k][l] = randy.NextInt64();
                        }
                    }
                }
            }
            switchPlayerZobristSeed = randy.NextInt64();
        }

        private void InitializeZobristArray()
        {
            zobristSeeds = JaggedArray.Initialize4D<long>(8, 8, 2, 6);
        }


        private void InitializeBoardAndPieces()
        {
            Piece whitePawn1 = new Piece(WHITE, PAWN, 6, 0, true, 93);
            Piece whitePawn2 = new Piece(WHITE, PAWN, 6, 1, true, 93);
            Piece whitePawn3 = new Piece(WHITE, PAWN, 6, 2, true, 93);
            Piece whitePawn4 = new Piece(WHITE, PAWN, 6, 3, true, 93);
            Piece whitePawn5 = new Piece(WHITE, PAWN, 6, 4, true, 93);
            Piece whitePawn6 = new Piece(WHITE, PAWN, 6, 5, true, 93);
            Piece whitePawn7 = new Piece(WHITE, PAWN, 6, 6, true, 93);
            Piece whitePawn8 = new Piece(WHITE, PAWN, 6, 7, true, 93);
            Piece whiteLeftRook = new Piece(WHITE, ROOK, 7, 0, true, 491);
            Piece whiteLeftKnight = new Piece(WHITE, KNIGHT, 7, 1, true, 314);
            Piece whiteLeftBishop = new Piece(WHITE, BISHOP, 7, 2, true, 321);
            Piece whiteQueen = new Piece(WHITE, QUEEN, 7, 3, true, 947);
            Piece whiteKing = new Piece(WHITE, KING, 7, 4, true, 9999);
            Piece whiteRightBishop = new Piece(WHITE, BISHOP, 7, 5, true, 321);
            Piece whiteRightKnight = new Piece(WHITE, KNIGHT, 7, 6, true, 314);
            Piece whiteRightRook = new Piece(WHITE, ROOK, 7, 7, true, 491);

            Piece blackPawn1 = new Piece(BLACK, PAWN, 1, 0, true, 93);
            Piece blackPawn2 = new Piece(BLACK, PAWN, 1, 1, true, 93);
            Piece blackPawn3 = new Piece(BLACK, PAWN, 1, 2, true, 93);
            Piece blackPawn4 = new Piece(BLACK, PAWN, 1, 3, true, 93);
            Piece blackPawn5 = new Piece(BLACK, PAWN, 1, 4, true, 93);
            Piece blackPawn6 = new Piece(BLACK, PAWN, 1, 5, true, 93);
            Piece blackPawn7 = new Piece(BLACK, PAWN, 1, 6, true, 93);
            Piece blackPawn8 = new Piece(BLACK, PAWN, 1, 7, true, 93);
            Piece blackLeftRook = new Piece(BLACK, ROOK, 0, 0, true, 491);
            Piece blackLeftKnight = new Piece(BLACK, KNIGHT, 0, 1, true, 314);
            Piece blackLeftBishop = new Piece(BLACK, BISHOP, 0, 2, true, 321);
            Piece blackQueen = new Piece(BLACK, QUEEN, 0, 3, true, 947);
            Piece blackKing = new Piece(BLACK, KING, 0, 4, true, 9999);
            Piece blackRightBishop = new Piece(BLACK, BISHOP, 0, 5, true, 321);
            Piece blackRightKnight = new Piece(BLACK, KNIGHT, 0, 6, true, 314);
            Piece blackRightRook = new Piece(BLACK, ROOK, 0, 7, true, 491);

            board = JaggedArray.Initialize2D<Piece>(8, 8);
            board[6][0] = whitePawn1;
            board[6][1] = whitePawn2;
            board[6][2] = whitePawn3;
            board[6][3] = whitePawn4;
            board[6][4] = whitePawn5;
            board[6][5] = whitePawn6;
            board[6][6] = whitePawn7;
            board[6][7] = whitePawn8;

            board[7][0] = whiteLeftRook;
            board[7][1] = whiteLeftKnight;
            board[7][2] = whiteLeftBishop;
            board[7][3] = whiteQueen;
            board[7][4] = whiteKing;
            board[7][5] = whiteRightBishop;
            board[7][6] = whiteRightKnight;
            board[7][7] = whiteRightRook;

            board[1][0] = blackPawn1;
            board[1][1] = blackPawn2;
            board[1][2] = blackPawn3;
            board[1][3] = blackPawn4;
            board[1][4] = blackPawn5;
            board[1][5] = blackPawn6;
            board[1][6] = blackPawn7;
            board[1][7] = blackPawn8;

            board[0][0] = blackLeftRook;
            board[0][1] = blackLeftKnight;
            board[0][2] = blackLeftBishop;
            board[0][3] = blackQueen;
            board[0][4] = blackKing;
            board[0][5] = blackRightBishop;
            board[0][6] = blackRightKnight;
            board[0][7] = blackRightRook;

            pieces = new Piece[2][]
            {
                new Piece[16]
                {
                    whitePawn1,
                    whitePawn2,
                    whitePawn3,
                    whitePawn4,
                    whitePawn5,
                    whitePawn6,
                    whitePawn7,
                    whitePawn8,

                    whiteLeftRook,
                    whiteLeftKnight,
                    whiteLeftBishop,
                    whiteQueen,
                    whiteKing,
                    whiteRightBishop,
                    whiteRightKnight,
                    whiteRightRook
                },
                new Piece[16]
                {
                    blackPawn1,
                    blackPawn2,
                    blackPawn3,
                    blackPawn4,
                    blackPawn5,
                    blackPawn6,
                    blackPawn7,
                    blackPawn8,

                    blackLeftRook,
                    blackLeftKnight,
                    blackLeftBishop,
                    blackQueen,
                    blackKing,
                    blackRightBishop,
                    blackRightKnight,
                    blackRightRook
                }
            };
        }
    }
}
