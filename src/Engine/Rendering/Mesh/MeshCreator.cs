
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
                instance.cube = AssetManager.GetMesh("Models/objs/cube.obj", "cube");
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
                instance.quad = CreateCube();//;AssetManager.GetMesh("Models/objs/Atilla.obj");
            }

            return instance.quad;
        }

    }
}
