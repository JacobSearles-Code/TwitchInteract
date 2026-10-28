using System;

class Program
{
    static void Main(string[] args)
    {
        TwitchReader twitchReader = new TwitchReader();
        twitchReader.Start();

        Console.WriteLine("Bot is running. Press Enter to exit.");
        Console.ReadLine();
    }
}