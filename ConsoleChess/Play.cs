using ConsoleChess.Source.AI;
using ConsoleChess.Source.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleChess
{
    public static class Play
    {
        public static void PlayManual()
        {
            Chess game = new Chess();
            while (true)
            {
                if (game.GetCurrentPlayer() == Chess.WHITE)
                {
                    Console.WriteLine("hash key: " + game.GetHashKey());
                    game.print();
                    Console.WriteLine("White's turn");
                    string selection;
                    Console.Write("0 - undo move, or anything else to proceed"); selection = Console.ReadLine();
                    if (selection == "0")
                    {
                        game.UndoMove();
                    }
                    else
                    {
                        int toRow;
                        int toCol;
                        int fromRow;
                        int fromCol;
                        Console.Write("Pick from row: "); fromRow = Convert.ToInt32(Console.ReadLine()) - 1;
                        Console.Write("Pick from col: "); fromCol = Convert.ToInt32(Console.ReadLine()) - 1;
                        Console.Write("Pick to row: "); toRow = Convert.ToInt32(Console.ReadLine()) - 1;
                        Console.Write("Pick to col: "); toCol = Convert.ToInt32(Console.ReadLine()) - 1;
                        game.DoMove(new Move(fromRow, fromCol, toRow, toCol));
                    }
                    Console.WriteLine(); Console.WriteLine();
                }
                else
                {
                    Console.WriteLine("hash key: " + game.GetHashKey());
                    game.print();
                    Console.WriteLine("Black's turn");
                    string selection;
                    Console.Write("0 - undo move, or anything else to proceed"); selection = Console.ReadLine();
                    if (selection == "0")
                    {
                        game.UndoMove();
                    }
                    else
                    {
                        int toRow;
                        int toCol;
                        int fromRow;
                        int fromCol;
                        Console.Write("Pick from row: "); fromRow = Convert.ToInt32(Console.ReadLine()) - 1;
                        Console.Write("Pick from col: "); fromCol = Convert.ToInt32(Console.ReadLine()) - 1;
                        Console.Write("Pick to row: "); toRow = Convert.ToInt32(Console.ReadLine()) - 1;
                        Console.Write("Pick to col: "); toCol = Convert.ToInt32(Console.ReadLine()) - 1;
                        game.DoMove(new Move(fromRow, fromCol, toRow, toCol));
                    }
                    Console.WriteLine(); Console.WriteLine();
                }
            }
        }

        public static void PlayAgainstAI()
        {
            Chess c = new Chess();

            //var ai = new MiniMaxAlphaBetaTransTable();
            //var ai = new MiniMaxAlphaBetaTransTableBestMoveFirst();
            //var ai = new MiniMaxAlphaBetaTransTableOptimized();
            //var ai = new MiniMaxAlphaBetaTransTableBestMoveFirstArray();
            //var ai = new MiniMaxAlphaBetaTransTableBestMoveFirstByteForLoop();
            var ai = new MiniMaxAlphaBetaTransTableBestMoveFirstMemoryControlled();

            bool notOver = true;
            while (notOver)
            {
                switch (c.GetCurrentPlayer())
                {
                    case Chess.WHITE:
                        c.print();
                        Console.WriteLine("your turn");

                        int toRow;
                        int toCol;
                        int fromRow;
                        int fromCol;
                        Console.Write("Pick from row: "); fromRow = Convert.ToInt32(Console.ReadLine()) - 1;
                        Console.Write("Pick from col: "); fromCol = Convert.ToInt32(Console.ReadLine()) - 1;
                        Console.Write("Pick to row: "); toRow = Convert.ToInt32(Console.ReadLine()) - 1;
                        Console.Write("Pick to col: "); toCol = Convert.ToInt32(Console.ReadLine()) - 1;

                        c.DoMove(new Move(fromRow, fromCol, toRow, toCol));
                        if (c.IsGameOver()) { notOver = false; }
                        Console.WriteLine();
                        break;

                    case Chess.BLACK:
                        c.print();
                        Console.WriteLine("Black AI's turn");

                        c.DoMove(ai.GetBestMove(c, 7));
                        //c.DoMove(MiniMaxAlphaBetaTransTableBestMoveFirstStatic.GetBestMove(c, 8));
                        if (c.IsGameOver()) { notOver = false; }
                        Console.WriteLine();
                        break;
                }
            }
            Console.ReadKey();
        }

        public static void PlayAIvsAI()
        {
            Chess game = new Chess();
            var blackAI = new MiniMaxAlphaBetaTransTable();
            var whiteAI = new MiniMaxAlphaBetaTransTableBestMoveFirst();
            bool notOver = true;

            while (notOver)
            {
                switch (game.GetCurrentPlayer())
                {
                    case Chess.WHITE:
                        game.print();
                        Console.WriteLine("White AI's turn");
                        game.DoMove(whiteAI.GetBestMove(game, 6));
                        Console.WriteLine();
                        if (game.IsGameOver()) notOver = false;
                        break;
                    case Chess.BLACK:
                        game.print();
                        Console.WriteLine("Black AI's turn");
                        game.DoMove(blackAI.GetBestMove(game, 6));
                        Console.WriteLine();
                        if (game.IsGameOver()) notOver = false;
                        break;
                }
            }
            Console.ReadKey();
        }
    }
}
