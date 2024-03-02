using System;
using System.Runtime.InteropServices.JavaScript;
using FNA.WASM.Sample.Core;

public static partial class Program
{
internal static void Main()
    {
        Console.WriteLine("Setting up main loop");
        SetMainLoop(MainLoop);
    }

    private static bool _firstRun = true;
    private static DateTime _lastLog = DateTime.UnixEpoch;
    private static SampleGame _myGame;

    private static void MainLoop()
    {
        try
        {
            if (_firstRun)
            {
                Console.WriteLine("First run of the main loop");
                _firstRun = false;
            
                _myGame = new SampleGame(); //replace this with your Game subclass
            }

            var now = DateTime.UtcNow;
            if ((now - _lastLog).TotalSeconds > 1.0)
            {
                _lastLog = now;
                Console.WriteLine($"Main loop still running at: {now}");
            }

            if (_myGame != null)
            {
                _myGame.RunOneFrame();
            }
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            throw;
        }
    }

    [JSImport("setMainLoop", "main.js")]
    internal static partial void SetMainLoop([JSMarshalAs<JSType.Function>] Action cb);
}
