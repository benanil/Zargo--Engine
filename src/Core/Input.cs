
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

        // MOUSE

        private static Vector2 mouseOld;

        private static Vector2 mouseAxis;
        public static Vector2 MouseAxis
        {
            get
            {
                mouseAxis = Vector2.Normalize(MousePosition() - mouseOld);
                mouseOld = MousePosition();
                return mouseAxis;
            }
        }

        public unsafe static void SetCursorPos(float x, float y)
        {
            GLFW.SetCursorPos(Program.MainGame.WindowPtr, x, y);
        }

        public static float MouseX() => MouseAxis.X;
        public static float MouseY() => MouseAxis.Y;

        public static float ScrollY;

        public static Vector2 MousePosition() => Program.MainGame.MousePosition;

        public static bool MouseButtonDown(MouseButton mouseButton) => Program.MainGame.IsMouseButtonDown(mouseButton);
        public static bool MouseButtonUp(MouseButton mouseButton) => Program.MainGame.IsMouseButtonReleased(mouseButton);
        public static bool MouseButton(MouseButton mouseButton) => Program.MainGame.IsMouseButtonPressed(mouseButton);
    }
}
