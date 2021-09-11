using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

namespace ZargoEngine.Rendering
{
    public class Texture : IDisposable
    {
        public const SizedInternalFormat Srgb8Alpha8 = (SizedInternalFormat)All.Srgb8Alpha8;
        public const SizedInternalFormat RGB32F = (SizedInternalFormat)All.Rgb32f;
        public const GetPName MAX_TEXTURE_MAX_ANISOTROPY = (GetPName)0x84FF;

        public readonly int texID;

        public readonly int width, height;

        public static readonly float MaxAniso = GL.GetFloat(MAX_TEXTURE_MAX_ANISOTROPY);

        public readonly string path;

        public Texture(string path, PixelFormat pixelFormat = PixelFormat.Rgba, bool createMipMap = true)
        {
            var pixels = ImageLoader.Load(path, out width, out height,true,true);
            this.path = path;

            texID = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, texID);
            
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.SrgbAlpha, width, height,0, pixelFormat, PixelType.UnsignedByte, pixels);

            if (createMipMap) {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapLinear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.GenerateTextureMipmap(texID);
            }
            else {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            }

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            Debug.Log(GL.GetError());
        }

        public Texture(int width, int height){
            texID = GL.GenTexture();
            GL.BindTexture (TextureTarget.Texture2D, texID);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, width, height, 0,PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
        }

        public void Bind(TextureUnit unit = TextureUnit.Texture0)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D,texID);
        }

        /// <summary>this is for better texture quality and disabling mip mapping etc.</summary>
        public void SetAsUI()
        {
            SetWrapS(TextureParameterName.ClampToEdge);
            SetWrapT(TextureParameterName.ClampToEdge);
            SetMinFilter(TextureMinFilter.Linear);
        }

        public static void UnBind()                       => GL.BindTexture(TextureTarget.Texture2D, 0);
        public void SetMinFilter(TextureMinFilter filter) => GL.TextureParameter(texID, TextureParameterName.TextureMinFilter, (int)filter);
        public void SetMagFilter(TextureMagFilter filter) => GL.TextureParameter(texID, TextureParameterName.TextureMagFilter, (int)filter);
        public void SetWrapS(TextureParameterName filter) => GL.TextureParameter(texID, TextureParameterName.TextureWrapS, (int)filter);
        public void SetWrapT(TextureParameterName filter) => GL.TextureParameter(texID, TextureParameterName.TextureWrapT, (int)filter);

        public void SetAnisotropy(float level){
            const TextureParameterName TEXTURE_MAX_ANISOTROPY = (TextureParameterName)0x84FE;
            GL.TextureParameter(texID, TEXTURE_MAX_ANISOTROPY, MathHelper.Clamp(level, 1, MaxAniso));
        }

        public void SetLod(int @base, int min, int max){
            GL.TextureParameter(texID, TextureParameterName.TextureLodBias, @base);
            GL.TextureParameter(texID, TextureParameterName.TextureMinLod, min);
            GL.TextureParameter(texID, TextureParameterName.TextureMaxLod, max);
        }

        public void Dispose()
        {
            GL.DeleteTexture(texID);
            GC.SuppressFinalize(this);
        }
    }
}
