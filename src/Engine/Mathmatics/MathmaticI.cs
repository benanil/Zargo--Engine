using System.Runtime.CompilerServices;

namespace ZargoEngine.Mathmatics
{
    public static partial class Mathmatic
    {
        public const int BigInt = 999999999;
        public const int SmallInt = -999999999;

        public static int Clamp(int value, in int min, in int max) => Clamp(ref value, min, max);

        public static int Remap(this int value, in int FirstMin, in int FirstMax, in int SecondMin, in int SecondMax)
        {
            int devide0 = Max(1, (value - FirstMin));
            int devide1 = Max(1, (FirstMax - FirstMin));
            return devide0 / devide1 * (SecondMax - SecondMin) + SecondMin;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Clamp(ref int value, in int min, in int max)
        {
            if (value < min) value = min;
            else if (value > max) value = max;
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Min(in int first, in int second)
        {
            if (first < second) return first;
            return second;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Max(in int first, in int second)
        {
            if (first > second) return first;
            return second;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Min(params int[] value)
        {
            int smallestValue = BigInt;
            int i = 0;
            for (; i < value.Length; i++)
                if (value[i] < smallestValue)
                    smallestValue = value[i];
            return value[i];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Max(params int[] value)
        {
            int biggestValue = SmallInt;
            int i = 0;
            for (; i < value.Length; i++)
                if (value[i] > biggestValue)
                    biggestValue = value[i];
            return value[i];
        }

    }
}
