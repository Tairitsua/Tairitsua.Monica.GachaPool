using System.Diagnostics;
using System.Text;
using MoLibrary.GachaPool.Conventions;
using MoLibrary.GachaPool.Implements;

namespace MoLibrary.GachaPool.Console;
/// <summary>
/// A utility class for updating console output in place, maintaining cursor position.
/// </summary>
public class ConsoleWriteUpdater
{
    private (int, int)? _previousCursorPosition;

    /// <summary>
    /// Updates the console output with the content of a StringBuilder.
    /// </summary>
    /// <param name="stringBuilder">The StringBuilder containing the text to display.</param>
    public void Update(StringBuilder stringBuilder) => Update(stringBuilder?.ToString());

    /// <summary>
    /// Updates the console output at the previous cursor position with new text.
    /// </summary>
    /// <param name="data">The text to display.</param>
    public void Update(string data)
    {
        if (_previousCursorPosition == null)
        {
            _previousCursorPosition = System.Console.GetCursorPosition();//The cursor position depends on your console window size.
        }
        else
        {
            System.Console.SetCursorPosition(_previousCursorPosition.Value.Item1, _previousCursorPosition.Value.Item2);
        }
        System.Console.Write(data);
    }
        
    // can't support \n
    // private int _previousDataLength = 0;
    // public void Update(string data)
    // {
    //     data ??= "";
    //     var backup = new string('\b', _previousDataLength); //The '\b' character in C# is the backspace character. It moves the cursor one position back in the console window.
    //     Console.Write(backup);
    //     Console.Write(data);
    //     _previousDataLength = data.Length;
    // }
}

/// <summary>
/// Main program class containing card pool demonstration methods.
/// </summary>
public static class Program
{
    /// <summary>
    /// Entry point of the application.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    private static void Main(string[] args)
    {
        //StartSerialDraw();
        StartParallelDraw();
    }
        
    private static CardsPool _pool;

    /// <summary>
    /// Sets up a basic card pool with one-star cards for demonstration.
    /// </summary>
    public static void BasicSetup()
    {
        var oneStarCards = Card<int>.CreateMultiCards(CardRarity.OneStar,
            1, 2, 3, 4, 5);
        var twoStarCards = Card<int>.CreateMultiCards(CardRarity.TwoStar, 6, 7, 8, 9, 10);
        oneStarCards[4].PresetProbability = 0.000001;
        oneStarCards[1].TotalCount = 50000;
        _pool = new CardsPool(oneStarCards, twoStarCards);
        _pool.SetPoolRarityProbability(CardRarity.TwoStar, 0.3);
    }
        
        
    /// <summary>
    /// Starts a parallel card drawing demonstration using multiple threads.
    /// </summary>
    public static void StartParallelDraw()
    {
        BasicSetup();
        System.Console.WriteLine(_pool.GetPoolProbabilityInfo());
        var drawer = new CardDrawer(_pool);
        var writer = new ConsoleWriteUpdater();
        var sleepTime = 1000;
        var previousDrawTimes = 0;
        var threadCount = 13;
        void Action()
        {
            while (true)
            {
                var drawOutCard = drawer.DrawCard();
            }
        }

        for (var i = 0; i < threadCount; i++)
        {
            Task.Factory.StartNew(Action);
        }
        while (true)
        {
            Thread.Sleep(sleepTime);
            var perMsGetCard = (drawer.Statistician.RecordedTimes - previousDrawTimes) / sleepTime;
            previousDrawTimes = drawer.Statistician.RecordedTimes;
            writer.Update($"{threadCount} threads 1ms could draw {perMsGetCard} cards\n{drawer.Statistician.GetReport().GetTableString()}");
        }
    }

    /// <summary>
    /// Starts a serial card drawing demonstration using a single thread.
    /// </summary>
    public static void StartSerialDraw()
    {
        BasicSetup();
        System.Console.WriteLine(_pool.GetPoolProbabilityInfo());
        var drawer = new CardDrawer(_pool);
        var writer = new ConsoleWriteUpdater();
        var times = 100000;
        while (true)
        {
            Thread.Sleep(1000);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            for (var i = 0; i < times; i++)
            {
                var drawOutCard = drawer.DrawCard();
                // var drawOutCard = drawer.DrawCardExcept(new Card<int>(1), new Card<int>(2));
            }

            stopwatch.Stop();
            writer.Update($"draw {times} cards take {stopwatch.ElapsedMilliseconds}ms\n{drawer.Statistician.GetReport().GetTableString()}");
        }
    }
}