using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

#nullable disable warnings

namespace ZargoEngine.Rendering
{
    using AVec3 = Assimp.Vector3D;
    using AVec2 = Assimp.Vector2D;

    public class SkinnedMesh : MeshBase
    {
        public Vector4[] weights; public Vector4i[] ids;
        int weightID, idsID;

        public SkinnedMesh(string path,
            AVec3[] positions, AVec2[] texCoords,
            AVec3[] normals  , int[] indices    ,
            Vector4[] weights, Vector4i[] ids) : base(path)
        {
            this.Positions = positions; this.TexCoords = texCoords;
            this.Normals = normals;     this.indices = indices;
            this.weights = weights;     this.ids = ids;
            this.path = path;
            LoadBuffers();

        }

        // call this after some calculation and here you go you deformed mesh multithreading recomennded use paralel class
        public sealed override void LoadBuffers()
        {
            vaoID = GL.GenVertexArray();
            GL.BindVertexArray(vaoID);

            vboID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboID);
            GL.BufferData(BufferTarget.ArrayBuffer, Vector3.SizeInBytes * Positions.Length, Positions, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vector3.SizeInBytes, IntPtr.Zero);
            GL.EnableVertexAttribArray(0);

            uvID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, uvID);
            GL.BufferData(BufferTarget.ArrayBuffer, Vector2.SizeInBytes * Positions.Length, TexCoords, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, true, Vector2.SizeInBytes, IntPtr.Zero);
            GL.EnableVertexAttribArray(1);

            normalID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, normalID);
            GL.BufferData(BufferTarget.ArrayBuffer, Vector3.SizeInBytes * Positions.Length, Normals, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, true, Vector3.SizeInBytes, IntPtr.Zero);
            GL.EnableVertexAttribArray(2);

            weightID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, weightID);
            GL.BufferData(BufferTarget.ArrayBuffer, Vector4.SizeInBytes * Positions.Length, weights, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, true, Vector4.SizeInBytes, IntPtr.Zero);
            GL.EnableVertexAttribArray(3);

            idsID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, idsID);
            GL.BufferData(BufferTarget.ArrayBuffer, Vector4.SizeInBytes * Positions.Length, ids, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Int, false, Vector4.SizeInBytes, IntPtr.Zero);
            GL.EnableVertexAttribArray(4);

            eboID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, eboID);
            GL.BufferData(BufferTarget.ElementArrayBuffer, sizeof(int) * indices.Length, indices, BufferUsageHint.StaticDraw);
        }

        internal override void Prepare()
        {
            GL.BindVertexArray(vaoID);
        }

        internal override void DrawOnce()
        {
            Prepare();
            {
                GL.DrawElements(BeginMode.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);
            }
            End();
        }

        internal override void End()
        {
            GL.BindVertexArray(0);
            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);
            GL.DisableVertexAttribArray(2);
            GL.DisableVertexAttribArray(3);
            GL.DisableVertexAttribArray(4);
        }
        
        protected override void DeleteBuffers()
        {
            base.DeleteBuffers();
            GL.DeleteBuffers(2, new int[] { idsID, weightID });
        }
    }
}
