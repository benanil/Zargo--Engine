

using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ZargoEngine.Helper
{
    public static partial class Extensions
    {
        // list
        public static T GetRandom<T>(this T[] array)
        {
            Random random = new Random((int)DateTime.Now.Ticks);
            return array[random.Next(0, array.Length - 1)];
        }

        public static T GetRandom<T>(this List<T> array)
        {
            Random random = new Random((int)DateTime.Now.Ticks);
            return array[random.Next(0,array.Count-1)];
        }

        public static void Foreach<T>(this T[] array,Action<T> action)
        {
            foreach (var t in array)
            {
                action.Invoke(t);
            }
        }

        // Vector 3
        public static Vector3 SetX(this Vector3 vector, in float value) {
            vector.X = value;
            return vector;
        }

        public static Vector3 SetY(this Vector3 vector, in float value) {
            vector.Y = value;
            return vector;
        }

        public static Vector3 SetZ(this Vector3 vector, in float value)
        {
            vector.Z = value;
            return vector;
        }

        public static Vector3 SetX(this Vector3 vector, Func<float,float> func) {
            vector.X = func.Invoke(vector.X);
            return vector;
        }

        public static Vector3 SetY(this Vector3 vector, Func<float, float> func) {
            vector.Y = func.Invoke(vector.Y);
            return vector;
        }

        public static Vector3 SetZ(this Vector3 vector, Func<float, float> func) {
            vector.Z = func.Invoke(vector.Z);
            return vector;
        }

        public static Vector3 ReplaceXY(this Vector3 vector)
        {
            (vector.X, vector.Y) = (vector.Y, vector.X);
            return vector;
        }

        public static Vector3 ReplaceYZ(this Vector3 vector)
        {
            (vector.Z, vector.Y) = (vector.Y, vector.Z);
            return vector;
        }

        // Vector2
        public static Vector2 SetX(this Vector2 vector, float value) {
            vector.X = value;
            return vector;
        }

        public static Vector2 SetX(this Vector2 vector, Func<float, float> func)
        {
            vector.X = func.Invoke(vector.X);
            return vector;
        }

        public static Vector2 SetY(this Vector2 vector, float value) {
            vector.Y = value;
            return vector;
        }

        public static Vector2 SetY(this Vector2 vector, Func<float, float> func)
        {
            vector.Y = func.Invoke(vector.Y);
            return vector;
        }

        public static List<T> ForeachGet<T>(this List<T> array,Action<T> action)
        {
            array.ForEach(action);
            return array;
        }

        // MATRİX
        /// <summary>
        /// this is little bit slow try to not use evry frame
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="scale"></param>
        /// <returns>created transformation</returns>
        public static Matrix4 TRS(in Vector3 position,in Quaternion rotation,in Vector3 scale)
        {
            return Matrix4.Transpose(Matrix4.CreateTranslation(position) * Matrix4.CreateFromQuaternion(rotation) * Matrix4.CreateScale(scale));
        }

        public static Matrix4 TRS(Transform transform)
        {
            return Matrix4.Transpose(Matrix4.CreateTranslation(transform.position) * Matrix4.CreateFromQuaternion(transform.rotation) * Matrix4.CreateScale(transform.scale));
        }

        public static Vector3 Right(this Matrix4 matrix)
        {
            return new(matrix.Row0[0], matrix.Row1[0], matrix.Row2[0]);
        }

        public static Vector3 Up(this Matrix4 matrix)
        {
            return new(matrix.Row0[1], matrix.Row1[1], matrix.Row2[1]);
        }

        public static Vector3 Forward(this Matrix4 matrix)
        {
            return new(matrix.Row0[2], matrix.Row1[2], matrix.Row2[2]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float RangeI(this Random random, in float minValue, in float maxValue) {
            int sample = random.Next();
            return (maxValue * sample) + (minValue * (1 - sample));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Rangef(this Random random, in float minValue, in float maxValue) {
            float sample = (float)random.NextDouble();
            return (maxValue * sample) + (minValue * (1 - sample));
        }

        /// <summary>
        /// max 255 iteration
        /// </summary>
        public static bool Contains(this string text, params string[] conditions)
        {
            if (string.IsNullOrWhiteSpace(text)) {
                return false;
            }
            for (byte i = 0; i < conditions.Length; i++) 
            {
                if (text.Contains(conditions[i], StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        /// <returns>if string contains all conditions</returns>
        public static bool ContainsAnd(this string text, params string[] conditions)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;
            byte trueCount = 0;

            for (byte i = 0; i < conditions.Length; i++)
            {
                if (text.Contains(conditions[i], StringComparison.OrdinalIgnoreCase)) trueCount++;
            }
            return trueCount == conditions.Length;
        }

        /// <param name="value"> between 0|100</param>
        public static bool Percent(this Random random, byte value)
        {
            return RangeI(random, 0, 100) < value;
        }
    }
}
