
using System;
using System.Runtime.InteropServices;

namespace ZargoEngine.Bindings
{
    public static class MouseBindings
    {
        /// <summary>
        /// Struct representing a point.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public override string ToString()
            {
                return $"x: {X} y: {Y}";
            }
        }

        /// <summary>
        /// Retrieves the cursor's position, in screen coordinates.
        /// </summary>
        /// <see>See MSDN documentation for further information.</see>
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        /// <summary>
        /// sets the cursor's position, in screen coordinates.
        /// </summary>
        [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
        public static extern bool SetCursorPos(int x, int y);

        //used for sending the mouse events like
        [DllImport("user32.dll")]
        private static extern void mouse_event(UInt32 dwFlags, UInt32 dx, UInt32 dy, UInt32 dwData, IntPtr dwExtraInfo);

        //constants for the mouse events
        //found at http://msdn.microsoft.com/en-us/library/ms646273(v=vs.85).aspx
        private const UInt32 MOUSEEVENTF_LEFTDOWN   = 0x0002;  //The left   button was pressed
        private const UInt32 MOUSEEVENTF_LEFTUP     = 0x0004;  //The left   button was released.
        private const UInt32 MOUSEEVENTF_RIGHTDOWN  = 0x08  ;  //The right  button was pressed
        private const UInt32 MOUSEEVENTF_RIGHTUP    = 0x10  ;  //The left   button was released.
        private const UInt32 MOUSEEVENTF_MIDDLEDOWN = 0x0020;  //The middle button was pressed
        private const UInt32 MOUSEEVENTF_MIDDLEUP   = 0x0040;  //The middle button was released.

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(UInt16 virtualKeyCode);

        //Virtual key codes
        //found at http://msdn.microsoft.com/en-us/library/dd375731(v=VS.85).aspx
        private const UInt16 VK_MBUTTON = 0x04; //middle mouse button
        private const UInt16 VK_LBUTTON = 0x01; //left mouse button
        private const UInt16 VK_RBUTTON = 0x02; //right mouse button

        public static void InfiniteMouse()
        {
            // infinite mouse
            GetCursorPos(out MouseBindings.POINT point);

            // fixed by adding -3 stuff
            if (point.X > Screen.MonitorWidth - 2)
            {
                SetCursorPos(2, point.Y);
                Input.SetCursorPos(2, point.Y);
            }
            
            if (point.X < 2)
            {
                SetCursorPos(Screen.MonitorWidth - 3, point.Y);
                Input.SetCursorPos(Screen.MonitorWidth - 3, point.Y); 
            }


            if (point.Y > Screen.MonitorHeight - 2)
            { 
                SetCursorPos(point.X, 2); 
                Input.SetCursorPos(point.X, 2); 
            }
            
            if (point.Y < 2)
            { 
                SetCursorPos(point.X, Screen.MonitorHeight - 3); 
                Input.SetCursorPos(point.X, Screen.MonitorHeight - 3); 
            }

        }

        ///<summary>
        /// Returns negative when the button is DOWN and 0 when the button is UP
        ///</summary>
        private static short GetMiddleButtonState()
        {
            return GetAsyncKeyState(VK_MBUTTON);
        }

        ///<summary>
        /// Returns negative when the button is DOWN and 0 when the button is UP
        ///</summary>
        private static short GetRightButtonState()
        {
            return GetAsyncKeyState(VK_RBUTTON);
        }

        ///<summary>
        /// Returns negative when the button is DOWN and 0 when the button is UP
        ///</summary>
        private static short GetLeftButtonState()
        {
            return GetAsyncKeyState(VK_LBUTTON);
        }

        ///<summary>
        /// Self explanatory
        ///</summary>
        public static bool IsMiddleButtonUp => (GetMiddleButtonState() == 0);

        ///<summary>
        /// Self explanatory
        ///</summary>
        public static bool IsLeftButtonUp => (GetLeftButtonState() == 0);

        ///<summary>
        /// Self explanatory
        ///</summary>
        public static bool IsRightButtonUp => (GetRightButtonState() == 0);

        ///<summary>
        /// Sends the Down and Up Events for the Left Mouse Button
        ///</summary>
        public static void SendClick()
        {
            SendDown();
            SendUp();
        }
        
        ///<summary>
        /// Clicks as a normal user does,
        /// if the mouse button is up the button will be pressed
        /// if the mouse button is down it will be released
        ///</summary>
        public static void SendClickUser()
        {
            if (IsLeftButtonUp) SendDown();
            else                SendUp();
        }

        ///<summary>
        /// Sends the Down and Up Events for the Right Mouse Button
        ///</summary>
        public static void SendRightClick()
        {
            SendDownRight();
            SendUpRight();
        }

        ///<summary>
        /// Clicks as a normal user does,
        /// if the mouse button is up the button will be pressed
        /// if the mouse button is down it will be released
        ///</summary>
        public static void SendRightClickUser()
        {
            if (IsRightButtonUp) SendDownRight();
            else                 SendUpRight();
        }

        ///<summary>
        /// Send the event Up for the Left Mouse Button
        ///</summary>
        public static void SendUp()
        {
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, new IntPtr());
        }

        ///<summary>
        /// Send the event Down for the Left Mouse Button
        ///</summary>
        public static void SendDown()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, new IntPtr());
        }

        ///<summary>
        /// Send the event Up for the Right Mouse Button
        ///</summary>
        public static void SendUpRight()
        {
            mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, new IntPtr());
        }

        ///<summary>
        /// Send the event Down for the Right Mouse Button
        ///</summary>
        public static void SendDownRight()
        {
            mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, new IntPtr());
        }

        ///<summary>
        /// Send the event Up for the Middle Mouse Button
        ///</summary>
        public static void SendMiddleUp()
        {
            mouse_event(MOUSEEVENTF_MIDDLEUP, 0, 0, 0, new IntPtr());
        }

        ///<summary>
        /// Send the event Down for the Middle Mouse Button
        ///</summary>
        public static void SendMiddleDown()
        {
            mouse_event(MOUSEEVENTF_MIDDLEDOWN, 0, 0, 0, new IntPtr());
        }
    }
}
