// See https://aka.ms/new-console-template for more information

namespace GridWorldGame;

public class Program
{
    public static async Task Main( string[] args )
    {
        GridWorld g1 = new( 10, 10, true )
        {
            ObstaclePercent = 45,
        };
        
        var g1Task = g1.StartAsync();

        //GridWorld g2 = new( 15, 15, true );
        //var g2Task = g2.StartAsync();

        await Task.WhenAll( new Task[] { g1Task, /*g2Task*/ } );

        Console.WriteLine( "Done!" );
    }
}
