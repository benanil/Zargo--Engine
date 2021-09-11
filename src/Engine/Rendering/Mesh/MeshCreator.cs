
using ZargoEngine.Core;

namespace ZargoEngine.Rendering
{
    public class MeshCreator : NativeSingleton<MeshCreator>
    {
        private Mesh cube;
        private Mesh quad;
        private Mesh Sphere;

        /// <summary>
        /// creates a Cube
        /// </summary>
        public static Mesh CreateCube()
        {
            if (instance.cube == null){
                instance.cube = AssetManager.GetMesh("Models/objs/cube.obj");
            }
            return instance.cube;
        }

        /// <summary>
        /// creates a Cube
        /// </summary>
        public static Mesh CreateSphere()
        {
            if (instance.Sphere == null)
            {
                instance.Sphere = instance.cube;//CreateSphereMesh();
            }
            return instance.Sphere;
        }

        /// <summary>
        /// creates a quad
        /// </summary>
        public static Mesh CreateQuad()
        {
            if (instance.quad == null){
                instance.quad = AssetManager.GetMesh("Models/objs/Atilla.obj");
            }

            return instance.quad;
        }

        // cpu efficent way
        private static readonly float[] cubeVertices = {
            -1.0f,  1.0f, -1.0f,
            -1.0f, -1.0f, -1.0f,
             1.0f, -1.0f, -1.0f,
             1.0f, -1.0f, -1.0f,
             1.0f,  1.0f, -1.0f,
            -1.0f,  1.0f, -1.0f,

            -1.0f, -1.0f,  1.0f,
            -1.0f, -1.0f, -1.0f,
            -1.0f,  1.0f, -1.0f,
            -1.0f,  1.0f, -1.0f,
            -1.0f,  1.0f,  1.0f,
            -1.0f, -1.0f,  1.0f,

             1.0f, -1.0f, -1.0f,
             1.0f, -1.0f,  1.0f,
             1.0f,  1.0f,  1.0f,
             1.0f,  1.0f,  1.0f,
             1.0f,  1.0f, -1.0f,
             1.0f, -1.0f, -1.0f,

            -1.0f, -1.0f,  1.0f,
            -1.0f,  1.0f,  1.0f,
             1.0f,  1.0f,  1.0f,
             1.0f,  1.0f,  1.0f,
             1.0f, -1.0f,  1.0f,
            -1.0f, -1.0f,  1.0f,

            -1.0f,  1.0f, -1.0f,
             1.0f,  1.0f, -1.0f,
             1.0f,  1.0f,  1.0f,
             1.0f,  1.0f,  1.0f,
            -1.0f,  1.0f,  1.0f,
            -1.0f,  1.0f, -1.0f,

            -1.0f, -1.0f, -1.0f,
            -1.0f, -1.0f,  1.0f,
             1.0f, -1.0f, -1.0f,
             1.0f, -1.0f, -1.0f,
            -1.0f, -1.0f,  1.0f,
             1.0f, -1.0f,  1.0f
        };

        private readonly struct TriangleIndices
        {
            public readonly int v1;
            public readonly int v2;
            public readonly int v3;

            public TriangleIndices(in int v1,in int v2,in int v3)
            {
                this.v1 = v1;
                this.v2 = v2;
                this.v3 = v3;
            }
        }

       
    }
}
