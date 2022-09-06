using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace GridWorldGame
{
    public enum Direction
    {
        Up = 0,
        Down = 1,
        Left = 2,
        Right = 3,
    }

    public struct Tile : IEnumerable<Tile> // no need to implement this for Tile
    {
        private readonly List<Tile> _list = new();

        public Tile(float[] weights, float[] elegibility, int reward, Direction direction)
        {
            Weights = weights;
            Direction = direction;
            Elegibility = elegibility;
            Reward = reward;
        }

        public Tile()
        {
            Random random = new();

            Weights = Enumerable
                .Repeat(1, 4)
                .Select(i => random.NextSingle())
                .ToArray();
            Elegibility = new float[4];
            Direction = Helpers.GetRandomEnum<Direction>();
            Reward = 0;
        }

        public Direction Direction { get; set; }
        public float[] Weights { get; set; }
        public float[] Elegibility { get; set; }
        public int Reward { get; set; }

        public IEnumerator<Tile> GetEnumerator()
        {
            foreach (var lst in _list)
            {
                yield return lst;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    public struct Location
    {
        public int X { get; }
        public int Y { get; }

        public Location(int x, int y) => (X, Y) = (x, y);

        public IEnumerable<Location> GetSurroundings()
        {
            return new List<Location>()
            {
                new Location(X-1, Y),
                new Location(X+1, Y),
                new Location(X, Y-1),
                new Location(X, Y+1),
            };
        }

        public override string ToString()
        {
            return $"({X},{Y})";
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return obj is Location other && this.Equals(other);
        }

        public bool Equals(Location other)
        {
            return other.X == this.X && other.Y == this.Y;
        }

        public static bool operator==(Location lhs, Location rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator!=(Location lhs, Location rhs)
        {
            return !(lhs == rhs);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
    }
}

