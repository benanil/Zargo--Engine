
using OpenTK.Mathematics;

namespace ZargoEngine.Physics
{
    using OTKvector3 = Vector3;
    using OTKmatrix = Matrix4;
    using OTKquaternion = Quaternion;
    using Bmatrix = BulletSharp.Math.Matrix;
    
    public static class BulletConverter
    {
        public static OTKmatrix ToZargo(this Bmatrix bm)
        {
            OTKmatrix um = new OTKmatrix();
            um[0, 0] = bm[0, 0];
            um[0, 1] = bm[1, 0];
            um[0, 2] = bm[2, 0];
            um[0, 3] = bm[3, 0];

            um[1, 0] = bm[0, 1];
            um[1, 1] = bm[1, 1];
            um[1, 2] = bm[2, 1];
            um[1, 3] = bm[3, 1];

            um[2, 0] = bm[0, 2];
            um[2, 1] = bm[1, 2];
            um[2, 2] = bm[2, 2];
            um[2, 3] = bm[3, 2];

            um[3, 0] = bm[0, 3];
            um[3, 1] = bm[1, 3];
            um[3, 2] = bm[2, 3];
            um[3, 3] = bm[3, 3];
            return um;
        }

        public static Bmatrix ToBullet(this OTKmatrix um)
        {
            Bmatrix bm = new();
            um.ToBullet(ref bm);
            return bm;
        }

        public static void ToBullet(this OTKmatrix um, ref Bmatrix bm)
        {
            bm[0, 0] = um[0, 0];
            bm[0, 1] = um[1, 0];
            bm[0, 2] = um[2, 0];
            bm[0, 3] = um[3, 0];

            bm[1, 0] = um[0, 1];
            bm[1, 1] = um[1, 1];
            bm[1, 2] = um[2, 1];
            bm[1, 3] = um[3, 1];

            bm[2, 0] = um[0, 2];
            bm[2, 1] = um[1, 2];
            bm[2, 2] = um[2, 2];
            bm[2, 3] = um[3, 2];

            bm[3, 0] = um[0, 3];
            bm[3, 1] = um[1, 3];
            bm[3, 2] = um[2, 3];
            bm[3, 3] = um[3, 3];
        }

        /// <summary>
        /// Extract translation from transform matrix.
        /// </summary>
        /// <param name="matrix">Transform matrix. This parameter is passed by reference
        /// to improve performance; no changes will be made to it.</param>
        /// <returns>
        /// Translation offset.
        /// </returns>
        public static OTKvector3 ExtractTranslationFromMatrix(ref OTKmatrix matrix)
        {
            OTKvector3 translate = new OTKvector3();
            translate.X = matrix.M13;
            translate.Y = matrix.M23;
            translate.Z = matrix.M33;
            return translate;
        }

        public static OTKvector3 ExtractTranslationFromMatrix(this Bmatrix matrix)
        {
            OTKvector3 translate = new OTKvector3();
            translate.X = matrix.M41;
            translate.Y = matrix.M42;
            translate.Z = matrix.M43;

            return translate;
        }

        public static OTKquaternion ExtractRotationFromMatrix(this Bmatrix matrix)
        {
            OTKvector3 forward = new OTKvector3();
            forward.X = matrix.M31;
            forward.Y = matrix.M32;
            forward.Z = matrix.M33;

            OTKvector3 upwards = new OTKvector3();
            upwards.X = matrix.M21;
            upwards.Y = matrix.M22;
            upwards.Z = matrix.M23;

            return OTKquaternion.FromMatrix(new Matrix3(forward, upwards, OTKvector3.Cross(forward, upwards)));
        }

        /// <summary>
        /// Extract scale from transform matrix.
        /// </summary>
        /// <param name="matrix">Transform matrix. This parameter is passed by reference
        /// to improve performance; no changes will be made to it.</param>
        /// <returns>
        /// Scale vector.
        /// </returns>
        public static Vector3 ExtractScaleFromMatrix(this ref OTKmatrix matrix)
        {
            Vector3 scale;
            scale.X = new Vector4(matrix.M11, matrix.M21, matrix.M31, matrix.M41).Length;
            scale.Y = new Vector4(matrix.M12, matrix.M22, matrix.M32, matrix.M42).Length;
            scale.Z = new Vector4(matrix.M13, matrix.M23, matrix.M33, matrix.M43).Length;
            return scale;
        }

        public static Vector3 ExtractScaleFromMatrix(this Bmatrix matrix)
        {
            Vector3 scale;
            scale.X = new Vector4(matrix.M11, matrix.M12, matrix.M13, matrix.M14).Length;
            scale.Y = new Vector4(matrix.M21, matrix.M22, matrix.M23, matrix.M24).Length;
            scale.Z = new Vector4(matrix.M31, matrix.M32, matrix.M33, matrix.M34).Length;
            return scale;
        }

        ///// <summary>
        ///// Extract position, rotation and scale from TRS matrix.
        ///// </summary>
        ///// <param name="matrix">Transform matrix. This parameter is passed by reference
        ///// to improve performance; no changes will be made to it.</param>
        ///// <param name="localPosition">Output position.</param>
        ///// <param name="localRotation">Output rotation.</param>
        ///// <param name="localScale">Output scale.</param>
        //public static void DecomposeMatrix(ref Matrix4x4 matrix, out Vector3 localPosition, out Quaternion localRotation, out Vector3 localScale)
        //{
        //    localPosition = ExtractTranslationFromMatrix(ref matrix);
        //    localRotation = ExtractRotationFromMatrix(ref matrix);
        //    localScale = ExtractScaleFromMatrix(ref matrix);
        //}

        ///// <summary>
        ///// Set transform component from TRS matrix.
        ///// </summary>
        ///// <param name="transform">Transform component.</param>
        ///// <param name="matrix">Transform matrix. This parameter is passed by reference
        ///// to improve performance; no changes will be made to it.</param>
        //public static void SetTransformFromMatrix(Transform transform, ref Matrix4x4 matrix)
        //{
        //    transform.localPosition = ExtractTranslationFromMatrix(ref matrix);
        //    transform.localRotation = ExtractRotationFromMatrix(ref matrix);
        //    transform.localScale = ExtractScaleFromMatrix(ref matrix);
        //}

    }
}
