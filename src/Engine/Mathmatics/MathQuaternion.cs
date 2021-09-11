

using OpenTK.Mathematics;
using System;
using System.Runtime.CompilerServices;

namespace ZargoEngine.Mathmatics
{
    public static partial class Mathmatic
    {
        public static float Dot(this Quaternion a, Quaternion b) => Dot(ref a, ref b);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(this ref Quaternion a, ref Quaternion b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W;
        }

        public static float Angle(this Quaternion a, Quaternion b) => Angle(ref a, ref b);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Angle(this ref Quaternion a, ref Quaternion b)
        {
            float dot = Dot(ref a, ref b);
            return MathF.Acos(MathF.Min(MathF.Abs(dot), 1.0F)) * 2.0F * Rad2Deg;
        }


        public static Quaternion RotateTowards(this Quaternion from, Quaternion to, float maxDegreesDelta) => RotateTowards(ref from, ref to, ref maxDegreesDelta);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion RotateTowards(this ref Quaternion from, ref Quaternion to,ref float maxDegreesDelta)
        {
            float angle = Angle(ref from,ref to);
            if (angle == 0.0f) return to;
            return Quaternion.Slerp(from, to, MathF.Min(1.0f, maxDegreesDelta / angle));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToEuler(this Quaternion q)
        {
            Vector3 eulerAngles = new Vector3();
            float sinr_cosp = 2 *     (q.W * q.X + q.Y * q.Z);
            float cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
            eulerAngles.X   = MathF.Atan2(sinr_cosp, cosr_cosp); 

            float sinp      = 2 * (q.W * q.Y - q.Z * q.X);
            
            if (MathF.Abs(sinp) >= 1)
                eulerAngles.Y = MathF.CopySign(MathF.PI / 2, sinp);
            else
                eulerAngles.Y = MathF.Asin(sinp);

            float siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
            float cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
            eulerAngles.Z = MathF.Atan2(siny_cosp, cosy_cosp);

            return eulerAngles;
        }
    }
}