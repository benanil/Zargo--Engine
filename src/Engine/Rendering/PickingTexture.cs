using OpenTK.Graphics.OpenGL4;
using System;

namespace ZargoEngine.Rendering
{
    public class PickingTexture : IDisposable
    {
        public int fboID;
        public int texID; // m_colorAttachment in cherno's video
        public int rbo;
        public int idAttacment;
        int depthAttachment;

        public int width, height;

        public int Samples;
        public bool SwapChainTarget;

        public int ColorAtachment;

        public PickingTexture(in int width, in int height)
        {
            Debug.Assert(!Init(width, height), "picking texture failed");
        }

        public bool Init(int width, int height)
        {
            this.width = width; this.height = height;

            // if alredy created clean old buffers
            if (this.fboID != 0) ClearBuffers();

            // create frame buffer
            fboID = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fboID);

            // create texture
            texID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texID);
            // we wrote intptr.zero cause first time we dont need to attach pixels to the image we will write it with frame buffer
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R32i, width, height, 0, PixelFormat.RedInteger, PixelType.UnsignedByte, IntPtr.Zero);

            // create min mag filter
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            // create warping settings
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            // initialize and attach texture to frame buffer
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, texID, 0);

            // create depth attachment
            depthAttachment = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, depthAttachment);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Depth24Stencil8,
                       width, height, 0, PixelFormat.DepthStencil, PixelType.UnsignedInt248,
                       IntPtr.Zero);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment,
                TextureTarget.Texture2D, depthAttachment, 0);

            GL.ReadBuffer(ReadBufferMode.None);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);

            Debug.Assert(GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete, "frame buffer failed");
            Unbind(); 
            return true;
        }

        public void Bind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fboID);
            GL.Viewport(0, 0, width, height);
        }

        public static void Unbind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void OnResize(in int x, in int y)
        {
            Init(x, y);
        }

        public int ReadPixel(in int x, in int y)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fboID);
            GL.ReadBuffer((ReadBufferMode)((int)ReadBufferMode.ColorAttachment0 ));
            int pixelData = 0;
            GL.ReadPixels(x, y, 1, 1, PixelFormat.RedInteger, PixelType.Int, ref pixelData);
            return pixelData;
        }

        public void ClearBuffers()
        {
            // GL.DeleteTexture(idAttacment);
            GL.DeleteFramebuffer(fboID);
            GL.DeleteTexture(rbo);
            GL.DeleteTexture(texID);
        }

        public int GetTextureId()
        {
            return texID;
        }

        public void Dispose()
        {
            ClearBuffers();
            GC.SuppressFinalize(this);
        }

    }
}
