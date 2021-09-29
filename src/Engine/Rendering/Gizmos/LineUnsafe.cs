using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace ZargoEngine.Rendering
{
    // now its safe
    public unsafe class LineUnsafe : IRenderable
    {
        public Vector3 startPoint;
        public Vector3 endPoint;

        readonly float[] vertecies = new float[6];
        System.Numerics.Vector4 color = new System.Numerics.Vector4(.9f, .7f, .2f, 1);
        public float LineWidth;

        public int vboID, vaoID;
       
        public LineUnsafe()
        {
            Gizmos.Register(this);

            vaoID = GL.GenVertexArray();
            GL.BindVertexArray(vaoID);

            vboID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboID);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 6, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, IntPtr.Zero);
            GL.EnableVertexAttribArray(0);
        }

        public void Render()
        {
            GizmoBase.GizmoShader.Use();
            // binds View matrix projection matrix and color to the shader
            const string model = "model";
            GizmoBase.GizmoShader.SetMatrix4(model, Matrix4.Identity, true);
            Matrix4 viewProj = Camera.main.GetViewMatrix() * Camera.main.GetProjectionMatrix();
            GL.UniformMatrix4(GizmoBase.GizmoShader.viewProjectionLoc, true, ref viewProj);
            Shader.SetVector4Sys(GizmoBase.GizmoShader.GetUniformLocation("color"), color);


            GL.BindVertexArray(vaoID);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboID);
            
            GL.LineWidth(LineWidth);
            
            vertecies[0] = startPoint.X;   vertecies[1] = startPoint.Y;   vertecies[2] = startPoint.Z;
            vertecies[3] = endPoint  .X;   vertecies[4] = endPoint  .Y;   vertecies[5] = endPoint  .Z;

            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, sizeof(float) * 6, vertecies);

            GL.DrawArrays(PrimitiveType.Lines, 0, 2);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0); // dispose
            GL.Disable(EnableCap.LineSmooth);
            GL.LineWidth(1);

            Shader.DetachShader();
            GL.DeleteBuffer(vboID);
            GL.BindVertexArray(0);
        }

        public void DeleteBuffers()
        {
            GL.DeleteBuffer(vaoID);
            GL.DeleteVertexArray(vaoID);
        }
    }
}
