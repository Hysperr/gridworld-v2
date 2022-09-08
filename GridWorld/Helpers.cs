namespace GridWorldGame
{
    public static class Helpers
    {
        public static IEnumerable<T> ToEnumerable<T>( this T[,] target )
        {
            foreach (var item in target)
                yield return item;
        }

        public static T GetEnumByIndex<T>( int index ) where T : struct, Enum
        {
            return !GetValues<T>().Any()
                ? throw new ArgumentException( "Empty enum" )
                : index < 0
                ? throw new ArgumentOutOfRangeException( $"Index less than 0: {index}" )
                : index >= GetValues<T>().Count()
                ? throw new ArgumentOutOfRangeException( $"Cannot index beyond available enums: {index}" )
                : GetEnum<T>( index );
        }

        public static T GetRandomEnum<T>() where T : struct, Enum
        {
            if (!GetValues<T>().Any())
            {
                throw new ArgumentException( "Empty enum" );
            }

            Random r = new();
            int index = r.Next( GetValues<T>().Count() );

            return GetEnum<T>( index );
        }

        private static T GetEnum<T>( int index ) where T : struct, Enum
        {
            IEnumerable<T> values = GetValues<T>();
            T elem = values.ElementAt( index );
            return elem;
        }

        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues( typeof( T ) ).Cast<T>();
        }

        public static bool CheckNotNull<T>( T refVariable, string name ) where T : class
        {
            if (refVariable != null)
            {
                throw new ArgumentException( $"Null parameter: {name}" );
            }

            return true;
        }

        public static (int index, T value) Max<T>( IEnumerable<T> source )
        {
            IComparer<T> comparer = Comparer<T>.Default;
            var iterator = source.GetEnumerator();

            if (!iterator.MoveNext())
            {
                throw new ArgumentException( "empty sequence" );
            }

            var maxValue = iterator.Current;
            var maxIndex = 0;
            var index = 0;

            while (iterator.MoveNext())
            {
                var current = iterator.Current;
                index++;

                if (comparer.Compare( current, maxValue ) > 0)
                {
                    maxValue = current;
                    maxIndex = index;
                }
            }

            return (maxIndex, maxValue);
        }
    }
}

