
using OpenTK.Graphics.OpenGL4;
using System;
using OpenTK.Mathematics;

#nullable disable warnings

namespace ZargoEngine.Rendering
{
    public class Mesh : MeshBase
    {
        public unsafe Mesh(string path) : base(path)
        {
            AssimpImporter.LoadMesh(path, out Positions, out TexCoords, out Normals, out indices);
                 
            LoadBuffers();
        }

        public override void LoadBuffers()
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
            GL.BufferData(BufferTarget.ArrayBuffer, Vector2.SizeInBytes * TexCoords.Length, TexCoords, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, true, Vector2.SizeInBytes, IntPtr.Zero);
            GL.EnableVertexAttribArray(1);

            normalID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, normalID);
            GL.BufferData(BufferTarget.ArrayBuffer, Vector3.SizeInBytes * Normals.Length, Normals, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, true, Vector3.SizeInBytes, IntPtr.Zero);
            GL.EnableVertexAttribArray(2);

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
            Draw();
            End();
        }

        internal override void End()
        {
            GL.BindVertexArray(0);
            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);
            GL.DisableVertexAttribArray(2);
        }
    }
}