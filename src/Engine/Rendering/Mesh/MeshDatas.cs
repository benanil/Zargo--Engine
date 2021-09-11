using OpenTK.Mathematics;
using Assimp;
using System.Runtime.CompilerServices;

#nullable disable warnings

namespace ZargoEngine.Rendering
{
    using AIMesh = Assimp.Mesh;
    using static EngineConstsants;

    public readonly struct Vertex
    {
        // basic
        public readonly Vector3D Position;
        public readonly Vector2D TexCoord;
        public readonly Vector3D Normal;

        public Vertex(AIMesh aIMesh, in int indice)
        {
            Position = aIMesh.Vertices[indice];
            TexCoord = aIMesh.HasTextureCoords(0) ? new Vector2D(aIMesh.TextureCoordinateChannels[0][indice].X, aIMesh.TextureCoordinateChannels[0][indice].Y) : Zero2;
            Normal   = aIMesh.HasNormals ? aIMesh.Normals[indice] : Zero;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vertex(in Vector3D position, in Vector2D texCoord, in Vector3D normal)
        {
            Position = position;
            TexCoord = texCoord;
            Normal   = normal;
        }
    }

    public struct SkinnedVertex
    {
        // basic
        public readonly Vector3D Position;
        public readonly Vector2D TexCoord;
        public readonly Vector3D Normal;
        public Vector4 weights;
        public Vector4i ids;

        public SkinnedVertex(AIMesh aIMesh, in int indice) 
        {
            Position = aIMesh.Vertices[indice];
            TexCoord = aIMesh.HasTextureCoords(0) ? new Vector2D(aIMesh.TextureCoordinateChannels[0][indice].X, aIMesh.TextureCoordinateChannels[0][indice].Y) : Zero2;
            Normal = aIMesh.HasNormals ? aIMesh.Normals[indice] : Zero;
            weights = new Vector4(); ids = new Vector4i();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SkinnedVertex(in Vector3D position, in Vector2D texCoord, in Vector3D normal)
        {
            Position = position;
            TexCoord = texCoord;
            Normal = normal;
            weights = new Vector4();
            ids = new Vector4i();
        }
    }

}
