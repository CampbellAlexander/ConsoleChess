using System;

namespace ConsoleChess.Source.Game
{
    public sealed class Move : IEquatable<Move>
    {
        public readonly int rowFrom;
        public readonly int colFrom;
        public readonly int rowTo;
        public readonly int colTo;


        public Move(int rowFrom, int colFrom, int rowTo, int colTo)
        {
            this.rowFrom = rowFrom;
            this.colFrom = colFrom;
            this.rowTo = rowTo;
            this.colTo = colTo;
        }

        
        public sealed override bool Equals(Object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            Move other = (Move)obj;
            return this.Equals(other);
        }
        public bool Equals(Move other)
        {
            return this.rowFrom == other.rowFrom &&
                   this.colFrom == other.colFrom &&
                   this.rowTo == other.rowTo &&
                   this.colTo == other.colTo;
        }
        public sealed override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + rowFrom;
            hash = hash * 31 + colFrom;
            hash = hash * 31 + rowTo;
            hash = hash * 31 + colTo;
            return hash;
        }
    }
}