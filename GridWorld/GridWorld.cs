using GridWorld;
using System.Diagnostics;
using System.Text;

namespace GridWorldGame
{
    public class GridWorld
    {
        public Tile[,] Board { get; }

        private static readonly Dictionary<object, char> _characters = new()
        {
            {'O', '#' },
            {"Player", 'X' },
            {"Goal", 'O' },
            {Direction.Up, 'U' },
            {Direction.Down, 'D' },
            {Direction.Left, 'L' },
            {Direction.Right, 'R' }
        };

        // end learning once _gammaDF is below this number
        private const float TERMINATE_VALUE = 0.05f;

        // learning rate
        private const float ALPHA = 0.005f;

        // faster,less accuracy on larger grids
        // consider making a function of the gridsize
        private const float LAMBDA = 0.0000005f;

        // print stats ever this many actions
        private const int CHECKPOINT = 100000;

        // if > .90 explore more early on, useful on larger grids
        private float _gammaDF = 0.90f;

        private readonly string _guid;

        public bool IsDoneRunning
        {
            get => _gammaDF < TERMINATE_VALUE;
        }

        public bool ObstaclesEnabled { get; }

        public IEnumerable<Location> Obstacles { get; private set; }

        public Location GoalSpot { get; set; }

        public Location PlayerSpot { get; set; }

        public int Rows => Board.GetLength( 0 );

        public int Columns => Board.GetLength( 1 );

        public Randomizer Randomizer { get; } = new Randomizer();

        public GridWorld( int rowSize, int colSize, bool enableObstacles )
        {
            if (rowSize < 1 || colSize < 1)
            {
                throw new ArgumentException( "Cannot have empty rows or columns" );
            }

            _guid = Guid.NewGuid().ToString( "D" )[..5];

            Board = new Tile[rowSize, colSize];
            ObstaclesEnabled = enableObstacles;
            Obstacles = enableObstacles ? GetObstacles() : Enumerable.Empty<Location>();

            InitBoard();

            ResetPlayerPosition();
            ResetGoalPosition();
        }

        private void InitBoard()
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    Board[i, j] = new Tile();
                }
            }
        }

        private bool IsInBounds( Location location )
        {
            return location.X >= 0 && location.X < Rows
                && location.Y >= 0 && location.Y < Columns;
        }

        private bool IsInBounds( Location location, Direction direction )
        {
            Location newLoc = GetNewLocation( location, direction );

            return IsInBounds( newLoc );
        }

        private static Location GetNewLocation( Location location, Direction direction )
        {
            Location newLoc = direction switch
            {
                Direction.Up => new Location( location.X - 1, location.Y ),
                Direction.Down => new Location( location.X + 1, location.Y ),
                Direction.Left => new Location( location.X, location.Y - 1 ),
                Direction.Right => new Location( location.X, location.Y + 1 ),
                _ => throw new ArgumentException( $"Unsupported direction: {direction}" )
            };

            return newLoc;
        }

        private bool IsOnObstacle( Location location )
        {
            return Obstacles.Contains( location );
        }

        private bool IsPlayerOnGoal()
        {
            return PlayerSpot == GoalSpot;
        }

        private IEnumerable<Location> GetObstacles()
        {
            // IEnumerable<Location> is covariant <out T>
            // (accepts more derived type). explains why it's readonly.

            List<Location> obstacles = new();
            Random random = new();

            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    //if (Randomizer.NextObstacle())
                    if (random.Next( 0, 100 ) < 25)
                    {
                        obstacles.Add( new Location( i, j ) );
                    }
                }
            }

            return obstacles;
        }

        private bool IsTrapped( Location location )
        {
            HashSet<Location> obstacles = new( Obstacles );

            var surroundings = location.GetSurroundings();
            var validSurrounds = surroundings.Where( loc => IsInBounds( loc ) );

            return validSurrounds.All( loc => obstacles.Contains( loc ) );
        }

        private void ResetPlayerPosition()
        {
            Random random = new();
            do
            {
                PlayerSpot = new Location( random.Next( 0, Rows ), random.Next( 0, Columns ) );
            }
            while (IsPlayerOnGoal() || IsTrapped( PlayerSpot ) || IsOnObstacle( PlayerSpot ));
        }

        private void ResetGoalPosition()
        {
            Random random = new();
            do
            {
                GoalSpot = new Location( random.Next( 0, Rows ), random.Next( 0, Columns ) );
                var old = Board[GoalSpot.X, GoalSpot.Y];

                Board[GoalSpot.X, GoalSpot.Y] = new Tile( old.Weights, old.Elegibility, 1, old.Direction );
            }
            while (IsPlayerOnGoal() || IsTrapped( GoalSpot ) || IsOnObstacle( GoalSpot ));
        }

        private void ResetAllElegibility()
        {
            foreach (Tile tile in Helpers.ToEnumerable( Board ))
            {
                Array.Clear( tile.Elegibility );
                // A copy of array reference passed to Clear(),
                // not a copy the struct (valuetype).
            }
        }

        private int CheckEndOfEpisode()
        {
            if (IsOnObstacle( PlayerSpot ) || !IsInBounds( PlayerSpot ) || IsPlayerOnGoal())
            {
                ResetPlayerPosition();
                ResetAllElegibility();

                _gammaDF -= LAMBDA;

                return 1;
            }

            return 0;
        }

        private Tile GetTile( Location location )
        {
            return Board[location.X, location.Y];
        }

        private Tile GetTile( Location location, Direction direction )
        {
            Location newLocation = GetNewLocation( location, direction );

            return GetTile( newLocation );
        }

        private static Direction GetDirectionOfMaxWeight( float[] weights )
        {
            var (index, _) = Helpers.Max( weights );
            var elem = Helpers.GetEnumByIndex<Direction>( index );
            return elem;
        }

        private int TakeAction()
        {
            // Gather Q(s,a) and gather 'a'

            Random r = new();
            var probability = r.NextSingle();
            var qsa = GetTile( PlayerSpot );
            (int index, float value) max_qsa = Helpers.Max( qsa.Weights );

            // Explore, Exploit
            Direction next_qsa = (probability < _gammaDF)
                ? Helpers.GetRandomEnum<Direction>()
                : Helpers.GetEnumByIndex<Direction>( max_qsa.index );

            // reward in s'
            int reward;
            // Q(s'a')
            (int index, float value) P_max;

            if (IsInBounds( PlayerSpot, next_qsa ))
            {
                Tile P_qsa = next_qsa switch
                {
                    Direction.Up => GetTile( PlayerSpot, Direction.Up ),
                    Direction.Down => GetTile( PlayerSpot, Direction.Down ),
                    Direction.Left => GetTile( PlayerSpot, Direction.Left ),
                    Direction.Right => GetTile( PlayerSpot, Direction.Right ),
                    _ => throw new ArgumentException( $"Unsupported Direction: {next_qsa}" )
                };

                P_max = Helpers.Max( P_qsa.Weights );
                reward = P_qsa.Reward;
            }
            else
            {
                // you attempted Q(s'a') out of bounds
                reward = -1;
                P_max = (-1, 0);
            }

            // delta = r + gamma * Q(s'a') - Q(s,a)
            float delta = reward + (_gammaDF * P_max.value) - max_qsa.value;

            // e(s,a) <- e(s,a) + 1
            qsa.Elegibility[(int)next_qsa] += 1;

            // for all s,a:
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        // Q(s,a) <- Q(s,a) + alpha * delta * e(s,a)
                        Board[i, j].Weights[k] += ALPHA * delta * Board[i, j].Elegibility[k];

                        // e(s,a) <- gamma * lambda * e(s,a)
                        Board[i, j].Elegibility[k] = _gammaDF * LAMBDA * Board[i, j].Elegibility[k];
                    }
                }
            }

            // move player
            // s <- s' ; a <- a'

            PlayerSpot = next_qsa switch
            {
                Direction.Up => new Location( PlayerSpot.X - 1, PlayerSpot.Y ),
                Direction.Down => new Location( PlayerSpot.X + 1, PlayerSpot.Y ),
                Direction.Left => new Location( PlayerSpot.X, PlayerSpot.Y - 1 ),
                Direction.Right => new Location( PlayerSpot.X, PlayerSpot.Y + 1 ),
                _ => throw new ArgumentException( "Unsupported" )
            };

            int ret = CheckEndOfEpisode();

            return ret;
        }

        private StringBuilder BuildCharacterBoard()
        {
            StringBuilder sb = new();

            foreach (var x in Helpers.ToEnumerable( Board ))
            {
                sb.Append( _characters[GetDirectionOfMaxWeight( x.Weights )] );
            }

            // apply obstacles
            foreach (Location ob in Obstacles)
            {
                var idx = ConvertLocationToIndex( ob );
                sb[idx] = _characters['O'];
            }

            // apply player and goal
            var idxPlayer = ConvertLocationToIndex( PlayerSpot );
            var idxGoal = ConvertLocationToIndex( GoalSpot );

            sb[idxPlayer] = _characters["Player"];
            sb[idxGoal] = _characters["Goal"];

            // insert newlines at end of rows

            for (int i = sb.Length - 1; i >= 0; i--)
            {
                if (i % Columns == 0)
                {
                    sb.Insert( i, '\n' );
                }
            }

            // insert spaces for readability

            for (int i = sb.Length - 1; i >= 0; i--)
            {
                sb.Insert( i, ' ' );
            }

            return sb;
        }

        private int ConvertLocationToIndex( Location location )
        {
            return (location.X * Rows) + location.Y;
        }

        public async Task StartAsync()
        {
            Console.WriteLine( $"Id: {_guid}" );
            Console.WriteLine( $"Board Dimensions: {Rows} x {Columns}" );
            Console.WriteLine( $"Obstacles Active: {ObstaclesEnabled}" );

            StringBuilder sbInitial = BuildCharacterBoard();
            Console.WriteLine( $"{sbInitial}\n" );

            long actions = 0;
            long episodes = 0;

            Stopwatch sw = new();
            sw.Restart();

            await Task.Run( () =>
            {
                while (_gammaDF > TERMINATE_VALUE)
                {
                    episodes += TakeAction();
                    actions++;
                    if (actions % CHECKPOINT == 0)
                    {
                        Console.WriteLine( $"Episodes: {episodes}" );
                        Console.WriteLine( $"Actions: {actions}" );
                        Console.WriteLine( $"Gamma: {_gammaDF}" );
                        Console.WriteLine();
                    }
                }
            } );

            sw.Stop();
            StringBuilder sbFinish = BuildCharacterBoard();
            Console.WriteLine( $"Id: {_guid}" );
            Console.WriteLine( $"{sbFinish}\n" );
            Console.WriteLine( $"Solving took {sw.ElapsedMilliseconds / 1e3} seconds" );
            Console.WriteLine( $"Total Episodes: {episodes}" );
            Console.WriteLine( $"Total Actions: {actions}" );
            Console.WriteLine( $"Board Dimensions: {Rows} x {Columns}" );
            Console.WriteLine( $"Obstacles Active: {ObstaclesEnabled}" );
            Console.WriteLine( $"PlayerSpot: {PlayerSpot}" );
            Console.WriteLine( $"GoalSpot: {GoalSpot}" );
            Console.WriteLine( $"------------------------------------" );
        }
    }
}
