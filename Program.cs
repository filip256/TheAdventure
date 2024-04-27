using System.Diagnostics;
using Silk.NET.SDL;
using System.Diagnostics;

namespace TheAdventure;

public static class Program
{
    public static void Main()
    {
        var sdl = new Sdl(new SdlContext());

        var sdlInitResult = sdl.Init(Sdl.InitVideo | Sdl.InitEvents | Sdl.InitTimer | Sdl.InitGamecontroller |
                                     Sdl.InitJoystick);
        if (sdlInitResult < 0)
        {
            throw new InvalidOperationException("Failed to initialize SDL.");
        }

        using (var window = new GameWindow(sdl, 800, 480))
        {
            var renderer = new GameRenderer(sdl, window);
            var input = new Input(sdl, window, renderer);
            var engine = new Engine(renderer, input);

            engine.InitializeWorld();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            long tickCount = 0;

            bool quit = false;
            while (!quit)
            {

                quit = input.ProcessInput();
                if (quit) break;
                
                engine.ProcessFrame();
                engine.RenderFrame();

                ++tickCount;

                if (Globals.RUN_PERF_TEST && stopwatch.Elapsed.TotalSeconds > 2)
                {
                    stopwatch.Stop();
                    Console.WriteLine((float)Math.Round(tickCount / stopwatch.Elapsed.TotalSeconds, 2) + " FPS (avg.)");
                    tickCount = 0;
                    stopwatch.Reset();
                    stopwatch.Start();
                }
            }
        }

        sdl.Quit();
    }
}