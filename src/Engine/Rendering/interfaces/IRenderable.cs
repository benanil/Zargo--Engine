
namespace ZargoEngine.Rendering
{
    // renderable things uses this interface
    public interface IRenderable
    {
        public void Render();
        public void DeleteBuffers();
    }
}
