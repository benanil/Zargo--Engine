using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Runtime.InteropServices;
using ZargoEngine.Helper;

namespace ZargoEngine.Rendering
{
    public class WireCube : GizmoBase
    {
        private static new readonly int vaoID, vboID;

        // kind of working

        private static readonly float[] vertices = new float[]
        {
            -1f, -1f, -1f, 
            -1f, -1f, 1f,  
            
            -1f, -1f, -1f,
            -1f,  1f, -1f,
            
            -1f, -1f, -1f,
             1f, -1f,  1f,
           //
             1f,  1f,  1f,
             1f,  1f, -1f,
          
             1f,  1f,  1f,
             1f, -1f,  1f,

             1f,  1f,  1f,
            -1f,  1f,  1f,
          //
            -1f,  1f, -1f,
            -1f, -1f, -1f,

            -1f,  1f, -1f,
            -1f,  1f,  1f,

            -1f,  1f, -1f,
             1f,  1f, -1f,
           //
             1f, -1f,  1f,
             1f, -1f, -1f,

             1f, -1f,  1f,
            -1f, -1f,  1f,

             1f, -1f,  1f,
             1f,  1f,  1f,
        };

        static WireCube()
        {
            vaoID = GL.GenVertexArray();
            GL.BindVertexArray(vaoID);

            vboID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboID);
            GL.BufferData(BufferTarget.ArrayBuffer, Marshal.SizeOf<Vector3>() * 24, vertices, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0,3,VertexAttribPointerType.Float,false, Marshal.SizeOf<Vector3>(), IntPtr.Zero);
        }

        public WireCube(Matrix4 trs) 
        {
            Gizmos.Register(this);
            this.ModelMatrix = trs;
        }

        public WireCube(Vector3 position, Vector3 scale, Quaternion rotation = default, System.Numerics.Vector4 color = default)
        {
            this.color = color == default ? this.color : color;
            Gizmos.Register(this);
            ModelMatrix = Extensions.TRS(position, rotation, scale);
        }

        public WireCube(Transform transform)
        {
            Gizmos.Register(this);
            this.transform = transform;
        }

        public override void Render()
        {
            // binds View matrix projection matrix and color to the shader
            base.Render();
            GizmoShader.SetMatrix4("model",ref GetModelMatrix(), true);
            GL.BindVertexArray(vaoID);

            GL.EnableVertexAttribArray(0);

            GL.LineWidth(LineWidth);
            // GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
            // GL.Enable(EnableCap.LineSmooth);

            GL.DrawArrays(PrimitiveType.LineLoop, 0, 12);

            GL.DisableVertexAttribArray(0);
            GL.BindVertexArray(0);

            Shader.DetachShader();
        }
    }
}
