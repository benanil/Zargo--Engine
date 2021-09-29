#nullable disable warnings

using Assimp;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
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

        public Box3d boundingBox;

        public string name;
        /// <summary> binary file name </summary>
        public string path;

        protected int vaoID, vboID, eboID, uvID, normalID;

        public abstract void LoadBuffers();
        internal abstract void Prepare();
        internal abstract void DrawOnce();
        internal abstract void End();

        internal string GetIdentifier() => path + '|' + name;

        /// <summary> saves mesh as binary </summary>
        internal void SaveToFile(BinaryWriter writer)
        {
            Serializer.SaveArray(writer, Normals);
            Serializer.SaveArray(writer, Positions);
            Serializer.SaveArray(writer, TexCoords);
            Serializer.SaveArray(writer, indices);
        }

        /// <summary> loads mesh as binary </summary>
        internal void LoadFromFile(BinaryReader reader)
        {
            Serializer.LoadArray(reader, out Normals);
            Serializer.LoadArray(reader, out Positions);
            Serializer.LoadArray(reader, out TexCoords);
            Serializer.LoadArray(reader, out indices);
            LoadBuffers();// vao vbo... must created after import
        }

        protected void GenerateBoundingBox()
        {
            Vector3 min = new Vector3(float.MaxValue);
            Vector3 max = new Vector3(float.MinValue);

            foreach(var pos in Positions)
            {
                if (pos.X < min.X) min.X = pos.X;
                if (pos.Y < min.Y) min.Y = pos.Y;
                if (pos.Z < min.Z) min.Z = pos.Z;

                if (pos.X > max.X) max.X = pos.X;
                if (pos.Y > max.Y) max.Y = pos.Y;
                if (pos.Z > max.Z) max.Z = pos.Z;
            }
        }

        public MeshBase(in string path)
        {
            AssetManager.meshes.Add(this);
            this.name = Path.GetFileNameWithoutExtension(path);
            this.path = path;
        }

        /// <summary>memory efficent position coppying</summary> 
        public virtual unsafe T[] GetPositions<T>() where T : unmanaged
        {
            T[] exportedPositions = new T[Positions.Length];

            fixed (void* exportPtr = &exportedPositions[0])
            fixed (void* vertexPtr = &Positions[0])
            Unsafe.CopyBlock(exportPtr, vertexPtr, (uint)(Vector3.SizeInBytes * Positions.Length));
            return exportedPositions;
        }

        /// <summary>memory efficent texCoord coppying</summary> 
        public virtual unsafe T[] GetTexCoords<T>() where T : unmanaged
        {
            T[] exportedPositions = new T[TexCoords.Length];

            fixed (void* exportPtr = &exportedPositions[0])
            fixed (void* texCoordPtr = &TexCoords[0])
            Unsafe.CopyBlock(exportPtr, texCoordPtr, (uint)(Vector2.SizeInBytes * TexCoords.Length));
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
