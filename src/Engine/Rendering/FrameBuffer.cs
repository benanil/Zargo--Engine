using OpenTK.Graphics.OpenGL4;
using System;

#nullable disable warnings

namespace ZargoEngine.Rendering
{
    public class FrameBuffer : IDisposable
    {
        public int fboID;
        public int texID; 
        public int depthAttachment;

        public int width, height;

        FramebufferAttachment[] attachments;
        PixelInternalFormat pixelInternalFormat = PixelInternalFormat.Rgba;

        public FrameBuffer(in int width, in int height)
        {
            Invalidate(width, height);
        }

        // you can adjustpixel internal format as floating point or rgba or something
        public FrameBuffer(in int width, in int height, PixelInternalFormat pixelInternalFormat) : this(width, height)
        {
            this.pixelInternalFormat = pixelInternalFormat;
        }

        public FrameBuffer(in int width, in int height, FramebufferAttachment[] attachments) : this(width, height)
        {
            this.attachments = attachments;
        }

        public void Invalidate(in int width, in int height)
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
            GL.TexImage2D(TextureTarget.Texture2D, 0, pixelInternalFormat, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);

            // create min mag filter
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            // create warping settings
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            // initialize and attach texture to frame buffer
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, texID, 0);

            // create depth attachment
            depthAttachment = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, depthAttachment);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Depth24Stencil8, width, height, 0, PixelFormat.DepthStencil, PixelType.UnsignedInt248, IntPtr.Zero);

            if (attachments != null) {
                GL.InvalidateNamedFramebufferData((uint)fboID, attachments.Length, ref attachments[0]);
            }

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment,
                TextureTarget.Texture2D, depthAttachment, 0);

            Unbind();
        }

        public void Bind() {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fboID);
            GL.Viewport(0, 0, width, height);
        }

        public static void Unbind() {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
        
        public void unbind() {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void OnResize(in int x, in int y) {
            Invalidate(x, y);
        }

        public IntPtr ReadPixel(in int attachmentIndex, in int x, in int y) {
            GL.ReadBuffer((ReadBufferMode)((int)ReadBufferMode.ColorAttachment0 + attachmentIndex));
            IntPtr pixelData = IntPtr.Zero;
            GL.ReadPixels(x, y, 1, 1, PixelFormat.RedInteger, PixelType.Int, pixelData);
            return pixelData;
        }

        public void ClearBuffers() {
            GL.DeleteFramebuffer(fboID);
            GL.DeleteTexture(depthAttachment);
            GL.DeleteTexture(texID);
        }

        public int GetTextureId() {
            return texID;
        }

        public void Dispose() {
            ClearBuffers();
            GC.SuppressFinalize(this);
        }
    }
}