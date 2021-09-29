
using OpenTK.Mathematics;

namespace ZargoEngine.Rendering
{
    public interface ICamera
    {
        ref Matrix4 GetProjectionMatrix();
        ref Matrix4 GetViewMatrix();
        ref Vector3 GetPosition();
        Vector3 GetForward();
        Vector3 GetRight();
        Vector3 GetUp();

    }
}
