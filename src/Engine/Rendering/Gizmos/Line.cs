using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

namespace ZargoEngine.Rendering
{
    // todo add textures
    public struct Line : IDisposable, IRenderable
    {
        public struct LinePoints
        { 
            public Vector3 startPoint;
            public Vector3 endPoint ;

            public LinePoints(in Vector3 startPoint,in Vector3 endPoint)
            {
                this.startPoint = startPoint;
                this.endPoint = endPoint;
            }
        }

        public float LineWidth; // lineWidth
        public System.Numerics.Vector4 color;
        private Matrix4 ModelMatrix;

        private int vaoID, vboID;

        private LinePoints points;

        public Line(in Vector3 startPoint,in Vector3 endPoint, System.Numerics.Vector4 color = default)
        {
            ModelMatrix = Matrix4.Identity;
            LineWidth = 2f;

            points = new LinePoints(startPoint, endPoint);

            this.color = color;

            if (color == default) this.color = new System.Numerics.Vector4(.9f, .7f, .2f, 1);

            GenerateVaoVbo(ref points, out vaoID, out vboID);

            Invalidate(startPoint, endPoint);
            
            Gizmos.Register(this);
        }

        public Line(Transform transform, in Vector3 startPoint, in Vector3 endPoint, System.Numerics.Vector4 color = default)
        {
            ModelMatrix = transform.Translation;
            LineWidth = 2f;

            points = new LinePoints(startPoint, endPoint);

            this.color = color;

            if (color == default) this.color = new System.Numerics.Vector4(.9f, .7f, .2f, 1);

            GenerateVaoVbo(ref points, out vaoID, out vboID);

            Invalidate(startPoint, endPoint);

            transform.OnTransformChanged += Transform_OnTransformChanged;

            Gizmos.Register(this);
        }

        private void Transform_OnTransformChanged([System.Runtime.InteropServices.In] ref Matrix4 transform)
        {
            ModelMatrix = transform;
        }

        public void Invalidate(in Vector3 startPoint, in Vector3 endPoint)
        {
            DeleteBuffers();
            points.startPoint = startPoint; points.endPoint = endPoint;

            GenerateVaoVbo();
        }

        public void Render()
        {
            // binds View matrix projection matrix and color to the shader
            GizmoBase.GizmoShader.Use();
            GizmoBase.GizmoShader.SetMatrix4Location(GizmoBase.GizmoShader.viewLoc, Camera.main.GetViewMatrix());
            GizmoBase.GizmoShader.SetMatrix4Location(GizmoBase.GizmoShader.projectionLoc, Camera.main.GetGetProjectionMatrix());
            Shader.SetVector4Sys(GizmoBase.GizmoShader.GetUniformLocation("color"), color);

            GizmoBase.GizmoShader.SetMatrix4Location(GizmoBase.GizmoShader.ModelMatrixLoc, ModelMatrix, true);

            GL.BindVertexArray(vaoID);

            GL.EnableVertexAttribArray(0);

            GL.LineWidth(LineWidth);

            GL.DrawArrays(PrimitiveType.Lines, 0, 2);

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

        private const byte PointsSizeInBytes = sizeof(float) * 6;

        private static void GenerateVaoVbo(ref LinePoints linePoints, out int vaoID, out int vboID)
        {
            vaoID = GL.GenVertexArray();
            GL.BindVertexArray(vaoID);

            vboID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboID);

            unsafe { 
                fixed (void* ptr = &linePoints)
                    GL.BufferData(BufferTarget.ArrayBuffer, PointsSizeInBytes, (IntPtr)ptr, BufferUsageHint.StaticDraw);
            }
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vector3.SizeInBytes, IntPtr.Zero);
            GL.EnableVertexAttribArray(0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0); // dispose
            GL.BindVertexArray(0);
        }

        private void GenerateVaoVbo()
        {
            vaoID = GL.GenVertexArray();
            GL.BindVertexArray(vaoID);

            vboID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboID);
            unsafe { 
                fixed (void* ptr = &points)
                GL.BufferData(BufferTarget.ArrayBuffer, PointsSizeInBytes, (IntPtr)ptr, BufferUsageHint.StaticDraw);
            }
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vector3.SizeInBytes, IntPtr.Zero);
            GL.EnableVertexAttribArray(0);

            GL.BindBuffer(BufferTarget.ArrayBuffer,0); // dispose
            GL.BindVertexArray(0);
        }

        public void Dispose()
        {
            Gizmos.Remove(this);
            DeleteBuffers();
            GC.SuppressFinalize(this);
        }
    }
}
