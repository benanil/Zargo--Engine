
using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using Image = SixLabors.ImageSharp.Image;

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

        public static byte[] Load(in string path,out int width, out int height, in bool mutate = false, bool fullPath = false)
        {
            // if (Path.GetExtension(path) == ".dds")
            // {
            //     System.Console.WriteLine("dds");
            //     using (FileStream stream = new FileStream(path, FileMode.Open))
            //     {
            //         DDSImage dds = new DDSImage(stream);
            //         Bitmap image = dds.BitmapImage;
            //         image.RotateFlip(RotateFlipType.RotateNoneFlipY);
            // 
            //         BitmapData data = image.LockBits(new System.Drawing.Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            //         width = data.Width;
            //         height = data.Height;
            //    
            //         unsafe
            //         { 
            //             byte[] result = new byte[data.Stride * height];
            //             
            //             fixed(void* ptr = &result[0])
            //             Unsafe.CopyBlock(ptr, data.Scan0.ToPointer(), (uint)result.Length);
            //             return result;
            //         }
            //     }
            // }
            
            Image<Rgba32> image = fullPath ? Image.Load<Rgba32>(path) : Image.Load<Rgba32>(AssetManager.AssetsPath + path);

            if (mutate)
            {
                image.Mutate(x =>
                {
                    x.Resize(ToNearest(image.Width) / 4, ToNearest(image.Height) / 4);
                });
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

        static int ToNextNearest(int v)
        {
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v++;
            return v;
        }

        // if value = 1000 or 1100 returns 1024
        // rounds closest power of 2
        static int ToNearest(in int value)
        {
            int next = ToNextNearest(value);
            int prev = next >> 1;
            return MathF.Abs(next - value) < MathF.Abs(prev - value) ? next : prev;
        }

        public static byte[][] LoadWithMipMaps(in string path, out int[] width, out int[] height, in bool mutate = false, bool fullPath = false)
        {
            Image<Rgba32> image = fullPath ? Image.Load<Rgba32>(path) : Image.Load<Rgba32>(AssetManager.AssetsPath + path);

            if (mutate)
            {
                image.Mutate(x =>
                {
                    x.Flip(FlipMode.Vertical);
                });
            }

            Vector2i nearest = new Vector2i(ToNearest(image.Width), ToNearest(image.Height));
            // 2048 = 4     1024 = 3 
            // 512  = 3     256  = 2
            // 128  = 1     64   = 1
            byte numMipmaps = (byte)MathF.Max(MathF.Floor(MathF.Log2(nearest.X) - 1 / 2) - 1, 1); // returns minimum 1

            width  = new int[numMipmaps];
            height = new int[numMipmaps];

            byte[][] result = new byte[numMipmaps][];

            for (byte i = 0; i < numMipmaps; i++)
            {
                // shifts for decreasing size over iteration
                int mipWidth  = width[i]  = nearest.X >> i; 
                int mipHeight = height[i] = nearest.Y >> i; 
               
                image.Mutate(x => x.Resize(mipWidth, mipHeight));

                result[i] = new byte[4 * width[i] * height[i]]; // pixels
                
                for (int y = 0; y < height[i]; y++)
                {
                    Span<Rgba32> row = image.GetPixelRowSpan(y);

                    for (int x = 0; x < width[i]; x++)
                    {
                        result[i][(x * i) + 0] = row[x].R;
                        result[i][(x * i) + 1] = row[x].G;
                        result[i][(x * i) + 2] = row[x].B;
                        result[i][(x * i) + 3] = row[x].A;
                    }
                }
            }

            return result;
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
