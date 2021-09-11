using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;

namespace ZargoEngine
{
    public static class Screen
    {
        static unsafe Screen()
        {
            Monitors.TryGetMonitorInfo(0, out PrimaryMonitorInfo);
            var windowMode = GLFW.GetVideoMode((Monitor*)PrimaryMonitorInfo.Handle.Pointer.ToPointer());
            RefreshRate = (*windowMode).RefreshRate;
        }

        public static readonly MonitorInfo PrimaryMonitorInfo;

        public static MonitorInfo GetMonitorInfo(int index)
        {
            Monitors.TryGetMonitorInfo(index, out MonitorInfo info);
            return info;
        }

        /// <summary>
        /// returns monitors Height as pixel
        /// </summary>
        public static int MonitorHeight => PrimaryMonitorInfo.VerticalResolution;

        /// <summary>
        /// returns monitors width as pixel
        /// </summary>
        public static int MonitorWidth => PrimaryMonitorInfo.HorizontalResolution;

        public static readonly int RefreshRate;

        public static bool _fullScreen;
        public unsafe static bool FullScreen
        {
            get => _fullScreen;
            set
            {
                _fullScreen = value;
                GLFW.SetWindowMonitor(Program.MainGame.WindowPtr, 
                                     _fullScreen ? (Monitor*)PrimaryMonitorInfo.Handle.Pointer.ToPointer(): null, 
                                     0, 0, MonitorWidth, MonitorHeight, RefreshRate);
            }
        }

        public static Tuple<float, float> GetMainWindowSizeTupple()
        {
            var clinetSize = Program.MainGame.ClientSize;
            return new Tuple<float, float>(clinetSize.X, clinetSize.Y);
        }

        public static Vector2i GetMainWindowSize()
        {
            return Program.MainGame.ClientSize;
        }

        public unsafe static string GetPrimaryMonitorName()
        { 
            return GLFW.GetMonitorName(GLFW.GetPrimaryMonitor());
        }
    }
}
