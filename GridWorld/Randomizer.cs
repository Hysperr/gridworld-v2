namespace GridWorld
{
    public class Randomizer
    {
        private static readonly Random _random = new();

        private int _obstaclePercent;
        public int ObstaclePercent
        {
            get => _obstaclePercent;
            set
            {
                if (InRange( _obstaclePercent ))
                {
                    _obstaclePercent = value;
                }
            }
        }

        public bool NextObstacle()
        {
            return _random.Next(0, 100) < ObstaclePercent;
        }

        private static bool InRange( int obstaclePercent )
        {
            return obstaclePercent is >= 0 and <= 100
                ? true
                : throw new ArgumentException($"Obstacle % is not between 0-100", nameof(obstaclePercent));
        }
    }
}
