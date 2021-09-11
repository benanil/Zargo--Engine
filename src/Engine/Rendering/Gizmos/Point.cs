using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

namespace ZargoEngine.Rendering
{
    public struct Point : IDisposable, IRenderable
    {
        public static readonly Shader PointShader;

        /// <summary>if you are settings this please call set position instead</summary>
        public Vector3 position;
        public float scale; // lineWidth
        public System.Numerics.Vector4 color;

        private int vaoID, vboID;
        public int texture;

        static Point() {
            PointShader = AssetManager.GetShader("Shaders/PointVert.glsl", "Shaders/PointFrag.glsl");
        }

        public Point(Vector3 position, System.Numerics.Vector4 color = default, Texture texture = null)
        {
            scale = 2f;
            this.position = position;
            this.color = color;
            this.texture = 0;

            if (texture != null) {
                this.texture = texture.texID;
            }

            if (color != default) this.color = color; // we want to set it only exist
            
            GenerateVaoVbo(ref position, out vaoID, out vboID);
            
            Gizmos.Register(this);
        }

        public Point(Transform transform, System.Numerics.Vector4 color = default, Texture texture = null) : this(transform.position, color, texture)
        {
            transform.OnTransformChanged += Transform_OnTransformChanged;
        }

        private void Transform_OnTransformChanged([System.Runtime.InteropServices.In] ref Matrix4 transform)
        {
            position = new Vector3(transform.Row0.W, transform.Row1.W, transform.Row2.W);
            DeleteBuffers();
            GenerateVaoVbo(ref position, out vaoID, out vboID);
        }

        public void SetPosition(in Vector3 position)
        {
            this.position = position;
            DeleteBuffers();
            GenerateVaoVbo(ref this.position, out vaoID, out vboID);
        }

        public void Render()
        {
            // binds View matrix projection matrix and color to the shader
            PointShader.Use();
            PointShader.SetMatrix4Location(PointShader.viewProjectionLoc, Camera.main.GetViewMatrix() * Camera.main.GetGetProjectionMatrix(), true);

            Shader.SetVector4Sys(PointShader.GetUniformLocation("color"), color);
            PointShader.SetMatrix4("model", Matrix4.CreateTranslation(position), true);

            GL.Enable(EnableCap.PointSprite); // for activating texture on point
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texture);

            GL.BindVertexArray(vaoID);

            GL.EnableVertexAttribArray(0);

            GL.PointSize(scale);

            GL.DrawArrays(PrimitiveType.Points, 0, 1);

            GL.Disable(EnableCap.LineSmooth);
            GL.LineWidth(1);

            GL.DisableVertexAttribArray(0);
            GL.BindVertexArray(0);

            Texture.UnBind();
            Shader.DetachShader();
        }

        private static void GenerateVaoVbo(ref Vector3 position, out int vaoID, out int vboID)
        {
            vaoID = GL.GenVertexArray();
            GL.BindVertexArray(vaoID);

            vboID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboID);

            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 3, ref position, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, IntPtr.Zero);
            GL.EnableVertexAttribArray(0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0); // dispose
            GL.BindVertexArray(0);
        }

        public void DeleteBuffers()
        {
            GL.DeleteVertexArray(vaoID);
            GL.DeleteBuffer(vboID);
        }

        public void Dispose()
        {
            Gizmos.Remove(this);
            DeleteBuffers();
            GC.SuppressFinalize(this);
        }
    }
}