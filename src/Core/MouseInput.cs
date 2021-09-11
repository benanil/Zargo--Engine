
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace ZargoEngine
{
    public static partial class Input
    {
        private static Vector2 mouseOld;

        private static Vector2 mouseAxis;
        public static Vector2 MouseAxis{
            get{
                mouseAxis = Vector2.Normalize(MousePosition() - mouseOld);
                mouseOld = MousePosition();
                return mouseAxis;        
            }
        }

        public unsafe static void SetCursorPos(float x, float y)
        {
            GLFW.SetCursorPos(Program.MainGame.WindowPtr, x , y);
        }

        public static float MouseX() => MouseAxis.X;
        public static float MouseY() => MouseAxis.Y;

        public static float ScrollY;

        public static Vector2 MousePosition() => Program.MainGame.MousePosition;

        public static bool MouseButtonDown(MouseButton mouseButton) => Program.MainGame.IsMouseButtonDown(mouseButton);
        public static bool MouseButtonUp(MouseButton mouseButton)   => Program.MainGame.IsMouseButtonReleased(mouseButton);
        public static bool MouseButton(MouseButton mouseButton)     => Program.MainGame.IsMouseButtonPressed(mouseButton);
    }
}
