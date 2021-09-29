
using OpenTK.Graphics.OpenGL4;
using System;
using OpenTK.Mathematics;
using Assimp;

#nullable disable warnings

namespace ZargoEngine.Rendering
{
    using AIMesh = Assimp.Mesh;

    public class Mesh : MeshBase
    {
        public Mesh(string path) : base(path) { }

        public Mesh(string path, AIMesh mesh) : base(path)
        {
            name = mesh.Name;

            Positions = new Vector3D[mesh.Vertices.Count];
            Normals = new Vector3D[mesh.Vertices.Count];
            TexCoords = new Vector2D[mesh.TextureCoordinateChannels[0].Count];
            
            mesh.Vertices.CopyTo(Positions);
            indices = mesh.GetIndices();
            
            if (mesh.HasNormals) mesh.Normals.CopyTo(Normals);
            
            if (mesh.HasTextureCoords(0))
            {
                for (int i = 0; i < mesh.TextureCoordinateChannels[0].Count; i++)
                {
                    TexCoords[i] = new Vector2D(mesh.TextureCoordinateChannels[0][i].X,
                                                mesh.TextureCoordinateChannels[0][i].Y);
                }
            }
            LoadBuffers();
        }

        public sealed override void LoadBuffers()
        {
            GenerateBoundingBox();

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