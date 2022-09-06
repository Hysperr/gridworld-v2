// See https://aka.ms/new-console-template for more information

using GridWorldGame;

public class Program
{
    public static async Task Main(string[] args)
    {
        GridWorld g1 = new(15, 15, true);
        await g1.StartAsync();

        Console.WriteLine("Done!");
    }
}



