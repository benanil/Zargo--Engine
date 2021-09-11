using Assimp;
using OpenTK.Mathematics;
using System.Runtime.CompilerServices;

namespace ZargoEngine.Helper
{
    public static class AssimpConvert
    {
        public static Vector3 ExportScale(this in Matrix4x4 mat)
            => new Vector3(mat.A1, mat.B2, mat.B3);

        public static Vector3 ExportTranslation(this in Matrix4x4 mat)
            => new Vector3(mat.A4, mat.B4, mat.C4);

        public static ref Vector2 toTKRef(this ref Vector2D from)
            => ref Unsafe.As<Vector2D, Vector2>(ref from);

        public static ref Vector3 toTKRef(this ref Vector3D from) 
            => ref Unsafe.As<Vector3D, Vector3>(ref from);

        public static Matrix4 toTK(this Matrix4x4 matrix) 
            => Unsafe.As<Matrix4x4, Matrix4>(ref matrix);

        public static ref Matrix4 toTKRef(this ref Matrix4x4 matrix) 
            => ref Unsafe.As<Matrix4x4, Matrix4>(ref matrix);

        public static ref OpenTK.Mathematics.Quaternion toTK(this ref Assimp.Quaternion quaternion) 
            => ref Unsafe.As<Assimp.Quaternion, OpenTK.Mathematics.Quaternion>(ref quaternion);

        ////////////////////
        
        public static ref Vector3D ToAssimpRef(this ref Vector3 from) 
            => ref Unsafe.As<Vector3, Vector3D>(ref from);

        public static ref Vector2D ToAssimpRef(ref Vector2 from) 
            => ref Unsafe.As<Vector2, Vector2D>(ref from);

        public static ref Matrix4x4 ToAssimpRef(this ref Matrix4 from)
            => ref Unsafe.As<Matrix4, Matrix4x4>(ref from);
    }
}
