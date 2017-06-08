using System;
using ConsoleChess.Source.AI;
using ConsoleChess.Source.Game;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleChess.Source.Performance
{
    public static class Test
    {
        /// <summary>
        /// RESULT: 9.4% time reduction when using Byte implementation
        /// </summary>
        /// <param name="AIwithInt"></param>
        /// <param name="AIwithByte"></param>
        public static void Speed(MiniMaxAlphaBetaTransTableBestMoveFirst AIwithInt,
                                 MiniMaxAlphaBetaTransTableBestMoveFirstByte AIwithByte)
        {
            Chess game = new Chess();
            Stopwatch s = new Stopwatch();
            long totalTicks = 0;
            //warmup:
            for (int i = 0; i < 100; i++)
            {
                AIwithByte.GetBestMove(game, 6);
            }
            //ok, begin:
            for (int i = 0; i < 1000; i++)
            {
                s.Start();
                AIwithByte.GetBestMove(game, 6);
                s.Stop();
                totalTicks += s.ElapsedTicks;
                s.Reset();
            }
            long avgTicks = totalTicks / 1000;
            Console.WriteLine("Avg ticks byteAI: " + avgTicks);

            totalTicks = 0;
            //warmup:
            for (int i = 0; i < 100; i++)
            {
                AIwithInt.GetBestMove(game, 6);
            }
            for (int i = 0; i < 1000; i++)
            {
                s.Start();
                AIwithInt.GetBestMove(game, 6);
                s.Stop();
                totalTicks += s.ElapsedTicks;
                s.Reset();
            }
            avgTicks = totalTicks / 1000;
            Console.WriteLine("Avg ticks intAI: " + avgTicks);
            Console.WriteLine();
            Console.Write("press enter to exit "); Console.ReadLine();
        }

        /// <summary>
        /// 8.98% time reduction using for-loop instead of foreach
        /// </summary>
        /// <param name="byteAI"></param>
        /// <param name="byteAIwithLoop"></param>
        public static void Speed(MiniMaxAlphaBetaTransTableBestMoveFirstByte byteAI,
                                 MiniMaxAlphaBetaTransTableBestMoveFirstByteForLoop byteAIwithLoop)
        {
            Chess game = new Chess();
            Stopwatch s = new Stopwatch();
            long totalTicks = 0;
            for (int i = 0; i < 100; i++)
            {
                s.Start();
                byteAIwithLoop.GetBestMove(game, 6);
                s.Stop();
                totalTicks += s.ElapsedTicks;
                s.Reset();
            }
            long avgTicks = totalTicks / 100;
            Console.WriteLine("Avg ticks byteAI with for loop: " + avgTicks);

            totalTicks = 0;
            for (int i = 0; i < 100; i++)
            {
                s.Start();
                byteAI.GetBestMove(game, 6);
                s.Stop();
                totalTicks += s.ElapsedTicks;
                s.Reset();
            }
            avgTicks = totalTicks / 100;
            Console.WriteLine("Avg ticks byteAI with foreach loop: " + avgTicks);
            Console.WriteLine();
            Console.Write("press enter to exit "); Console.ReadLine();
        }

        // avg: 3660864
        //      3624192
        //      3697847
        //      3676144
        //      3670415
        //      3647177
        //      3599488
        //      3673152
        //      3621731
        //10    3672413
        //      3598945
        //      3589117
        //      3627234
        //      3635836
        //15    3612390
        //==========================
        //MEAN: 3640463   (15 samples)
        public static void Speed()
        {
            var c = new Chess();
            var ai = new MiniMaxAlphaBetaTransTableBestMoveFirst();
            var s = new Stopwatch();
            long totalTicks = 0;
            for (int i = 0; i < 100; i++)
            {
                s.Start();
                ai.GetBestMove(c, 6);
                s.Stop();
                totalTicks += s.ElapsedTicks;
                s.Reset();
            }
            Console.WriteLine("Avg, 100 runs, 6 depth: " + (totalTicks / 100));
            Console.ReadLine();
        }

        // avg: 3576827
        //      3584684
        //      3671535
        //      3636403
        //      3578173
        //      3641127
        //      3661919
        //      3658287
        //      3590852
        //10    3594844
        //      3639650
        //      3534476
        //      3601245
        //      3643566
        //      3605105
        //16    3636239
        //17    3622144
        //==========================
        //MEAN: 3615930   (16 samples)
        public static void SpeedStatic()
        {
            var c = new Chess();
            var s = new Stopwatch();
            long totalTicks = 0;
            for (int i = 0; i < 100; i++)
            {
                s.Start();
                MiniMaxAlphaBetaTransTableBestMoveFirstStatic.GetBestMove(c, 6);
                s.Stop();
                totalTicks += s.ElapsedTicks;
                s.Reset();
            }
            Console.WriteLine("Avg, 100 runs, 6 depth: " + (totalTicks / 100));
            Console.ReadLine();
        }

        public static void Memory()
        {
            Dictionary<long, GamestateData> table = new Dictionary<long, GamestateData>(15000000);
            long i = 0;
            try
            {
                while (true)
                {
                    table[i] = new GamestateData(1, 2, 3, true, new Move(1, 2, 3, 4));
                    i++;
                    //Console.WriteLine(i);
                }
            }
            catch (OutOfMemoryException e)
            {
                Console.WriteLine(table.Count);
                Console.ReadLine();
            }
        }
        private class GamestateData
        {
            public int shallowestDepthFound;
            public int highestMaxDepth;
            public int score;
            public bool scoreFinalized;
            public Move bestMove;

            public GamestateData(int a, int b, int c, bool d, Move e)
            {
                a = shallowestDepthFound;
                b = highestMaxDepth;
                c = score;
                d = scoreFinalized;
                e = bestMove;
            }
        }
    }
}
