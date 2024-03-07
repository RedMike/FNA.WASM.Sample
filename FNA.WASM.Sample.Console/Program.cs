using FNA.WASM.Sample.Core;

static class Program
{
    static void Main()
    {
        using var game = new SampleGame();
        game.OnAudioAllowedToInit();
        game.Run();
    }
}