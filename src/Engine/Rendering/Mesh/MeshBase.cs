#nullable disable warnings

using Assimp;
using OpenTK.Graphics.OpenGL4;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace ZargoEngine.Rendering
{
    public unsafe abstract class MeshBase : IDisposable
    {
        public Vector3D[] Normals;
        public Vector3D[] Positions;
        public Vector2D[] TexCoords;
        public int[] indices;

        public string name;
        public string path;

        protected int vaoID, vboID, eboID, uvID, normalID;

        public abstract void LoadBuffers();
        internal abstract void Prepare();
        internal abstract void DrawOnce();
        internal abstract void End();

        public MeshBase(in string path)
        {
            this.path = path;
            this.name = Path.GetFileName(path);
        }

        /// <summary>memory efficent position coppying</summary> 
        public virtual unsafe T[] GetPositions<T>() where T : unmanaged
        {
            T[] exportedPositions = new T[Positions.Length];

            for (int i = 0; i < Positions.Length; i++)
            {
                fixed (void* exportPtr = &exportedPositions[i])
                fixed (void* vertexPtr = &Positions[i])
                Unsafe.CopyBlock(exportPtr, vertexPtr, (uint)OpenTK.Mathematics.Vector3.SizeInBytes);
            }
            return exportedPositions;
        }

        /// <summary>memory efficent texCoord coppying</summary> 
        public virtual unsafe T[] GetTexCoords<T>() where T : unmanaged
        {
            T[] exportedPositions = new T[TexCoords.Length];

            for (int i = 0; i < TexCoords.Length; i++)
            {
                fixed (void* exportPtr = &exportedPositions[i])
                fixed (void* texCoordPtr = &TexCoords[i])
                Unsafe.CopyBlock(exportPtr, texCoordPtr, (uint)OpenTK.Mathematics.Vector2.SizeInBytes);
            }
            return exportedPositions;
        }

        protected virtual void DeleteBuffers()
        {
            GL.DeleteVertexArray(vaoID);
            GL.DeleteBuffer(eboID); GL.DeleteBuffer(vboID);
            GL.DeleteBuffer(uvID);  GL.DeleteBuffer(normalID);
        }
     
        public void Dispose()
        {
            DeleteBuffers();
            GC.SuppressFinalize(this);
        }

        public virtual void Draw()
        {
            GL.DrawElements(BeginMode.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);
        }
    }
}
