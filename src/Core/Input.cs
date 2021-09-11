
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Runtime.InteropServices;

namespace ZargoEngine
{
    public static partial class Input
    {
        // it will only work windows
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
        public static extern short GetKeyState(int keyCode);

        private static float horizontal;
        private static float vertical;
        public static Vector2 mouseWheelOffset;

        public static bool CapslockOn => (((ushort)GetKeyState(0x14)) & 0xffff) != 0;
        public static bool NumLock    => (((ushort)GetKeyState(0x90)) & 0xffff) != 0;
        public static bool ScrollLock => (((ushort)GetKeyState(0x91)) & 0xffff) != 0;

        public static bool Any()
        {
            return Program.MainGame.IsAnyKeyDown;
        }

        public static float Horizontal(){
            float target = 0;
            if (GetKey(Keys.A) || GetKey(Keys.Left))  target += 1;
            if (GetKey(Keys.D) || GetKey(Keys.Right)) target -= 1;
            horizontal = MathHelper.Lerp(horizontal, target, Time.DeltaTime * 50);
            return horizontal;
        }

        public static float Vertical(){
            float target = 0;
            if (GetKey(Keys.W) || GetKey(Keys.Up))   target += 1;
            if (GetKey(Keys.S) || GetKey(Keys.Down)) target -= 1;

            vertical = MathHelper.Lerp(vertical, target, Time.DeltaTime * 50);

            return vertical;
        }

        public static Vector2 KeyAxis()         => new (Horizontal(), Vertical()); 
        
        public static bool GetKeyDown(Keys key) => Program.MainGame.IsKeyPressed(key);
        public static bool GetKey(Keys key)     => Program.MainGame.IsKeyDown(key);
        public static bool GetKeyUp(Keys key)   => Program.MainGame.IsKeyReleased(key);
    }
}
