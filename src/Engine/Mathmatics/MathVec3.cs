

using OpenTK.Mathematics;
using System;
using System.Runtime.CompilerServices;

namespace ZargoEngine.Mathmatics
{
    public static partial class Mathmatic
    {
        public static Vector3 Pow(in Vector3 x, in Vector3 y)
        {
            Vector3 rv = new Vector3();

            for (int i = 0; i < 3; i++)
            {
                rv[i] = MathF.Exp(x[i] * MathF.Log(y[i]));
            }

            return rv;
        }

        public static Vector3 V3DegreToRadian(in this Vector3 from)
        {
            return new Vector3(MathHelper.DegreesToRadians(from.X),
                               MathHelper.DegreesToRadians(from.Y),
                               MathHelper.DegreesToRadians(from.Z));
        }

        public static System.Numerics.Vector3 V3DegreToRadian(in this System.Numerics.Vector3 from)
        {
            return new System.Numerics.Vector3(MathHelper.DegreesToRadians(from.X),
                               MathHelper.DegreesToRadians(from.Y),
                               MathHelper.DegreesToRadians(from.Z));
        }

        public static Vector3 LerpAngle(Vector3 from, in Vector3 to, in float t)
        {
            from.X = LerpAngle(from.X, to.X, t);
            from.Y = LerpAngle(from.Y, to.Y, t);
            from.Z = LerpAngle(from.Z, to.Z, t);
            return from;    
        }

        public static Vector3 V3RadianToDegree(in this Vector3 from)
        {
            return new Vector3(MathHelper.RadiansToDegrees(from.X),
                               MathHelper.RadiansToDegrees(from.Y),
                               MathHelper.RadiansToDegrees(from.Z));
        }

        public static System.Numerics.Vector3 V3RadianToDegree(in this System.Numerics.Vector3 from)
        {
            return new System.Numerics.Vector3(MathHelper.RadiansToDegrees(from.X),
                               MathHelper.RadiansToDegrees(from.Y),
                               MathHelper.RadiansToDegrees(from.Z));
        }
        
        public static float Magnitude(this Vector3 value){
            float result = value.X + value.Y + value.Z;
            return result;
        }

        public static void Clamp(this ref Vector3 value, in float min, in float max)
        {
            value.X = Clamp(value.X, min, max);
            value.Y = Clamp(value.Y, min, max);
            value.Z = Clamp(value.Z, min, max);
        }

        public static void Min(this ref Vector3 value, in float min)
        {
            value.X = MathF.Min(value.X, min);
            value.Y = MathF.Min(value.Y, min);
            value.Z = MathF.Min(value.Z, min);
        }

        public static void Max(this ref Vector3 value, in float min)
        {
            value.X = MathF.Max(value.X, min);
            value.Y = MathF.Max(value.Y, min);
            value.Z = MathF.Max(value.Z, min);
        }

        public static void Abs(this ref Vector3 value)
        {
            value.X = MathF.Abs(value.X);
            value.Y = MathF.Abs(value.Y);
            value.Z = MathF.Abs(value.Z);
        }

        public static Vector3 LerpUnclamped(in Vector3 a, in Vector3 b, in float t)
        {
            return new Vector3(
                a.X + (b.X - a.X) * t,
                a.Y + (b.Y - a.Y) * t,
                a.Z + (b.Z - a.Z) * t
            );
        }

        public static Vector3 MoveTowards(in Vector3 current, in Vector3 target, in float maxDistanceDelta)
        {
            // avoid vector ops because current scripting backends are terrible at inlining
            float toVector_x = target.X - current.X;
            float toVector_y = target.Y - current.Y;
            float toVector_z = target.Z - current.Z;

            float sqdist = toVector_x * toVector_x + toVector_y * toVector_y + toVector_z * toVector_z;

            if (sqdist == 0 || (maxDistanceDelta >= 0 && sqdist <= maxDistanceDelta * maxDistanceDelta))
                return target;
            var dist = (float)MathF.Sqrt(sqdist);

            return new Vector3(current.X + toVector_x / dist * maxDistanceDelta,
                current.Y + toVector_y / dist * maxDistanceDelta,
                current.Y + toVector_z / dist * maxDistanceDelta);
        }

        public static float Distance(this Vector3 a, Vector3 b) => Distance(ref a, ref b);
        
        public static float Distance(this ref Vector3 a, ref Vector3 b)
        {
            return Vector3.Distance(a, b);
        }

    }
}
