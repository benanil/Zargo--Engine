using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using ImGuiNET;

namespace ZargoEngine.Rendering { 
    using ZargoEngine.Editor;

    // https://learnopengl.com/Advanced-Lighting/SSAO
    public static class SSAO
    {
        private static readonly Shader ShaderSSAO = AssetManager.GetShader("Shaders/Misc/SSAO.vert", "Shaders/Misc/SSAO.glsl");
        private static readonly Shader ShaderSSAO_Blur = AssetManager.GetShader("Shaders/Misc/SSAO.vert", "Shaders/Misc/SSAO_Blur.glsl");
        private static readonly Shader ShaderSSAO_Geometry = AssetManager.GetShader("Shaders/Misc/SSAO_Geometry.vert", "Shaders/Misc/SSAO_Geometry.glsl");

        private static readonly int noiseTexture;

        private static readonly Vector3[] karnel = new Vector3[64];

        private static int rboDepth;
        private static int gBuffer;
        private static int gPosition, gNormal;
        
        static SSAO()
        {
            ShaderSSAO_Blur.Use();
            ShaderSSAO_Blur.SetInt("ssaoInput", 0);
            
            Random random = new Random();

            // generating karnel
            for (int i = 0; i < karnel.Length; i++)
            {
                karnel[i] = new Vector3((float)random.NextDouble() * 2.0f - 1.0f,
                                        (float)random.NextDouble() * 2.0f - 1.0f, (float)random.NextDouble());
                float scale = i / 64.0f;
                karnel[i] *= MathHelper.Lerp(0.1f, 1.0f, scale * scale);
            }

            ShaderSSAO.Use();
            ShaderSSAO.SetInt("gPosition", 0);
            ShaderSSAO.SetInt("gNormal", 1);
            ShaderSSAO.SetInt("texNoise", 2);

            for (byte i = 0; i < karnel.Length; i++) {
                ShaderSSAO.SetVector3($"samples[{i}]", karnel[i]);
            }

            Shader.DetachShader();
            
            // noise texture
            Vector3[] noise = new Vector3[16];
            for (int i = 0; i < 16; i++)
            {
                noise[i] = new Vector3((float)random.NextDouble(),
                                       (float)random.NextDouble(), 0);
            }
            
            noiseTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, noiseTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb16f, 4, 4, 0, PixelFormat.Rgb, PixelType.Float, ref noise[0]);
            
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            
            GenerateGbufferFramebuffers();
            
            // new TempraryWindow("SSAO debug", () =>
            // {
            //     GUI.Image((IntPtr)ResultTexture, new System.Numerics.Vector2(600, 700));
            //     return false;       
            // });
        }

        private static void GenerateGbufferFramebuffers()
        {
            // for exporting screen space normal and positions
            gBuffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, gBuffer);

            gPosition = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, gPosition);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, Screen.MonitorWidth, Screen.MonitorHeight,
                                                   0, PixelFormat.Rgb, PixelType.Float, IntPtr.Zero);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, gPosition, 0);

            gNormal = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, gNormal);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, Screen.MonitorWidth, Screen.MonitorHeight,
                                                   0, PixelFormat.Rgb, PixelType.Float, IntPtr.Zero);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2D, gNormal, 0);

            DrawBuffersEnum drawBuffers = DrawBuffersEnum.ColorAttachment0 | DrawBuffersEnum.ColorAttachment1;
            GL.DrawBuffers(2, ref drawBuffers);

            // create depth attachment
            rboDepth = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, rboDepth);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, Screen.MonitorWidth, Screen.MonitorHeight);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, rboDepth);

            GenerateSSAO_Texture();
        }

        private static int SSAO_FBO, SSAO_BlurFBO,
                           SSAO_TEX, SSAO_Blur_TEX;

        /// <summary>creates ssao texture, ssao blur texture and their framebuffers</summary>
        private static void GenerateSSAO_Texture()
        {
            // generation of ssao fbo
            SSAO_FBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, SSAO_FBO);

            SSAO_TEX = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, SSAO_TEX);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8, Screen.MonitorWidth, Screen.MonitorHeight, 
                                                   0, PixelFormat.Red, PixelType.Float, IntPtr.Zero);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, SSAO_TEX, 0);
            
            // generation of blur fbo
            SSAO_BlurFBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, SSAO_BlurFBO);

            SSAO_Blur_TEX = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, SSAO_Blur_TEX);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8, Screen.MonitorWidth, Screen.MonitorHeight,
                                                   0, PixelFormat.Red, PixelType.Float, IntPtr.Zero);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, SSAO_Blur_TEX, 0);
        }

        internal static void CalculateSSAO(out int ssaoTex)
        {
            // geometry pass for exporting screenspace normal and position
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, gBuffer);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            ShaderSSAO_Geometry.Use();
            {
                ShaderSSAO_Geometry.SetMatrix4Location(16, ref Camera.main.ViewMatrix);
                ShaderSSAO_Geometry.SetMatrix4Location(32, ref Camera.main.projectionMatrix);
                
                foreach (var pair in Renderer3D.Shaders)
                {
                    foreach (var material in pair.materials)
                    {
                        material.RenderMeshes(0); // is model matrix location of the shader
                    }
                }
            }
            ShaderSSAO_Geometry.Detach();

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, SSAO_FBO);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            GL.Disable(EnableCap.Blend);
            ShaderSSAO.Use();
            {
                GL.Disable(EnableCap.DepthTest);
                GL.BindVertexArray(Renderer3D.screenVao);
                GL.EnableVertexAttribArray(0);

                ShaderSSAO.SetVector2("screenSize", new Vector2(Screen.MonitorWidth, Screen.MonitorHeight));

                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, gPosition);
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2D, gNormal);
                GL.ActiveTexture(TextureUnit.Texture2);
                GL.BindTexture(TextureTarget.Texture2D, noiseTexture);

                GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
                GL.DisableVertexAttribArray(0);
            }
            ShaderSSAO.Detach();

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, SSAO_BlurFBO);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            ShaderSSAO_Blur.Use();
            {
                GL.Disable(EnableCap.DepthTest);
                GL.BindVertexArray(Renderer3D.screenVao);
                GL.EnableVertexAttribArray(0);

                GL.BindTexture(TextureTarget.Texture2D, SSAO_TEX);
                GL.ActiveTexture(TextureUnit.Texture0);

                GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
                GL.DisableVertexAttribArray(0);
            }
            ShaderSSAO_Blur.Detach();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);

            ssaoTex = SSAO_Blur_TEX;
        }
    }
}
