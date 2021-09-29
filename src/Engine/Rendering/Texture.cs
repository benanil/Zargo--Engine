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
            var pixels = ImageLoader.Load(path, out width, out height, true, true);
            // var mipmaps = ImageLoader.LoadWithMipMaps(path, out var widths, out var heights,true,true);
            // width = widths[0]; height = heights[0];
            this.path = path;

            texID = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, texID);
            // GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
            // GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, mipmaps.Length - 1);
            
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.SrgbAlpha, width, height, 0, pixelFormat, PixelType.UnsignedByte, pixels);
            
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            Debug.Log(GL.GetError());
        }

        public void Bind(TextureUnit unit = TextureUnit.Texture0)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D,texID);
        }

        /// <summary>this is for better texture quality and disabling mip mapping etc.</summary>
        public void SetAsUI()
        {
            GL.BindTexture(TextureTarget.Texture2D, texID);
            SetWrapS(TextureWrapMode.ClampToEdge);
            SetWrapT(TextureWrapMode.ClampToEdge);
            SetMinFilter(TextureMinFilter.Linear);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public static void UnBind() 
            => GL.BindTexture(TextureTarget.Texture2D, 0);
        public void SetMinFilter(TextureMinFilter filter)
        {
            GL.BindTexture(TextureTarget.Texture2D, texID);
            GL.TextureParameter(texID, TextureParameterName.TextureMinFilter, (int)filter);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void SetMagFilter(TextureMagFilter filter)
        {
            GL.BindTexture(TextureTarget.Texture2D, texID);
            GL.TextureParameter(texID, TextureParameterName.TextureMagFilter, (int)filter);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void SetWrapS(TextureWrapMode filter) => GL.TextureParameter(texID, TextureParameterName.TextureWrapS, (int)filter);
        public void SetWrapT(TextureWrapMode filter) => GL.TextureParameter(texID, TextureParameterName.TextureWrapT, (int)filter);

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
