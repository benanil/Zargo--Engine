// this class manages shaders and materials each material registers here and renders here
// and shadow rendering and managind turns here

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

#nullable disable warnings

namespace ZargoEngine.Rendering {
    using ZargoEngine.Editor;

    public static class Renderer3D
    {
        private static readonly Shader ShadowShader = AssetManager.GetShader("Shaders/Shadow.vert", "Shaders/Depth.frag");
        private static readonly Shader DepthShader = null;// AssetManager.GetShader("DepthVert.glsl","Depth.frag");

        private sealed record ShaderMaterials(in Shader shader, in List<Material> materials);
        private static readonly List<ShaderMaterials> Shaders = new List<ShaderMaterials>();

        internal static void RenderMaterials(RenderHandeller handeller)
        {
            // loop every material in game , 
            foreach (var shaderMatPair in Shaders) // key = shader value = material
            {
                Shader shader = shaderMatPair.shader;

                shader.Use();
                shader.SetDefaults(handeller);
                
                for (short i = 0; i < shaderMatPair.materials.Count; i++)
                {
                    shaderMatPair.materials[i].Render();
                }

                Shader.DetachShader();
            }
        }

        // SHADOWS

        private static int FrameBufferID;
        private static int ShadowTexId;


        /// <summary>
        /// settings for full shadowed bukra scene 
        /// ShadowMapSize 1600; farPlane 1600; ortho size 500; 
        /// </summary>
        private static bool ShadowsNeedsUpdate;
        /// <summary> texture Size </summary>
        private static int ShadowMapSize = 8188;
        private static int OrthoSize = 160;
        private static float NearPlane = 0;
        private static float FarPlane = 1000;

        internal static float Bias = 0.001f;
        internal static Matrix4 lightSpaceMatrix;
        
        /// <summary> draws for imguı </summary>
        internal static void DrawShadowSettings()
        {
            GUI.HeaderIn("Shadow Settings");
            GUI.IntField  (ref ShadowMapSize, nameof(ShadowMapSize), UpdateShadows);
            GUI.IntField  (ref OrthoSize    , nameof(OrthoSize)    , UpdateShadows);
            GUI.FloatField(ref Bias         , nameof(Bias)         , UpdateShadows, 0.0001f);
            GUI.FloatField(ref NearPlane    , nameof(NearPlane)    , UpdateShadows, 1);
            GUI.FloatField(ref FarPlane     , nameof(FarPlane)     , UpdateShadows, 1);
        }
        
        internal static void CalculateShadows()
        {
            if (ShadowsNeedsUpdate)
            {
                CalculateAndPrepare();
                GL.Clear(ClearBufferMask.DepthBufferBit);
                GL.Viewport(0, 0, ShadowMapSize, ShadowMapSize);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBufferID);
                GL.Clear(ClearBufferMask.DepthBufferBit);
                { 
                    RenderToShadowTexture();
                }
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                GL.Viewport(0, 0, Screen.MonitorWidth, Screen.MonitorHeight);
                GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

                ShadowsNeedsUpdate = false;
            }
        }

        private static void RenderToShadowTexture()
        {
            ShadowShader.Use();
            {
                ShadowShader.SetMatrix4Location(16, lightSpaceMatrix);

                foreach (var pair in Shaders)
                {
                    foreach (var material in pair.materials)
                    {
                        material.RenderMeshes(0); // 0 is model matrix location for shadow shader
                    }
                }
            }
            ShadowShader.Detach();
        }

        private static void CalculateAndPrepare()
        {
            // calculation
            Matrix4 view = Matrix4.LookAt(RenderHandeller.instance.GetSunPosition() / 2, // 350 mt far away from 0 altitude
                                          Vector3.Zero, Vector3.UnitY);
            Matrix4 proj = Matrix4.CreateOrthographicOffCenter(-OrthoSize, OrthoSize,
                                                               -OrthoSize, OrthoSize, NearPlane, FarPlane);
            lightSpaceMatrix = view * proj;

            // preperation
            // delete old frame buffer and texture
            GL.DeleteTexture(ShadowTexId);
            GL.DeleteFramebuffer(FrameBufferID);

            // create texture
            ShadowTexId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, ShadowTexId);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent16, ShadowMapSize, ShadowMapSize,
                                                   0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            int[] borderColor = { 1, 1, 1, 1 };
            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, borderColor);

            // create framebuffer texture
            FrameBufferID = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBufferID);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, ShadowTexId, 0);

            GL.ReadBuffer(ReadBufferMode.None);
            GL.DrawBuffer(DrawBufferMode.None);
                                               
            DrawOrthographicView();
        }
                                           
        private static readonly Line[] lines = new Line[] {
            new Line(Vector3.Zero, Vector3.Zero),
            new Line(Vector3.Zero, Vector3.Zero),
            new Line(Vector3.Zero, Vector3.Zero),
            new Line(Vector3.Zero, Vector3.Zero),
            new Line(Vector3.Zero, Vector3.Zero),
            new Line(Vector3.Zero, Vector3.Zero),
            new Line(Vector3.Zero, Vector3.Zero),
            new Line(Vector3.Zero, Vector3.Zero),
        };

        // debug lines for orthographic projection of the lightspace matrix
        internal static void DrawOrthographicView()
        {
            Vector3 camCenter  = RenderHandeller.instance.GetSunPosition() / 2;
            Vector3 camForward = RenderHandeller.instance.GetSunDirection();
            Vector3 camRight = Vector3.Normalize(Vector3.Cross(camForward, Vector3.UnitY));
            Vector3 camUp    = Vector3.Normalize(Vector3.Cross(camRight, camForward));

            Vector3 rightUp   = camCenter + (camRight * OrthoSize) + (camUp * OrthoSize);
            Vector3 leftUp    = camCenter - (camRight * OrthoSize) + (camUp * OrthoSize);
            Vector3 rightDown = camCenter + (camRight * OrthoSize) - (camUp * OrthoSize);
            Vector3 leftDown  = camCenter - (camRight * OrthoSize) - (camUp * OrthoSize);

            lines[0].Invalidate(rightUp  , rightDown);
            lines[1].Invalidate(rightDown, leftDown);
            lines[2].Invalidate(leftDown , leftUp);
            lines[3].Invalidate(leftUp   , rightUp);

            lines[4].Invalidate(rightUp   + (camForward * NearPlane), rightUp   + (camForward * FarPlane));
            lines[5].Invalidate(rightDown + (camForward * NearPlane), rightDown + (camForward * FarPlane));
            lines[6].Invalidate(leftDown  + (camForward * NearPlane), leftDown  + (camForward * FarPlane));
            lines[7].Invalidate(leftUp    + (camForward * NearPlane), leftUp    + (camForward * FarPlane));
        }

        // call this void when new mesh added to the scene or on shadow resolution changed
        // or when an objct moved we are using this because we dont want to create shadowmap everytime
        public static void UpdateShadows() => ShadowsNeedsUpdate = true;
        public static int GetShadowTexture() => ShadowTexId;
        
        internal static void DebugMatrix()
        {
            // temp
            Debug.LogWarning("lightspace: " + lightSpaceMatrix);
        }

        /// <summary>
        /// renders depth texture relative to frame buffer
        /// this method can be usefull for shadows and depth effects
        /// </summary>
        internal static void RenderDepthTexture(in Matrix4 viewMatrix, in Matrix4 projectionMatrix, in int frameBuffer)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer);
            {
                DepthShader.Use();
                DepthShader.SetMatrix4Location(28, viewMatrix * projectionMatrix);
                foreach (var shaderMaterial in Shaders)
                {
                    foreach (var material in shaderMaterial.materials)
                    {
                        material.RenderMeshes();
                    }
                }
                DepthShader.Detach();
            }
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }


        // Registering and Managing
        internal static void AssignMaterial(Material material)
        {
            ShaderMaterials containingPair = default;

            Shaders.Find(shaderPair => shaderPair.shader == material.shader);

            if (containingPair != null)
            {
                if (!containingPair.materials.Contains(material)) // blocak assigning same material mltiple times
                {
                    containingPair.materials.Add(material);
                }
            }
            else
            {
                Shaders.Add(new ShaderMaterials(material.shader, new List<Material>() { material }));
            }
        }

        internal static void RemoveMaterial(Material material)
        {
            ShaderMaterials containingPair = Shaders.Find(shaderPair => shaderPair.shader == material.shader);

            if (containingPair != null)
            {
                containingPair.materials.Remove(material);
            }
        }

        internal static void OnShaderChanged(Material material, Shader beforeShader, Shader afterShader)
        {
            if (beforeShader != afterShader)
            {
                Shaders.Find(pair => pair.shader == beforeShader).materials.Remove(material);
                material.ChangeShader(afterShader);
                AssignMaterial(material);
            }
        }
    } 
}