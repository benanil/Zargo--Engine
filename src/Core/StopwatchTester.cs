using System;
using System.Diagnostics;

namespace ZargoEngine.Core
{
    public static class StopwatchTester
    {
        /// <summary>
        /// calculates 2 diffrent method's speed
        /// </summary>
        /// <param name="first">method and methods name</param>
        /// <param name="second">method and methods name</param>
        public static void Test(Tuple<Action,string> first, Tuple<Action, string> second,in long iteration)
        {
            Stopwatch stopwatch = new Stopwatch();
            Console.WriteLine(first.Item2);

            stopwatch.Start();

            int i;

            for (i = 0; i < iteration; i++) {
                first.Item1();
            }

            Console.WriteLine(stopwatch.ElapsedMilliseconds);
            
            stopwatch.Reset();
            stopwatch.Start();

            Console.WriteLine(second.Item2);

            for (i = 0; i < iteration; i++) {
                second.Item1();
            }

            Console.WriteLine(stopwatch.ElapsedMilliseconds);
            stopwatch.Stop();
        }
    }
}
