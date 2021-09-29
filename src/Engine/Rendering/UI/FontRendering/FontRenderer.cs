#nullable disable
#define Editor

using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using FreeTypeSharp.Native;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;

// todo add Turkish characters
// note: highly optimized
namespace ZargoEngine.UI
{
    using Mathmatics;
    using Editor;
    using Rendering;
    
    public unsafe class FontRenderer : UIbase
    {
        public string text = "Animation Test";
        public float Scale = 1f;
        private int fontName; // (hashmap) it is not string cause of memory allocation

        public Action OnTextChanged;

        private static readonly Shader shader;
        private static readonly SortedDictionary<int, Font> avalibleFonts = new SortedDictionary<int, Font>(); // fontları taşıyan dictionary ve onun içinde her bir font ve characterleri
        
        private int vaoID, vboID, eboID;

        readonly List<float> vertexes = new List<float>();
        readonly List<int>   indices  = new List<int>();

        ~FontRenderer() => shader.Dispose();

        static FontRenderer()
        {
            // create font shader
            shader = new Shader(AssetManager.AssetsPath + "Shaders/Font.vert", AssetManager.AssetsPath + "Shaders/Font.glsl");
        }

        public FontRenderer(GameObject go) : this(go, AssetManager.GetFileLocation("Fonts/OpenSans-Regular.ttf")) { }

        public FontRenderer(GameObject go, in string path) : base(go,path)
        {
            name = "Font Renderer";
            fontName = path.GetHashCode();

            OnTextChanged = TextChanged;

            ChangeFont(path);
            TextChanged();

            PositionChanged(ref transform.position);
        }

        protected override bool CanInitialize(in string path)
        {
            if (!File.Exists(path)) {
                Debug.LogError("lao font adres wrong"); // szaszi
                Console.ReadLine();
                return false;
            }
            return true;
        }

        public void ChangeFont(in string path)
        {
            if (!avalibleFonts.ContainsKey(path.GetHashCode())) CreateFont(path);
            fontName = path.GetHashCode();
        }

        private static void CreateFont(in string fontName)
        {
            // const veriables
            const int TexWidth = 48, TexHeight = 48;
            const int OneTexSize = TexWidth * TexHeight;
            // classic initialization
            FT_LibraryRec fT_LibraryRec = new FT_LibraryRec();
            IntPtr libraryPtr = (IntPtr)(&fT_LibraryRec);
            FT.FT_Init_FreeType(out libraryPtr);
            FT.FT_New_Face(libraryPtr, fontName, 0, out IntPtr facePtr);
            FT_FaceRec face = Marshal.PtrToStructure<FT_FaceRec>(facePtr);
            Debug.Assert(facePtr == IntPtr.Zero, "font file initialization failed");
            FT.FT_Set_Pixel_Sizes(facePtr, TexWidth, TexHeight);

            Character[] characters = new Character[101];

            AssetManager.TryGetFileLocation($"Fonts/Generated Bitmaps/{Path.GetFileName(fontName)}Chars.json", out string charPath);

            if (!AssetManager.TryGetFileLocation($"Fonts/Generated Bitmaps/{Path.GetFileName(fontName)}.png", out string fileLocation))
            {
                List<Rgba32> Atlas = new List<Rgba32>(100 * OneTexSize); // 97 because 128-31(all chars) 480 x 480 image
                List<Image<Rgba32>> textures = new List<Image<Rgba32>>();
                Rgba32 blackByte = new Rgba32(byte.MinValue, byte.MinValue, byte.MinValue);
                Rgba32[] blackByteArray = new Rgba32[OneTexSize];
                // for get rid of null pixels
                int i;
                for (i = 0; i < blackByteArray.Length; i++) blackByteArray[i] = blackByte;
                for (i = 0; i < Atlas.Count; i++) Atlas[i] = blackByte;

                // calculate all chatacter textures
                for (i = 0; i < 101; i++)
                {
                    FT.FT_Load_Char(facePtr, Convert.ToChar(32 + i), FT.FT_LOAD_RENDER); // 32 start offset of writing characters

                    IntPtr texPointer = face.glyph->bitmap.buffer;

                    if (texPointer == IntPtr.Zero)
                    { // if texture is not exist create black one
                        textures.Add(Image.LoadPixelData(blackByteArray, TexWidth, TexHeight));
                        continue;
                    }

                    int width = (int)face.glyph->bitmap.width;
                    int height = (int)face.glyph->bitmap.rows;

                    byte[] managedArray = new byte[width * height];

                    Marshal.Copy(texPointer, managedArray, 0, managedArray.Length);
                    Rgba32[] data = new Rgba32[managedArray.Length];

                    int j;
                    for (j = 0; j < managedArray.Length; j++)
                    {
                        data[j] = new Rgba32(managedArray[j], managedArray[j], managedArray[j]);
                    }

                    for (j = 0; j < height; j++)
                    {
                        Array.Reverse(data, j * width, width);
                    }

                    Rgba32[] fiiledData = new Rgba32[OneTexSize];

                    for (j = 0; j < fiiledData.Length; j++) fiiledData[j] = blackByte;

                    for (int column = 0; column < height; column++)
                    {
                        Array.Copy(data, column * width, fiiledData, 48 * column + (48 - width), width);
                    }
                    Array.Reverse(fiiledData);

                    textures.Add(Image.LoadPixelData(fiiledData, TexWidth, TexHeight));

                    characters[i] = new Character
                    (
                        new Vector2(Mathmatic.Repeat(i * 48, 480), MathF.Floor(i / 10) * 48 + (48 - height)),
                        new Vector2i((int)face.glyph->bitmap.width, (int)face.glyph->bitmap.rows),
                        new Vector2i(face.glyph->bitmap_left, face.glyph->bitmap_top), (uint)face.glyph->advance.x
                    );
                }

                // align character textures correctly
                for (int columns = 0; columns < 10; columns++)
                    for (int texHeight = 0; texHeight < TexHeight; texHeight++)
                        for (int rowIndex = 0; rowIndex < 10; rowIndex++)
                            for (int texWidth = 0; texWidth < TexWidth; texWidth++)
                                Atlas.Add(textures[columns * 10 + rowIndex].GetPixelRowSpan(texHeight)[texWidth]);

                // create bitmap
                using var image = Image.LoadPixelData(Atlas.ToArray(), 480, 480);
                using FileStream stream = File.Create(fileLocation);
                image.SaveAsPng(stream);
                // create FontData
                characters.SaveArrayStream(File.CreateText(charPath));
            }
            else
            {
                characters = JsonManager.LoadArray<Character>(charPath);
            }

            Texture bitmapTexture = AssetManager.GetTexture(fileLocation);
            bitmapTexture.SetAsUI();

            var newFont = new Font(characters, bitmapTexture);

            avalibleFonts.Add(fontName.GetHashCode(), newFont);

            // ending
            FT.FT_Done_Face(facePtr);
            FT.FT_Done_FreeType(libraryPtr);
        }

        public override void DrawWindow()
        {
            base.DrawWindow();
            GUI.TextField(ref text, nameof(text), OnTextChanged);
            GUI.FloatField(ref Scale, nameof(Scale), OnValidate);
            ImGui.Separator();
        }

        protected override void CalculateBounds()
        {
            float x = 0, y = 0;
            float xPos = 0, yPos = 0;
#if Editor
            Debug.Log("WindowBottom: " + WindowBottom);
#else
            Vector2i windowBottom = new(WindowBounds.Min.X, Screen.MonitorHeight - (WindowBounds.Min.Y + Program.MainGame.ClientSize.Y));
#endif

            Font font = avalibleFonts[fontName];

            Character ch = font[text[0]];

            bounds.Min = transform.position.Xy + WindowBottom + new Vector2(x + ch.bearing.X, y - (ch.size.Y - ch.bearing.Y)) - BoundsMultipler;

            for (int i = 0; i < text.Length; i++)
            {
                char character = text[i];
                ch = font[character];

                if (character == ' ' || character > 128 || character < 32)
                { // > 128 cause we have only asci characters for now
                    x += 1280 >> 6;
                    continue;
                }

                xPos = x + ch.bearing.X;
                yPos = y - (ch.size.Y - ch.bearing.Y);

                x += ch.advance >> 6;
            }

            Character lastCharacter = font[text[^1]];

            float w = lastCharacter.size.X, h = lastCharacter.size.Y;

            bounds.Max = transform.position.Xy + WindowBottom + new Vector2(xPos + w, yPos + h) + BoundsMultipler;
        }

        private void TextChanged()
        {
            CalculateBounds();
            CalculateVertices();
        }

        int realTextLength; // lenght of text whitout spaces and unknown characters

        private void CalculateVertices()
        {
            float x = 0, y = 0;

            Font font = avalibleFonts[fontName];

            realTextLength = text.Length;

            for (int i = 0; i < text.Length; i++)
                if (text[i] == ' ') realTextLength--;

            Character ch;
            char character;
            int nextIndex = 0;

            vertexes.Clear();
            indices.Clear();

            for (int i = 0; i < text.Length; i++)
            {
                character = text[i];

                if (character == ' ' || character > 128 || character < 32)
                { // > 128 cause we have only asci characters for now
                    x += 1280 >> 6; // one character space
                    nextIndex -= 4;
                    continue;
                }

                ch = font[character];

                float xPos = x + ch.bearing.X;
                float yPos = y - (ch.size.Y - ch.bearing.Y);
                float w = ch.size.X;
                float h = ch.size.Y;

                float[] vertex = new float[16]
                {
                    xPos    , yPos + h, ch.texCoord.X    , ch.texCoord.Y + h,
                    xPos    , yPos    , ch.texCoord.X    , ch.texCoord.Y    ,
                    xPos + w, yPos    , ch.texCoord.X + w, ch.texCoord.Y    ,    
                    xPos + w, yPos + h, ch.texCoord.X + w, ch.texCoord.Y + h
                };

                vertexes.AddRange(vertex);

                int indiceOffset = i * 4 + nextIndex;
                indices.AddRange(new int[6] { indiceOffset + 0, indiceOffset + 1, indiceOffset + 2, indiceOffset + 0, indiceOffset + 2, indiceOffset + 3 });

                x += ch.advance >> 6;
            }

            GenerateBuffers();
        }

        protected void GenerateBuffers()
        {
            DeleteBuffers();
            // generate buffers and vao
            vaoID = GL.GenVertexArray();
            GL.BindVertexArray(vaoID);

            vboID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboID);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * vertexes.Count, vertexes.ToArray(), BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, true, sizeof(float) * 4, IntPtr.Zero);
            GL.EnableVertexAttribArray(0);

            eboID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, eboID);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Count * sizeof(int), indices.ToArray(), BufferUsageHint.StaticDraw);

            // unbind
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        // render line of text
        // -------------------
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
            GL.BindTexture(TextureTarget.Texture2D, avalibleFonts[fontName].texture.texID);
            GL.BindVertexArray(vaoID);

            GL.DrawElements(PrimitiveType.Triangles, indices.Count, DrawElementsType.UnsignedInt, 0);

            GL.DisableVertexAttribArray(0);
            GL.BindVertexArray(0);
            Shader.DetachShader();

            GL.Disable(EnableCap.CullFace);
            GL.DepthMask(true);
            GL.Enable(EnableCap.DepthTest);
        }

        public override void DeleteBuffers()
        {
            GL.DeleteVertexArray(vaoID);
            GL.DeleteBuffer(vboID);
            GL.DeleteBuffer(eboID);
        }
    }

    public readonly struct Font
    {
        private readonly Character[] characters;
        public readonly Texture texture;

        public Font(Character[] characters, Texture texture)
        {
            this.texture = texture;
            this.characters = characters;
        }

        public Character this[char index]
        {
            get => characters[index - 32];
        }
    }

    [Serializable]
    public struct Character
    {
        public Vector2 texCoord;
        public Vector2 size;
        public Vector2 bearing;
        public uint advance;

        public Character(in Vector2 texturePosition, in Vector2i size, in Vector2i bearing, in uint advance)
        {
            this.size = size;
            this.bearing = bearing;
            this.advance = advance;
            texCoord = texturePosition;
        }
    }
}
