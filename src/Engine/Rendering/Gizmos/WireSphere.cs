using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using ZargoEngine.Helper;
using ZargoEngine.Rendering;

namespace ZargoEngine
{
    public unsafe class WireSphere : GizmoBase, IDisposable
    {
        public float radius;
        public float lineSize;

        public readonly List<Vector3> vertecies = new List<Vector3>();

        public float SampleRate => radius * 54;

        public WireSphere(in float radius, Transform go) 
        {
            Gizmos.Register(this);
            this.transform = go.transform; this.radius = radius;
            GenerateBuffers();
        }

        public WireSphere(in float radius, Vector3 position,Quaternion rotation = new() , Vector3 scale = new())
        {
            Gizmos.Register(this);
            this.radius = radius;
            ModelMatrix = Extensions.TRS(position, rotation, scale);
            GenerateBuffers();
        }

        private void GenerateBuffers()
        {
            var vertical = new List<Vector3>();

            for (int i = 0; i < SampleRate; i++)
            {
                float Pi   = MathHelper.TwoPi * (SampleRate / i);
                float sinP = MathF.Sin(Pi) * radius;
                float cosP = MathF.Cos(Pi) * radius;

                // horizontal
                vertecies.Add(new Vector3(sinP, 0, cosP));
                // vertical
                vertical.Add(new Vector3(sinP,cosP, 0));
            }

            vertecies.AddRange(vertical);

            vaoID = GL.GenVertexArray();
            GL.BindVertexArray(vaoID);

            vboID = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, vboID);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 3 * vertecies.Count, vertecies.ToArray(), BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0);
        } 

        public override void Render()
        {
            // binds View matrix projection matrix and color to the shader
            GizmoShader.Use();
            GizmoShader.SetMatrix4("view", Camera.main.GetViewMatrix(), false);
            GizmoShader.SetMatrix4("projection", Camera.main.GetGetProjectionMatrix(), false);
            Shader.SetVector4Sys(GizmoShader.GetUniformLocation("color"), color);

            GizmoShader.SetMatrix4("model", ref GetModelMatrix(), true);

            GL.BindVertexArray(vaoID);

            GL.EnableVertexAttribArray(0);

            GL.PointSize(lineSize);
            // GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
            // GL.Enable(EnableCap.LineSmooth);

            GL.DrawArrays(PrimitiveType.LineLoop, 0, vertecies.Count);

            GL.Disable(EnableCap.LineSmooth);
            GL.LineWidth(1);

            GL.DisableVertexAttribArray(0);
            GL.BindVertexArray(0);

            Shader.DetachShader();
        }

        public void DeleteBuffers()
        {
            GL.DeleteVertexArray(vaoID);
            GL.DeleteBuffer(vboID);
        }

        public void Dispose()
        {
            DeleteBuffers();
            GC.SuppressFinalize(this);
        }

    }
}
