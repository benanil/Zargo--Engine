
using System;
using System.Runtime.CompilerServices;

namespace ZargoEngine.Mathmatics
{
    // float
    public static partial class Mathmatic
    {
        public const float Deg2Rad = 0.0174533f;
        public const float Rad2Deg = 57.2958f;

        public const float BigFloat = 999999999f;
        public const float SmallFloat = -999999999f;

        public const float threeZeroOne = 0.0001f;
        public const float twoZeroOne = 0.001f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Remap(this float value, float FirstMin = -1, float FirstMax = 1, float SecondMin = 0, float SecondMax = 1)
        {
            float devide0 = Max(1, value - FirstMin);
            float devide1 = Max(1, FirstMax - FirstMin);
            return devide0 / devide1 * (SecondMax - SecondMin) + SecondMin;
        }

        public static float Repeat(in float t,in float length)
        {
            return (float)Math.Clamp(t - Math.Floor(t / length) * length, 0, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp(float value, float min, float max) => Clamp(ref value,min,max);

        public static float Clamp(ref float value, in float min, in float max)
        {
            if (value < min) value = min;
            else if (value > max) value = max;
            return value;
        }

        // Clamps value between 0 and 1 and returns value
        public static float Clamp01(in float value)
        {
            if (value < 0F) return 0F;
            if (value > 1F) return 1F;
            return value;
        }

        public static float Min(in float first, in float second)
        {
            return first < second ? first : second;
        }

        public static float Max(in float first, in float second)
        {
            return first > second ? first : second;
        }

        public static float Min(params float[] value)
        {
            int smallestIndex;
            float smallestValue = BigFloat;

            for (smallestIndex = 0; smallestIndex < value.Length; smallestIndex++)
                if (value[smallestIndex] < smallestValue) smallestValue = value[smallestIndex];
            
            return value[smallestIndex];
        }

        public static float Max(params float[] value)
        {
            int biggestIndex;
            float biggestValue = SmallFloat;

            for (biggestIndex = 0; biggestIndex < value.Length; biggestIndex++)
                if (value[biggestIndex] > biggestValue) biggestValue = value[biggestIndex];
            
            return value[biggestIndex];
        }
        public static float SmoothDamp(in float current, in float target, ref float currentVelocity, in float smoothTime)
        {
            return SmoothDamp(current, target, ref currentVelocity, smoothTime, float.MaxValue, Time.DeltaTime);
        }

        // Gradually changes a value towards a desired goal over time.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SmoothDamp(float current, float target, ref float currentVelocity, float smoothTime, float maxSpeed, float deltaTime)
        {
            // Based on Game Programming Gems 4 Chapter 1.10
            smoothTime = MathF.Max(0.0001F, smoothTime);
            float omega = 2F / smoothTime;

            float x = omega * deltaTime;
            float exp = 1F / (1F + x + 0.48F * x * x + 0.235F * x * x * x);
            float change = current - target;
            float originalTo = target;

            // Clamp maximum speed
            float maxChange = maxSpeed * smoothTime;
            change = Clamp(change, -maxChange, maxChange);
            target = current - change;

            float temp = (currentVelocity + omega * change) * deltaTime;
            currentVelocity = (currentVelocity - omega * temp) * exp;
            float output = target + (change + temp) * exp;

            // Prevent overshooting
            if (originalTo - current > 0.0F == output > originalTo)
            {
                output = originalTo;
                currentVelocity = (output - originalTo) / deltaTime;
            }

            return output;
        }

        public static float SmoothDamp(in float current, in float target, ref float currentVelocity, in float smoothTime, in float maxSpeed)
        {
            return SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, Time.DeltaTime);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SmoothDampAngle(float current, float target, ref float currentVelocity, float smoothTime, float maxSpeed)
        {
            return SmoothDampAngle(current, target, ref currentVelocity, smoothTime, maxSpeed, Time.DeltaTime);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SmoothDampAngle(float current, float target, ref float currentVelocity, float smoothTime)
        {
            var deltaTime = Time.DeltaTime;
            const float maxSpeed = float.MaxValue;
            return SmoothDampAngle(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
        }

        // Gradually changes an angle given in degrees towards a desired goal angle over time.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SmoothDampAngle(float current, float target, ref float currentVelocity, float smoothTime, float maxSpeed, float deltaTime)
        {
            target = current + DeltaAngle(current, target);
            return SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
        }

        public static float DeltaAngle(in float current, in float target)
        {
            float delta = Repeat(target - current, 360.0F);
            if (delta > 180.0F)
                delta -= 360.0F;
            return delta;
        }

        public static float MoveTowardsAngle(in float current, float target, in float maxDelta)
        {
            float deltaAngle = DeltaAngle(current, target);
            if (-maxDelta < deltaAngle && deltaAngle < maxDelta)
                return target;
            target = current + deltaAngle;
            return MoveTowards(current, target, maxDelta);
        }

        public static float LerpAngle(in float a, in float b, in float t)
        {
            var delta = Repeat(b - a, 360);
            if (delta > 180)
                delta -= 360;
            return a + delta * Clamp01(t);
        }

        public static float Lerp(in float a, in float b, in float t)
        {
            return a + (b - a) * Clamp01(t);
        }

        public static float Diffrance(this float a, float b) => Diffrance(ref a, ref b);
        
        public static float Diffrance(this ref float a, ref float b)
        {
            return MathF.Sqrt(MathF.Pow(a-b, 2));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float MoveTowards(float current, float target, float maxDelta)
        {
            if (MathF.Abs(target - current) <= maxDelta)
                return target;
            return current + MathF.Sign(target - current) * maxDelta;
        }

        public static float Frac(this ref float value)
        {
            return value - MathF.Truncate(value);
        }

        public static float LerpUnclamped(in float a, in float b, in float t)
        {
            return a + (b - a) * t;
        }

    }
}
