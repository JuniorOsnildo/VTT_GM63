using System;

namespace Core
{
    public readonly struct GridCoordinate : IEquatable<GridCoordinate>
    {
        public readonly int X;
        public readonly int Y;

        public GridCoordinate(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public bool Equals(GridCoordinate other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is GridCoordinate other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
        
        public static bool operator ==(GridCoordinate a, GridCoordinate b) => a.Equals(b);
        
        public static bool operator !=(GridCoordinate a, GridCoordinate b) => !a.Equals(b);
        
        public static GridCoordinate operator +(GridCoordinate a, GridCoordinate b)
        {
            return new GridCoordinate(a.X + b.X, a.Y + b.Y);
        }
        
        public int ManhattanDistanceTo(GridCoordinate other)
        {
            return Math.Abs(X - other.X) + Math.Abs(Y - other.Y);
        }
        
        public override string ToString() => $"({X}, {Y})";
    }
}