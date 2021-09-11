using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using ZargoEngine.Helper;
using ZargoEngine.Rendering;

namespace ZargoEngine
{
    public class PointMesh : GizmoBase
    {
        public Vector3[] points;

        public PointMesh(in Vector3[] points, Transform transform) 
        {
            Gizmos.Register(this);
            this.transform = transform;
            this.points = points;
        }

        public PointMesh(in Vector3[] points, Vector3 position,Quaternion rotation = new (), Vector3 scale = new ())
        {
            Gizmos.Register(this);
            this.ModelMatrix = Extensions.TRS(position, rotation, scale);
            this.points = points;
        }

        public void Invalidate()
        {
            vaoID = GL.GenVertexArray();
            GL.BindVertexArray(vaoID);

            vboID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer,vboID);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 3 * points.Length, points, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, IntPtr.Zero);
        }

        public override void Render()
        {
            // binds View matrix projection matrix and color to the shader
            base.Render();
            GizmoShader.SetMatrix4("model", ref GetModelMatrix(), true);
            GL.BindVertexArray(vaoID);

            GL.EnableVertexAttribArray(0);

            GL.PointSize(LineWidth);
            // GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
            // GL.Enable(EnableCap.LineSmooth);

            GL.DrawArrays(PrimitiveType.Points, 0, points.Length);

            GL.DisableVertexAttribArray(0);
            GL.BindVertexArray(0);

            Shader.DetachShader();
        }
    }
}
