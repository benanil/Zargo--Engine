

using OpenTK.Mathematics;
using System.Collections.Generic;
using ZargoEngine.Rendering;

namespace ZargoEngine
{
    public static unsafe partial class Gizmos
    {
        // todo add custom mesh debuging
        // todo add sphere mesh debugging

        private readonly static List<IRenderable> gizmoBases = new List<IRenderable>();

        public static void Render()
        {
            for (int i = 0; i < gizmoBases.Count; i++)
            {
                gizmoBases[i].Render();
            }
        }

        public static IRenderable Register(IRenderable gizmoBase)
        {
            gizmoBases.Add(gizmoBase);
            return gizmoBase;
        }

        public static void Remove(IRenderable gizmoBase)
        {
            gizmoBases.Remove(gizmoBase);
        }

        /// <summary> </summary>
        /// <param name="lines">please create 8 line for this important for memory allocation</param>
        public static void DrawOrthographicView(
            in Line[] lines, in Vector3 centerPos, 
            in Vector3 cameraFront, in Vector2 orthoSize, 
            in int NearPlane, in int FarPlane)
        {
            Vector3 cameraRight = Vector3.Normalize(Vector3.Cross(cameraFront, Vector3.UnitY));
            Vector3 cameraUp = Vector3.Normalize(Vector3.Cross(cameraRight, cameraFront));

            Vector3 rightUp   = centerPos +  cameraUp * orthoSize.X +  cameraRight * orthoSize.Y;
            Vector3 leftUp    = centerPos +  cameraUp * orthoSize.X + -cameraRight * orthoSize.Y;
            Vector3 rightDown = centerPos + -cameraUp * orthoSize.X +  cameraRight * orthoSize.Y;
            Vector3 leftDown  = centerPos + -cameraUp * orthoSize.X + -cameraRight * orthoSize.Y;

            lines[0].Invalidate(leftUp, rightUp);
            lines[1].Invalidate(rightUp, rightDown);
            lines[2].Invalidate(rightDown, leftDown);
            lines[3].Invalidate(leftDown, leftUp);

            // second create lines through view
            lines[4].Invalidate(rightUp   + cameraFront * NearPlane, cameraFront * FarPlane);
            lines[5].Invalidate(leftUp    + cameraFront * NearPlane, cameraFront * FarPlane);
            lines[6].Invalidate(rightDown + cameraFront * NearPlane, cameraFront * FarPlane);
            lines[7].Invalidate(leftDown  + cameraFront * NearPlane, cameraFront * FarPlane);
        }

    }
}
