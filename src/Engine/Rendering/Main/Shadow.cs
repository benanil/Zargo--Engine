
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

#nullable disable warnings

namespace ZargoEngine.Rendering {
    using ZargoEngine.Editor;

    public static class Shadow
    {
        static Shadow() {
            Settings = ShadowSettings.Default;
            showDebug = Engine.IsEditor;
        }

        private static readonly Shader ShadowShader = AssetManager.GetShader("Shaders/Shadow.vert", "Shaders/Depth.frag");

        private static int FrameBufferID;
        private static int ShadowTexId;

        /// <summary>
        /// settings for full shadowed bukra scene 
        /// ShadowMapSize 1600; farPlane 1600; ortho size 500; 
        /// </summary>
        private static bool ShadowsNeedsUpdate;
        private static bool showDebug;

        internal static ShadowSettings Settings;
        internal static Matrix4 lightSpaceMatrix;

        /// <summary> draws for imguı </summary>
        internal static void DrawShadowSettings()
        {
            GUI.HeaderIn("Shadow Settings");
            GUI.IntField(ref Settings.ShadowMapSize, "ShadowMap Size", UpdateShadows);
            GUI.IntField(ref Settings.OrthoSize, "Ortho Size", UpdateShadows);
            GUI.Vector3Field(ref Settings.OrthoOffset, "Ortho Offset", UpdateShadows, 1);
            GUI.FloatField(ref Settings.Bias, "Bias", UpdateShadows, 0.0001f);
            GUI.FloatField(ref Settings.NearPlane, "Near Plane", UpdateShadows, 1);
            GUI.FloatField(ref Settings.FarPlane, "Far Plane", UpdateShadows, 1);
            GUI.OnOfField(ref showDebug, nameof(showDebug), onSellect: DebugModeChanged);
        }

        private static void DebugModeChanged()
        {
            for (int i = 0; i < ShadowSettings.lines.Length; i++)
            {
                ShadowSettings.lines[i].Enabled = showDebug;
            }
        }

        internal static void Set(ShadowSettings settings)
        {
            Settings = settings;
            UpdateShadows();
        }

        internal static void CalculateShadows()
        {
            if (ShadowsNeedsUpdate)
            {
                CalculateAndPrepare();
                GL.Clear(ClearBufferMask.DepthBufferBit);
                GL.Viewport(0, 0, Settings.ShadowMapSize, Settings.ShadowMapSize);
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

                foreach (var pair in Renderer3D.Shaders)
                {
                    foreach (var material in pair.materials)
                    {
                        material.RenderMeshes(0, true); // 0 is model matrix location for shadow shader
                    }
                }
            }
            ShadowShader.Detach();
        }

        private static void CalculateAndPrepare()
        {
            // calculation
            Matrix4 view = Matrix4.LookAt(RenderConfig.GetSunPosition() / 2 + Settings.OrthoOffset, // 350 mt far away from 0 altitude
                                          Vector3.Zero, Vector3.UnitY);
            Matrix4 proj = Settings.GetOrthoMatrix();
            lightSpaceMatrix = view * proj;

            // delete old ones
            GL.DeleteTexture(ShadowTexId);
            GL.DeleteFramebuffer(FrameBufferID);

            // create texture
            ShadowTexId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, ShadowTexId);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent16, Settings.ShadowMapSize, Settings.ShadowMapSize,
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

            Settings.DrawOrthographicView();
        }

        // call this void when new mesh added to the scene or on shadow resolution changed
        // or when an object moved we are using this because we don't want to create shadowmap everytime
        public static void UpdateShadows() => ShadowsNeedsUpdate = true;
        public static int GetShadowTexture() => ShadowTexId;
    }

    [Serializable]
    public struct ShadowSettings
    {
        public static readonly ShadowSettings Default = new ShadowSettings()
        {
            ShadowMapSize = 1 << 13, // 8k
            OrthoSize = 500,
            FarPlane = 1000,
            Bias = 0.001f,
            OrthoOffset = Vector3.Zero
        };

        /// <summary> texture Size </summary>
        public int ShadowMapSize; 
        public int OrthoSize;
        public float NearPlane;
        public float FarPlane;

        public float Bias;
        public Vector3 OrthoOffset;

        internal static readonly Line[] lines = new Line[] {
            new Line(Vector3.Zero, Vector3.Zero),
            new Line(Vector3.Zero, Vector3.Zero),
            new Line(Vector3.Zero, Vector3.Zero),
            new Line(Vector3.Zero, Vector3.Zero),
            new Line(Vector3.Zero, Vector3.Zero),
            new Line(Vector3.Zero, Vector3.Zero),
            new Line(Vector3.Zero, Vector3.Zero),
            new Line(Vector3.Zero, Vector3.Zero),
        };

        internal Matrix4 GetOrthoMatrix()
        {
            return Matrix4.CreateOrthographicOffCenter(-OrthoSize, OrthoSize, -OrthoSize, OrthoSize, NearPlane, FarPlane);
        }

        // debug lines for orthographic projection of the lightspace matrix
        
        internal void DrawOrthographicView()
        {
            Vector3 camCenter = RenderConfig.GetSunPosition() / 2 + OrthoOffset;
            Vector3 camForward = RenderConfig.GetSunDirection();
            Vector3 camRight = Vector3.Normalize(Vector3.Cross(camForward, Vector3.UnitY));
            Vector3 camUp = Vector3.Normalize(Vector3.Cross(camRight, camForward));

            Vector3 rightUp   = camCenter + (camRight * OrthoSize) + (camUp * OrthoSize);
            Vector3 leftUp    = camCenter - (camRight * OrthoSize) + (camUp * OrthoSize);
            Vector3 rightDown = camCenter + (camRight * OrthoSize) - (camUp * OrthoSize);
            Vector3 leftDown  = camCenter - (camRight * OrthoSize) - (camUp * OrthoSize);

            lines[0].Invalidate(rightUp, rightDown);
            lines[1].Invalidate(rightDown, leftDown);
            lines[2].Invalidate(leftDown, leftUp);
            lines[3].Invalidate(leftUp, rightUp);

            lines[4].Invalidate(rightUp   + (camForward * NearPlane), rightUp   + (camForward * FarPlane));
            lines[5].Invalidate(rightDown + (camForward * NearPlane), rightDown + (camForward * FarPlane));
            lines[6].Invalidate(leftDown  + (camForward * NearPlane), leftDown  + (camForward * FarPlane));
            lines[7].Invalidate(leftUp    + (camForward * NearPlane), leftUp    + (camForward * FarPlane));
        }
    }
}
