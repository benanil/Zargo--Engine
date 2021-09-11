using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using System;
using ZargoEngine.Rendering;


// Todo render with projection matrix and model matrix cause of scaling or something
// and we need to fix scaling problems diffrent window sizes
namespace ZargoEngine.UI
{
    public class SpriteRenderer : UIbase
    {
        static readonly Shader shader;
        
        public Texture texture;
    
        private static readonly float[] vertices = 
        {
            1,1,
            1,0,
            0,0,
            1,1,
            0,0,
            0,1
        };
    
        private static readonly int vaoID, vboID, uvID, eboID;
    
        #region Constructor Deconstructors
        ~SpriteRenderer() {
            GL.DeleteVertexArray(vaoID);
            GL.DeleteBuffer(vboID);
            GL.DeleteBuffer(eboID);
            GL.DeleteBuffer(uvID);
        }

        static SpriteRenderer()
        {
            shader = AssetManager.GetShader("Shaders/Sprite.vert", "Shaders/Sprite.frag");

            const short unitSize = sizeof(float) * 2; // vec2's size
            // generate buffers and vao
            vaoID = GL.GenVertexArray();
            GL.BindVertexArray(vaoID);

            vboID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboID);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, unitSize, IntPtr.Zero);

            uvID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, uvID);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, unitSize, IntPtr.Zero);

            // unbind
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        public SpriteRenderer(GameObject go) : base(go)  {
            texture = AssetManager.DefaultTexture;
            texture.SetAsUI();
        }
        
        public SpriteRenderer(GameObject go, string path) : base(go, path) {
            texture = AssetManager.GetTexture(path);
            texture.SetAsUI();
        }
        #endregion
    
        protected override void CalculateBounds()
        {
            bounds.Min = WindowBottom + transform.position.Xy;
            bounds.Max = WindowBottom + transform.position.Xy + transform.scale.Xy;
        }
    
        public override void DrawWindow()
        {
            base.DrawWindow();
            ImGui.Separator();
        }
    
        protected override void RenderHUD()
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            GL.DepthMask(false);
    
            shader.Use();
            shader.SetInt("texture0", 0);
            shader.SetVector4Sys("color", color);
            shader.SetVector2(nameof(ScreenScale), ScreenScale);
            shader.SetVector2("position", transform.position.Xy);
    
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texture.texID);
            GL.BindVertexArray(vaoID);
            //GL.BindBuffer(BufferTarget.ElementArrayBuffer, eboID);
    
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
    
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
    
            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);
            GL.BindVertexArray(0);
            Shader.DetachShader();
    
            GL.Disable(EnableCap.CullFace);
            GL.DepthMask(true);
            GL.Enable(EnableCap.DepthTest);
        }
    }
}
