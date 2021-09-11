
using System;

namespace ZargoEngine
{
    public static class Time
    {
        public static TimeSpan StartTimeSpan;

        public static float DeltaTime;
        public static float StartTime;
        public static float time;

        public static float TimeSinceStartUp
        {
            get
            {
                return (float)(DateTime.Now.TimeOfDay - StartTimeSpan).TotalSeconds;
            }
        }

        public static void Tick(in float deltaTime)
        {
            time += DeltaTime;
            DeltaTime = deltaTime;
        }

        public static void Start()
        {
            StartTimeSpan = DateTime.Now.TimeOfDay;
            StartTime     = (float)DateTime.Now.TimeOfDay.TotalSeconds;
        }
    }
}
