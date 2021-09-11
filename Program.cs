
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using System;
using System.Diagnostics;
using ZargoEngine.Helper;

#nullable disable warnings

namespace ZargoEngine
{
    using Image = OpenTK.Windowing.Common.Input.Image;
    
    public static class Program
    {
        public static Engine MainGame;

        [STAThread]
        private static void Main(string[] args)
        {
            AdminRelauncher.RelaunchIfNotAdmin();

            //Convert ImageSharp's format into a byte array, so we can use it with OpenGL.
            var pixels = ImageLoader.Load("Images/Engine icon.png", out int width, out int height);

            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;

            GameWindowSettings gameWindowSettings = new GameWindowSettings();

            int monitorHeight = System.Windows.Forms.SystemInformation.PrimaryMonitorSize.Height - 30;
            int monitorWidth  = System.Windows.Forms.SystemInformation.PrimaryMonitorSize.Width;

            NativeWindowSettings nativeWindowSettings = new NativeWindowSettings()
            {
                Title = "Zargo Engine",
                Size = new Vector2i(monitorWidth, monitorHeight),
                Icon = new OpenTK.Windowing.Common.Input.WindowIcon(new Image(width, height, pixels)),
                WindowState = OpenTK.Windowing.Common.WindowState.Maximized,
                NumberOfSamples = 8 // 8x msaa
            };

            using Engine game = new(gameWindowSettings, nativeWindowSettings);
            MainGame = game;

            game.Run();
        }
    }
}
