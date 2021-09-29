using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

namespace ZargoEngine.Rendering
{
    public abstract class GizmoBase : IDisposable, IRenderable
    {
        public static readonly Shader GizmoShader = AssetManager.GetShader("Shaders/GizmoVert.glsl",
                                                                           "Shaders/GizmoFrag.glsl");

        public float LineWidth = 2f; // lineWidth
        public System.Numerics.Vector4 color = new System.Numerics.Vector4(.9f, .7f, .2f, 1);

        protected int vaoID, vboID;

        protected Matrix4 ModelMatrix;

        public Transform transform;

        public virtual void Render()
        {
            GizmoShader.Use();
            GizmoShader.SetMatrix4Location(GizmoShader.viewLoc, Camera.main.GetViewMatrix());
            GizmoShader.SetMatrix4Location(GizmoShader.projectionLoc, Camera.main.GetGetProjectionMatrix(), true);
            Shader.SetVector4Sys(GizmoShader.GetUniformLocation("color"), color);
        }

        public virtual ref Matrix4 GetModelMatrix()
        {
            if (transform != null) return ref transform.Translation;
            return ref ModelMatrix;
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
