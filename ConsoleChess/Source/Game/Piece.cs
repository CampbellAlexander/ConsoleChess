using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleChess.Source.Game
{
    public class Piece
    {
        public int owner;
        public int type;
        public int row;
        public int col;
        public bool live;
        public int val;

        public Piece(int owner, int type, int row, int col, bool live, int val)
        {
            this.owner = owner;
            this.type = type;
            this.row = row;
            this.col = col;
            this.live = live;
            this.val = val;
        }

        public void MoveTo(int row, int col)
        {
            this.row = row;
            this.col = col;
        }
    }
}
