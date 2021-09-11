using System;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace ZargoEngine
{
    public static class Debug
    {
        private const string LogString     = "[LOG] ";
        private const string ErrorString   = "[ERROR] ";
        private const string WarningString = "[Warning] ";

        public static void Log(object value,ConsoleColor consoleColor = ConsoleColor.White)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = consoleColor;
            Console.WriteLine(LogString + value.ToString());
            Console.ForegroundColor = oldColor;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void LogName(this object value, ConsoleColor consoleColor = ConsoleColor.White)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = consoleColor;
            Console.WriteLine($"{LogString} {value.GetType()}: {value}");
            Console.ForegroundColor = oldColor;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void LogError(object value)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ErrorString + value.ToString());
            Console.ForegroundColor = oldColor;

            MessageBox.Show(value.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void LogWarning(object value)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(WarningString + value.ToString());
            Console.ForegroundColor = oldColor;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string LogWithtype(this object value) => $"{LogString} {value.GetType()}: {value}";

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool Assert(bool condition, string errorMessage)
        {
            if (condition == true)
            {
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ErrorString);
                Console.ForegroundColor = oldColor;
                MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return condition;
        }

        public static void LogIf(bool condition, string message)
        {
            if (condition == true)
            {
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(message);
                Console.ForegroundColor = oldColor;
            }
        }

        public class SlowDebugger
        {
            private readonly float updateTime;

            private float startime;

            public void LogSlow(object value)
            {
                if (Time.time >= startime + updateTime)
                {
                    startime = Time.time + updateTime;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(LogString + value.ToString());
                }
            }

            public SlowDebugger(float updateTime = .8f)
            {
                startime = Time.time;
                this.updateTime = updateTime;
            }
        }
    }
}