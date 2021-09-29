
namespace ZargoEngine.Rendering
{
    using Helper;
    using OpenTK.Graphics.OpenGL4;

    public class SkinnedMeshRenderer : MeshRenderer
    {
        private readonly Animator animator;

        public SkinnedMeshRenderer(SkinnedMesh mesh, GameObject gameObject, Animator animator, Material material) : base(mesh, gameObject, material) 
        {
            this.animator = animator;
        }

        public override void Render()
        {
            for (ushort i = 0; i < animator.boneMatrices.Length; i++)
            {
                GL.ProgramUniformMatrix4(Material.shader.program, 44 + i, true, ref animator.boneMatrices[i].toTKRef());
            }
        }
    }
}
