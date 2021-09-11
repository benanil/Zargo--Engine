
using OpenTK.Mathematics;
using System;
using System.Runtime.CompilerServices;

namespace ZargoEngine.Mathmatics
{
    public static partial class Mathmatic
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Pow(Vector2 x, Vector2 y)
        {
            Vector2 rv = new(MathF.Exp(x[0] * MathF.Log(y[0])),
                             MathF.Exp(x[1] * MathF.Log(y[1])));
            return rv;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 V3DegreToRadian(this Vector2 from)
        {
            return new Vector2(MathHelper.DegreesToRadians(from.X),
                               MathHelper.DegreesToRadians(from.Y));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static System.Numerics.Vector2 V2DegreToRadian(this System.Numerics.Vector2 from)
        {
            return new System.Numerics.Vector2(MathHelper.DegreesToRadians(from.X),
                                               MathHelper.DegreesToRadians(from.Y));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 V2RadianToDegree(this Vector2 from)
        {
            return new Vector2(MathHelper.RadiansToDegrees(from.X),
                               MathHelper.RadiansToDegrees(from.Y));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static System.Numerics.Vector2 V2RadianToDegree(this System.Numerics.Vector2 from)
        {
            return new System.Numerics.Vector2(MathHelper.RadiansToDegrees(from.X),
                               MathHelper.RadiansToDegrees(from.Y));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Magnitude(this ref Vector2 value)
        {
            float result = value.X + value.Y;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clamp(this ref Vector2 value, float min, float max)
        {
            value.X = Clamp(value.X, min, max);
            value.Y = Clamp(value.Y, min, max);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Min(this ref Vector2 value, float min)
        {
            value.X = MathF.Min(value.X, min);
            value.Y = MathF.Min(value.Y, min);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Max(this ref Vector2 value, float min)
        {
            value.X = MathF.Max(value.X, min);
            value.Y = MathF.Max(value.Y, min);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Abs(this ref Vector2 value)
        {
            value.X = MathF.Abs(value.X);
            value.Y = MathF.Abs(value.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 LerpUnclamped(in Vector2 a, in Vector2 b, in float t)
        {
            return new Vector2(
                a.X + (b.X - a.X) * t,
                a.Y + (b.Y - a.Y) * t
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(this Vector2 a, Vector2 b) => Distance(ref a, ref b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(this ref Vector2 a, ref Vector2 b)
        {
            return Vector2.Distance(a, b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 MoveTowards(in Vector2 current, in Vector2 target, float maxDistanceDelta)
        {
            // avoid vector ops because current scripting backends are terrible at inlining
            float toVector_x = target.X - current.X;
            float toVector_y = target.Y - current.Y;

            float sqdist = toVector_x * toVector_x + toVector_y * toVector_y ;

            if (sqdist == 0 || (maxDistanceDelta >= 0 && sqdist <= maxDistanceDelta * maxDistanceDelta))
                return target;
            var dist = (float)MathF.Sqrt(sqdist);

            return new Vector2(current.X + toVector_x / dist * maxDistanceDelta,
                current.Y + toVector_y / dist * maxDistanceDelta);
        }


    }
}