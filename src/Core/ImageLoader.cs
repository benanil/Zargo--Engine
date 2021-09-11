
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;

namespace ZargoEngine
{
    public static class ImageLoader
    {
        public static byte[] Load(string path,bool fullPath = false)
        {
            Image<Rgba32> image = fullPath ? Image.Load<Rgba32>(path) : Image.Load<Rgba32>(AssetManager.AssetsPath + path);

            var pixels = new List<byte>(4 * image.Width * image.Height);

            for (int y = 0; y < image.Height; y++)
            {
                var row = image.GetPixelRowSpan(y);

                for (int x = 0; x < image.Width; x++)
                {
                    pixels.Add(row[x].R);
                    pixels.Add(row[x].G);
                    pixels.Add(row[x].B);
                    pixels.Add(row[x].A);
                }
            }
            return pixels.ToArray();
        }

        public static byte[] Load(in string path,out int width,out int height, in bool mutate = false, bool fullPath = false)
        {
            Image<Rgba32> image = fullPath ? Image.Load<Rgba32>(path) : Image.Load<Rgba32>(AssetManager.AssetsPath + path);

            if (mutate)
            {
                image.Mutate(x =>
                {
                    x.Flip(FlipMode.Vertical);
                });
            }

            width = image.Width;
            height = image.Height;

            //Convert ImageSharp's format into a byte array, so we can use it with OpenGL.
            var pixels = new List<byte>(4 * image.Width * image.Height);

            for (int y = 0; y < image.Height; y++)
            {
                var row = image.GetPixelRowSpan(y);

                for (int x = 0; x < image.Width; x++)
                {
                    pixels.Add(row[x].R);
                    pixels.Add(row[x].G);
                    pixels.Add(row[x].B);
                    pixels.Add(row[x].A);
                }
            }
            return pixels.ToArray();
        }


        public static byte[] LoadRgb24(string path, out int width, out int height, bool mutate = false)
        {
            Image<Rgb24> image = Image.Load<Rgb24>(AssetManager.AssetsPath + path);

            if (mutate) image.Mutate(x => x.Flip(FlipMode.Vertical));

            width = image.Width;
            height = image.Height;

            //Convert ImageSharp's format into a byte array, so we can use it with OpenGL.
            var pixels = new List<byte>(4 * image.Width * image.Height);

            for (int y = 0; y < image.Height; y++)
            {
                var row = image.GetPixelRowSpan(y);

                for (int x = 0; x < image.Width; x++){
                    pixels.Add(row[x].R);
                    pixels.Add(row[x].G);
                    pixels.Add(row[x].B);
                }
            }
            return pixels.ToArray();
        }

    }
}
