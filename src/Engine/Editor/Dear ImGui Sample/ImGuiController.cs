using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;
using System.Windows.Forms;
using ZargoEngine.Helper;
using ZargoEngine;
using ZargoEngine.SaveLoad;

using Keys = OpenTK.Windowing.GraphicsLibraryFramework.Keys;
using ImGuizmoNET;
using IconFonts;

namespace Dear_ImGui_Sample
{
    /// <summary>
    /// A modified version of Veldrid.ImGui's ImGuiRenderer.
    /// Manages input for ImGui and handles rendering ImGui's DrawLists with Veldrid.
    /// </summary>
    public class ImGuiController : IDisposable
    {
        private const string WindowStylePlayerPrefs = "Window Style";
        
        public static ImGuiController instance;
        private bool _frameBegun;

        private int _vertexArray;
        private int _vertexBuffer;
        private int _vertexBufferSize;
        private int _indexBuffer;
        private int _indexBufferSize;

        private Texture _fontTexture;
        private Shader _shader;
        
        private int _windowWidth;
        private int _windowHeight;

        private System.Numerics.Vector2 _scaleFactor = System.Numerics.Vector2.One;

        public static ImFontPtr RobotoFont;
        public static ImFontPtr RobotoBold;

        public static unsafe float BoldFontSize => RobotoBold.NativePtr->Scale;

        /// <summary>
        /// Constructs a new ImGuiController.
        /// </summary>
        public unsafe ImGuiController(int width, int height)
        {
            instance = this;
            _windowWidth = width;
            _windowHeight = height;

            IntPtr context = ImGui.CreateContext();
            ImGui.SetCurrentContext(context);

            var io = ImGui.GetIO();

            RobotoFont = ImGui.GetIO().Fonts.AddFontFromFileTTF(AssetManager.GetFileLocation(@"Fonts\JetBrainsMono-Regular.ttf"), 20);
            RobotoBold = ImGui.GetIO().Fonts.AddFontFromFileTTF(AssetManager.GetFileLocation(@"Fonts\Roboto-Bold.ttf"), 18);

            FontAwesome5.Construct();

            RobotoFont.NativePtr->Scale = .75f;

            io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
            io.BackendFlags |= ImGuiBackendFlags.HasMouseCursors;

            io.ConfigFlags  |= ImGuiConfigFlags.DockingEnable;
            io.ConfigFlags  |= ImGuiConfigFlags.ViewportsEnable;

            io.ConfigWindowsResizeFromEdges = true; 

            io.ConfigDockingWithShift = true;

            CreateDeviceResources();
            SetKeyMappings();

            SetPerFrameImGuiData(1f / 60f);

            ImGui.NewFrame();
            
            ImGuizmo.SetImGuiContext(context);
            ImGuizmo.BeginFrame();

            if (PlayerPrefs.TryGetInt(WindowStylePlayerPrefs, out int value))
            {
                switch (value)
                {
                    case 0: DarkTheme(); break;
                    case 1: ImGui.StyleColorsLight(); break;
                    case 2: RedStyle();  break;
                }
            }
            else DarkTheme();

            _frameBegun = true;
        }

        private bool dockOpen = true;
        ImGuiDockNodeFlags dockspace_flags = ImGuiDockNodeFlags.None;
        
        public unsafe void GenerateDockspace(in Action editorWindow)
        {
            ImGui.PushFont(RobotoFont);

            var viewportPtr = ImGui.GetMainViewport();
        
            ImGui.SetNextWindowPos(System.Numerics.Vector2.Zero, ImGuiCond.Always);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(Program.MainGame.ClientSize.X, Program.MainGame.ClientSize.Y));
        
            ImGui.SetNextWindowViewport(viewportPtr.ID);
        
            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
        
            const ImGuiWindowFlags windowFlags = ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoDocking  
            | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove
            | ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus;
        
            ImGui.Begin("Dockspace Demo", ref dockOpen,windowFlags);
        
            ImGui.PopStyleVar(2);
        
            ImGui.DockSpace(ImGui.GetID("Dockspace"), System.Numerics.Vector2.Zero,dockspace_flags);
        
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    ImGui.Separator();
        
                    if (ImGui.MenuItem("Save Scene","CTRL+S")) {
                        SceneManager.currentScene.SaveScene();
                    }
        
                    if (ImGui.MenuItem("Load")) {
                        LoadSceneDialag();
                    }

                    if (ImGui.MenuItem("Import")){
                        AssetImporter.ShowImportWindow();
                    }

                    if (ImGui.BeginMenu("Folders"))
                    {
                        if (ImGui.MenuItem("Appdata Folder")){
                            Process.Start("explorer.exe", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
                        }

                        if (ImGui.MenuItem("Assets Folder")){
                            Process.Start("explorer.exe",Environment.CurrentDirectory + "\\" + AssetManager.AssetsPathBackSlash);
                        }
                        ImGui.EndMenu();
                    }

                    if (ImGui.MenuItem("Clear All Player Prefs")){
                        PlayerPrefs.ClearAllPlayerPrefs();
                    }
        
                    if (ImGui.MenuItem("Exit")){
                        Program.MainGame.Close();
                    }
        
                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }
        
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("Edit"))
                {
                    ImGui.Separator();
        
                    if (ImGui.MenuItem("Dark Theme")){
                        PlayerPrefs.SetInt(WindowStylePlayerPrefs, 0);
                        DarkTheme(); 
                    }
                    if (ImGui.MenuItem("White Theme")){
                        PlayerPrefs.SetInt(WindowStylePlayerPrefs, 1);
                        ImGui.StyleColorsLight();
                    }
                    if (ImGui.MenuItem("Red Theme")){
                        PlayerPrefs.SetInt(WindowStylePlayerPrefs, 2);
                        RedStyle(); 
                    }
        
                    ImGui.EndMenu();
                }
                ImGui.EndMenuBar();
            }

            editorWindow();
        
            ImGui.End();
            
            ImGui.PopFont();
        }

        private static void LoadSceneDialag()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                DefaultExt = ".xml",
                Title = "chose scene",
                Filter = "All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Todo : :>
            }
        }

        public void WindowResized(int width, int height)
        {
            _windowWidth = width;
            _windowHeight = height;
        }

        public void DestroyDeviceObjects()
        {
            Dispose();
        }

        public void CreateDeviceResources()
        {
            Util.CreateVertexArray("ImGui", out _vertexArray);

            _vertexBufferSize = 10000;
            _indexBufferSize = 2000;

            Util.CreateVertexBuffer("ImGui", out _vertexBuffer);
            Util.CreateElementBuffer("ImGui", out _indexBuffer);
            GL.NamedBufferData(_vertexBuffer, _vertexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.NamedBufferData(_indexBuffer, _indexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            RecreateFontDeviceTexture();

            const string VertexSource = @"#version 330 core
            uniform mat4 projection_matrix;
            
            layout(location = 0) in vec2 in_position;
            layout(location = 1) in vec2 in_texCoord;
            layout(location = 2) in vec4 in_color;
            
            out vec4 color;
            out vec2 texCoord;
            
            void main()
            {
                gl_Position = projection_matrix * vec4(in_position, 0, 1);
                color = in_color;
                texCoord = in_texCoord;
            }";

            const string FragmentSource =
            @"#version 330 core
            uniform sampler2D in_fontTexture;
            
            in vec4 color;
            in vec2 texCoord;
            
            out vec4 outputColor;
            
            void main()
            {
                outputColor = color * texture(in_fontTexture, texCoord);
            }";
            
            _shader = new Shader("ImGui", VertexSource, FragmentSource);

            GL.VertexArrayVertexBuffer(_vertexArray, 0, _vertexBuffer, IntPtr.Zero, Unsafe.SizeOf<ImDrawVert>());
            GL.VertexArrayElementBuffer(_vertexArray, _indexBuffer);

            GL.EnableVertexArrayAttrib(_vertexArray, 0);
            GL.VertexArrayAttribBinding(_vertexArray, 0, 0);
            GL.VertexArrayAttribFormat(_vertexArray, 0, 2, VertexAttribType.Float, false, 0);

            GL.EnableVertexArrayAttrib(_vertexArray, 1);
            GL.VertexArrayAttribBinding(_vertexArray, 1, 0);
            GL.VertexArrayAttribFormat(_vertexArray, 1, 2, VertexAttribType.Float, false, 8);

            GL.EnableVertexArrayAttrib(_vertexArray, 2);
            GL.VertexArrayAttribBinding(_vertexArray, 2, 0);
            GL.VertexArrayAttribFormat(_vertexArray, 2, 4, VertexAttribType.UnsignedByte, true, 16);

            Util.CheckGLError("End of ImGui setup");
        }

        /// <summary>
        /// Recreates the device texture used to render text.
        /// </summary>
        public unsafe void RecreateFontDeviceTexture()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height, out int bytesPerPixel);

            _fontTexture = new Texture("ImGui Text Atlas", width, height, pixels);
            _fontTexture.SetMagFilter(TextureMagFilter.Linear);
            _fontTexture.SetMinFilter(TextureMinFilter.Linear);

            io.Fonts.SetTexID((IntPtr)_fontTexture.TexID);

            io.Fonts.ClearTexData();
        }

        public static void RedStyle()
        {
            var style = ImGui.GetStyle();
            style.FrameRounding = 4.0f;
            style.WindowBorderSize = 0.0f;
            style.PopupBorderSize = 0.0f;
            style.GrabRounding = 4.0f;
            style.Alpha = 1;

            var colors = style.Colors;
            colors[(int)ImGuiCol.Text]                  = new Color4(1.00f, 1.00f, 1.00f, 1.00f).ToSystem();
            colors[(int)ImGuiCol.TextDisabled]          = new Color4(0.73f, 0.75f, 0.74f, 1.00f).ToSystem();
            colors[(int)ImGuiCol.WindowBg]              = new Color4(0.09f, 0.09f, 0.09f, 0.94f).ToSystem();
            colors[(int)ImGuiCol.ChildBg]               = new Color4(0.00f, 0.00f, 0.00f, 0.00f).ToSystem();
            colors[(int)ImGuiCol.PopupBg]               = new Color4(0.08f, 0.08f, 0.08f, 0.94f).ToSystem();
            colors[(int)ImGuiCol.Border]                = new Color4(0.20f, 0.20f, 0.20f, 0.50f).ToSystem();
            colors[(int)ImGuiCol.BorderShadow]          = new Color4(0.00f, 0.00f, 0.00f, 0.00f).ToSystem();
            colors[(int)ImGuiCol.FrameBg]               = new Color4(0.71f, 0.39f, 0.39f, 0.54f).ToSystem();
            colors[(int)ImGuiCol.FrameBgHovered]        = new Color4(0.84f, 0.66f, 0.66f, 0.40f).ToSystem();
            colors[(int)ImGuiCol.FrameBgActive]         = new Color4(0.84f, 0.66f, 0.66f, 0.67f).ToSystem();
            colors[(int)ImGuiCol.TitleBg]               = new Color4(0.47f, 0.22f, 0.22f, 0.67f).ToSystem();
            colors[(int)ImGuiCol.TitleBgActive]         = new Color4(0.47f, 0.22f, 0.22f, 1.00f).ToSystem();
            colors[(int)ImGuiCol.TitleBgCollapsed]      = new Color4(0.47f, 0.22f, 0.22f, 0.67f).ToSystem();
            colors[(int)ImGuiCol.MenuBarBg]             = new Color4(0.34f, 0.16f, 0.16f, 1.00f).ToSystem();
            colors[(int)ImGuiCol.ScrollbarBg]           = new Color4(0.02f, 0.02f, 0.02f, 0.53f).ToSystem();
            colors[(int)ImGuiCol.ScrollbarGrab]         = new Color4(0.31f, 0.31f, 0.31f, 1.00f).ToSystem();
            colors[(int)ImGuiCol.ScrollbarGrabHovered]  = new Color4(0.41f, 0.41f, 0.41f, 1.00f).ToSystem();
            colors[(int)ImGuiCol.ScrollbarGrabActive]   = new Color4(0.51f, 0.51f, 0.51f, 1.00f).ToSystem();
            colors[(int)ImGuiCol.CheckMark]             = new Color4(1.00f, 1.00f, 1.00f, 1.00f).ToSystem();
            colors[(int)ImGuiCol.SliderGrab]            = new Color4(0.71f, 0.39f, 0.39f, 1.00f).ToSystem();
            colors[(int)ImGuiCol.SliderGrabActive]      = new Color4(0.84f, 0.66f, 0.66f, 1.00f).ToSystem();
            colors[(int)ImGuiCol.Button]                = new Color4(0.47f, 0.22f, 0.22f, 0.65f).ToSystem();
            colors[(int)ImGuiCol.ButtonHovered]         = new Color4(0.71f, 0.39f, 0.39f, 0.65f).ToSystem();
            colors[(int)ImGuiCol.ButtonActive]          = new Color4(0.20f, 0.20f, 0.20f, 0.50f).ToSystem();
            colors[(int)ImGuiCol.Header]                = new Color4(0.71f, 0.39f, 0.39f, 0.54f).ToSystem();
            colors[(int)ImGuiCol.HeaderHovered]         = new Color4(0.84f, 0.66f, 0.66f, 0.65f).ToSystem();
            colors[(int)ImGuiCol.HeaderActive]          = new Color4(0.84f, 0.66f, 0.66f, 0.00f).ToSystem();
            colors[(int)ImGuiCol.Separator]             = new Color4(0.43f, 0.43f, 0.50f, 0.50f).ToSystem();
            colors[(int)ImGuiCol.SeparatorHovered]      = new Color4(0.71f, 0.39f, 0.39f, 0.54f).ToSystem();
            colors[(int)ImGuiCol.SeparatorActive]       = new Color4(0.71f, 0.39f, 0.39f, 0.54f).ToSystem();
            colors[(int)ImGuiCol.ResizeGrip]            = new Color4(0.71f, 0.39f, 0.39f, 0.54f).ToSystem();
            colors[(int)ImGuiCol.ResizeGripHovered]     = new Color4(0.84f, 0.66f, 0.66f, 0.66f).ToSystem();
            colors[(int)ImGuiCol.ResizeGripActive]      = new Color4(0.84f, 0.66f, 0.66f, 0.66f).ToSystem();
            colors[(int)ImGuiCol.Tab]                   = new Color4(0.71f, 0.39f, 0.39f, 0.54f).ToSystem();
            colors[(int)ImGuiCol.TabHovered]            = new Color4(0.84f, 0.66f, 0.66f, 0.66f).ToSystem();
            colors[(int)ImGuiCol.TabActive]             = new Color4(0.84f, 0.66f, 0.66f, 0.66f).ToSystem();
            colors[(int)ImGuiCol.TabUnfocused]          = new Color4(0.07f, 0.10f, 0.15f, 0.97f).ToSystem();
            colors[(int)ImGuiCol.TabUnfocusedActive]    = new Color4(0.14f, 0.26f, 0.42f, 1.00f).ToSystem();
            colors[(int)ImGuiCol.PlotLines]             = new Color4(0.61f, 0.61f, 0.61f, 1.00f).ToSystem();
            colors[(int)ImGuiCol.PlotLinesHovered]      = new Color4(1.00f, 0.43f, 0.35f, 1.00f).ToSystem();
            colors[(int)ImGuiCol.PlotHistogram]         = new Color4(0.90f, 0.70f, 0.00f, 1.00f).ToSystem();
            colors[(int)ImGuiCol.PlotHistogramHovered]  = new Color4(1.00f, 0.60f, 0.00f, 1.00f).ToSystem();
            colors[(int)ImGuiCol.TextSelectedBg]        = new Color4(0.26f, 0.59f, 0.98f, 0.35f).ToSystem();
            colors[(int)ImGuiCol.DragDropTarget]        = new Color4(1.00f, 1.00f, 0.00f, 0.90f).ToSystem();
            colors[(int)ImGuiCol.NavHighlight]          = new Color4(0.41f, 0.41f, 0.41f, 1.00f).ToSystem();
            colors[(int)ImGuiCol.NavWindowingHighlight] = new Color4(1.00f, 1.00f, 1.00f, 0.70f).ToSystem();
            colors[(int)ImGuiCol.NavWindowingDimBg]     = new Color4(0.80f, 0.80f, 0.80f, 0.20f).ToSystem();
            colors[(int)ImGuiCol.ModalWindowDimBg]      = new Color4(0.80f, 0.80f, 0.80f, 0.35f).ToSystem();
        }

        public static void DarkTheme()
        {
            var style = ImGui.GetStyle();
            style.GrabRounding = style.FrameRounding = 2.3f;

            style.Colors[(int)ImGuiCol.Separator]             = style.Colors[(int)ImGuiCol.Border];
            style.Colors[(int)ImGuiCol.Text]                  = new Color4(1.00f, 1.00f, 1.00f, 1.00f).ToSystem();
            style.Colors[(int)ImGuiCol.TextDisabled]          = new Color4(0.50f, 0.50f, 0.50f, 1.00f).ToSystem();
            style.Colors[(int)ImGuiCol.WindowBg]              = new Color4(0.13f, 0.14f, 0.15f, 1.00f).ToSystem();
            style.Colors[(int)ImGuiCol.ChildBg]               = new Color4(0.13f, 0.14f, 0.15f, 1.00f).ToSystem();
            style.Colors[(int)ImGuiCol.PopupBg]               = new Color4(0.13f, 0.14f, 0.15f, 1.00f).ToSystem();
            style.Colors[(int)ImGuiCol.Border]                = new Color4(0.43f, 0.43f, 0.50f, 0.50f).ToSystem();
            style.Colors[(int)ImGuiCol.BorderShadow]          = new Color4(0.00f, 0.00f, 0.00f, 0.00f).ToSystem();
            style.Colors[(int)ImGuiCol.FrameBg]               = new Color4(0.25f, 0.25f, 0.25f, 1.00f).ToSystem();
            style.Colors[(int)ImGuiCol.FrameBgHovered]        = new Color4(0.38f, 0.38f, 0.38f, 1.00f).ToSystem();
            style.Colors[(int)ImGuiCol.FrameBgActive]         = new Color4(0.67f, 0.67f, 0.67f, 0.39f).ToSystem();
            style.Colors[(int)ImGuiCol.TitleBg]               = new Color4(0.08f, 0.08f, 0.09f, 1.00f).ToSystem();
            style.Colors[(int)ImGuiCol.TitleBgActive]         = new Color4(0.08f, 0.08f, 0.09f, 1.00f).ToSystem();
            style.Colors[(int)ImGuiCol.TitleBgCollapsed]      = new Color4(0.00f, 0.00f, 0.00f, 0.51f).ToSystem();
            style.Colors[(int)ImGuiCol.MenuBarBg]             = new Color4(0.14f, 0.14f, 0.14f, 1.00f).ToSystem();
            style.Colors[(int)ImGuiCol.ScrollbarBg]           = new Color4(0.02f, 0.02f, 0.02f, 0.53f).ToSystem();
            style.Colors[(int)ImGuiCol.ScrollbarGrab]         = new Color4(0.31f, 0.31f, 0.31f, 1.00f).ToSystem();
            style.Colors[(int)ImGuiCol.ScrollbarGrabHovered]  = new Color4(0.41f, 0.41f, 0.41f, 1.00f).ToSystem();
            style.Colors[(int)ImGuiCol.ScrollbarGrabActive]   = new Color4(0.51f, 0.51f, 0.51f, 1.00f).ToSystem();
            style.Colors[(int)ImGuiCol.CheckMark]             = new Color4(0.11f, 0.64f, 0.92f, 1.00f).ToSystem();
            style.Colors[(int)ImGuiCol.SliderGrab]            = new Color4(0.11f, 0.64f, 0.92f, 1.00f).ToSystem();
            style.Colors[(int)ImGuiCol.SliderGrabActive]      = new Color4(0.08f, 0.50f, 0.72f, 1.00f).ToSystem();
            style.Colors[(int)ImGuiCol.Button]                = new Color4(0.25f, 0.25f, 0.25f, 1.00f).ToSystem();
            style.Colors[(int)ImGuiCol.ButtonHovered]         = new Color4(0.38f, 0.38f, 0.38f, 1.00f).ToSystem();
            style.Colors[(int)ImGuiCol.ButtonActive]          = new Color4(0.67f, 0.67f, 0.67f, 0.39f).ToSystem();
            style.Colors[(int)ImGuiCol.Header]                = new Color4(0.22f, 0.22f, 0.22f, 1.00f).ToSystem();
            style.Colors[(int)ImGuiCol.HeaderHovered]         = new Color4(0.25f, 0.25f, 0.25f, 1.00f).ToSystem();
            style.Colors[(int)ImGuiCol.HeaderActive]          = new Color4(0.67f, 0.67f, 0.67f, 0.39f).ToSystem();
            style.Colors[(int)ImGuiCol.SeparatorHovered]      = new Color4(0.41f, 0.42f, 0.44f, 1.00f).ToSystem();
            style.Colors[(int)ImGuiCol.SeparatorActive]       = new Color4(0.26f, 0.59f, 0.98f, 0.95f).ToSystem();
            style.Colors[(int)ImGuiCol.ResizeGrip]            = new Color4(0.00f, 0.00f, 0.00f, 0.00f).ToSystem();
            style.Colors[(int)ImGuiCol.ResizeGripHovered]     = new Color4(0.29f, 0.30f, 0.31f, 0.67f).ToSystem();
            style.Colors[(int)ImGuiCol.ResizeGripActive]      = new Color4(0.26f, 0.59f, 0.98f, 0.95f).ToSystem();
            style.Colors[(int)ImGuiCol.Tab]                   = new Color4(0.08f, 0.08f, 0.09f, 0.83f).ToSystem();
            style.Colors[(int)ImGuiCol.TabHovered]            = new Color4(0.33f, 0.34f, 0.36f, 0.83f).ToSystem();
            style.Colors[(int)ImGuiCol.TabActive]             = new Color4(0.23f, 0.23f, 0.24f, 1.00f).ToSystem();
            style.Colors[(int)ImGuiCol.TabUnfocused]          = new Color4(0.08f, 0.08f, 0.09f, 1.00f).ToSystem();
            style.Colors[(int)ImGuiCol.TabUnfocusedActive]    = new Color4(0.13f, 0.14f, 0.15f, 1.00f).ToSystem();
            style.Colors[(int)ImGuiCol.DockingPreview]        = new Color4(0.26f, 0.59f, 0.98f, 0.70f).ToSystem();
            style.Colors[(int)ImGuiCol.DockingEmptyBg]        = new Color4(0.20f, 0.20f, 0.20f, 1.00f).ToSystem();
            style.Colors[(int)ImGuiCol.PlotLines]             = new Color4(0.61f, 0.61f, 0.61f, 1.00f).ToSystem();
            style.Colors[(int)ImGuiCol.PlotLinesHovered]      = new Color4(1.00f, 0.43f, 0.35f, 1.00f).ToSystem();
            style.Colors[(int)ImGuiCol.PlotHistogram]         = new Color4(0.90f, 0.70f, 0.00f, 1.00f).ToSystem();
            style.Colors[(int)ImGuiCol.PlotHistogramHovered]  = new Color4(1.00f, 0.60f, 0.00f, 1.00f).ToSystem();
            style.Colors[(int)ImGuiCol.TextSelectedBg]        = new Color4(0.26f, 0.59f, 0.98f, 0.35f).ToSystem();
            style.Colors[(int)ImGuiCol.DragDropTarget]        = new Color4(0.11f, 0.64f, 0.92f, 1.00f).ToSystem();
            style.Colors[(int)ImGuiCol.NavHighlight]          = new Color4(0.26f, 0.59f, 0.98f, 1.00f).ToSystem();
            style.Colors[(int)ImGuiCol.NavWindowingHighlight] = new Color4(1.00f, 1.00f, 1.00f, 0.70f).ToSystem();
            style.Colors[(int)ImGuiCol.NavWindowingDimBg]     = new Color4(0.80f, 0.80f, 0.80f, 0.20f).ToSystem();
            style.Colors[(int)ImGuiCol.ModalWindowDimBg]      = new Color4(0.80f, 0.80f, 0.80f, 0.35f).ToSystem();
        }

        /// <summary>
        /// Renders the ImGui draw list data.
        /// This method requires a <see cref="GraphicsDevice"/> because it may create new DeviceBuffers if the size of vertex
        /// or index data has increased beyond the capacity of the existing buffers.
        /// A <see cref="CommandList"/> is needed to submit drawing and resource update commands.
        /// </summary>
        public void Render()
        {
            if (_frameBegun)
            {
                _frameBegun = false;
                ImGui.Render();
                RenderImDrawData(ImGui.GetDrawData());
            }
        }

        /// <summary>
        /// Updates ImGui input and IO configuration state.
        /// </summary>
        public void Update(GameWindow wnd, float deltaSeconds)
        {
            if (_frameBegun)
            {
                ImGui.Render();
            }

            SetPerFrameImGuiData(deltaSeconds);
            UpdateImGuiInput(wnd);

            _frameBegun = true;
            ImGui.NewFrame();
        }

        /// <summary>
        /// Sets per-frame data based on the associated window.
        /// This is called by Update(float).
        /// </summary>
        private void SetPerFrameImGuiData(float deltaSeconds)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.DisplaySize = new System.Numerics.Vector2(
                _windowWidth / _scaleFactor.X,
                _windowHeight / _scaleFactor.Y);
            io.DisplayFramebufferScale = _scaleFactor;
            io.DeltaTime = deltaSeconds; // DeltaTime is in seconds.
        }

        readonly List<char> PressedChars = new List<char>();

        private void UpdateImGuiInput(GameWindow wnd)
        {
            ImGuiIOPtr io = ImGui.GetIO();

            MouseState MouseState = wnd.MouseState;
            KeyboardState KeyboardState = wnd.KeyboardState;

            io.MouseDown[0] = MouseState[MouseButton.Left];
            io.MouseDown[1] = MouseState[MouseButton.Right];
            io.MouseDown[2] = MouseState[MouseButton.Middle];

            var screenPoint = new Vector2i((int)MouseState.X, (int)MouseState.Y);
            var point = screenPoint;//wnd.PointToClient(screenPoint);
            io.MousePos = new System.Numerics.Vector2(point.X, point.Y);
            
            foreach (Keys key in Enum.GetValues(typeof(Keys)))
            {
                if (key == Keys.Unknown)
                {
                    continue;
                }
                io.KeysDown[(int)key] = KeyboardState.IsKeyDown(key);
            }

            foreach (var c in PressedChars)
            {
                io.AddInputCharacter(c);
            }
            PressedChars.Clear();

            io.KeyCtrl = KeyboardState.IsKeyDown(Keys.LeftControl) || KeyboardState.IsKeyDown(Keys.RightControl);
            io.KeyAlt = KeyboardState.IsKeyDown(Keys.LeftAlt) || KeyboardState.IsKeyDown(Keys.RightAlt);
            io.KeyShift = KeyboardState.IsKeyDown(Keys.LeftShift) || KeyboardState.IsKeyDown(Keys.RightShift);
            io.KeySuper = KeyboardState.IsKeyDown(Keys.LeftSuper) || KeyboardState.IsKeyDown(Keys.RightSuper);
        }

        internal void PressChar(char keyChar)
        {
            PressedChars.Add(keyChar);
        }

        internal void MouseScroll(Vector2 offset)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            
            io.MouseWheel = offset.Y;
            io.MouseWheelH = offset.X;
        }

        private static void SetKeyMappings()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.KeyMap[(int)ImGuiKey.Tab] = (int)Keys.Tab;
            io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)Keys.Left;
            io.KeyMap[(int)ImGuiKey.RightArrow] = (int)Keys.Right;
            io.KeyMap[(int)ImGuiKey.UpArrow] = (int)Keys.Up;
            io.KeyMap[(int)ImGuiKey.DownArrow] = (int)Keys.Down;
            io.KeyMap[(int)ImGuiKey.PageUp] = (int)Keys.PageUp;
            io.KeyMap[(int)ImGuiKey.PageDown] = (int)Keys.PageDown;
            io.KeyMap[(int)ImGuiKey.Home] = (int)Keys.Home;
            io.KeyMap[(int)ImGuiKey.End] = (int)Keys.End;
            io.KeyMap[(int)ImGuiKey.Delete] = (int)Keys.Delete;
            io.KeyMap[(int)ImGuiKey.Backspace] = (int)Keys.Backspace;
            io.KeyMap[(int)ImGuiKey.Enter] = (int)Keys.Enter;
            io.KeyMap[(int)ImGuiKey.Escape] = (int)Keys.Escape;
            io.KeyMap[(int)ImGuiKey.A] = (int)Keys.A;
            io.KeyMap[(int)ImGuiKey.C] = (int)Keys.C;
            io.KeyMap[(int)ImGuiKey.V] = (int)Keys.V;
            io.KeyMap[(int)ImGuiKey.X] = (int)Keys.X;
            io.KeyMap[(int)ImGuiKey.Y] = (int)Keys.Y;
            io.KeyMap[(int)ImGuiKey.Z] = (int)Keys.Z;
        }

        private void RenderImDrawData(ImDrawDataPtr draw_data)
        {
            if (draw_data.CmdListsCount == 0)
            {
                return;
            }

            for (int i = 0; i < draw_data.CmdListsCount; i++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdListsRange[i];

                int vertexSize = cmd_list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>();
                if (vertexSize > _vertexBufferSize)
                {
                    int newSize = (int)Math.Max(_vertexBufferSize * 1.5f, vertexSize);
                    GL.NamedBufferData(_vertexBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                    _vertexBufferSize = newSize;

                    Console.WriteLine($"Resized dear imgui vertex buffer to new size {_vertexBufferSize}");
                }

                int indexSize = cmd_list.IdxBuffer.Size * sizeof(ushort);
                if (indexSize > _indexBufferSize)
                {
                    int newSize = (int)Math.Max(_indexBufferSize * 1.5f, indexSize);
                    GL.NamedBufferData(_indexBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                    _indexBufferSize = newSize;

                    Console.WriteLine($"Resized dear imgui index buffer to new size {_indexBufferSize}");
                }
            }

            // Setup orthographic projection matrix into our constant buffer
            ImGuiIOPtr io = ImGui.GetIO();
            Matrix4 mvp = Matrix4.CreateOrthographicOffCenter(
                0.0f,
                io.DisplaySize.X,
                io.DisplaySize.Y,
                0.0f,
                -1.0f,
                1.0f);

            _shader.UseShader();
            GL.UniformMatrix4(_shader.GetUniformLocation("projection_matrix"), false, ref mvp);
            GL.Uniform1(_shader.GetUniformLocation("in_fontTexture"), 0);
            Util.CheckGLError("Projection");

            GL.BindVertexArray(_vertexArray);
            Util.CheckGLError("VAO");

            draw_data.ScaleClipRects(io.DisplayFramebufferScale);

            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.ScissorTest);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);

            // Render command lists
            for (int n = 0; n < draw_data.CmdListsCount; n++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdListsRange[n];

                GL.NamedBufferSubData(_vertexBuffer, IntPtr.Zero, cmd_list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>(), cmd_list.VtxBuffer.Data);
                Util.CheckGLError($"Data Vert {n}");

                GL.NamedBufferSubData(_indexBuffer, IntPtr.Zero, cmd_list.IdxBuffer.Size * sizeof(ushort), cmd_list.IdxBuffer.Data);
                Util.CheckGLError($"Data Idx {n}");

                int vtx_offset = 0;
                int idx_offset = 0;

                for (int cmd_i = 0; cmd_i < cmd_list.CmdBuffer.Size; cmd_i++)
                {
                    ImDrawCmdPtr pcmd = cmd_list.CmdBuffer[cmd_i];
                    if (pcmd.UserCallback != IntPtr.Zero)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        GL.ActiveTexture(TextureUnit.Texture0);
                        GL.BindTexture(TextureTarget.Texture2D, (int)pcmd.TextureId);
                        Util.CheckGLError("Texture");

                        // We do _windowHeight - (int)clip.W instead of (int)clip.Y because gl has flipped Y when it comes to these coordinates
                        var clip = pcmd.ClipRect;
                        GL.Scissor((int)clip.X, _windowHeight - (int)clip.W, (int)(clip.Z - clip.X), (int)(clip.W - clip.Y));
                        Util.CheckGLError("Scissor");

                        if ((io.BackendFlags & ImGuiBackendFlags.RendererHasVtxOffset) != 0)
                        {
                            GL.DrawElementsBaseVertex(PrimitiveType.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (IntPtr)(idx_offset * sizeof(ushort)), vtx_offset);
                        }
                        else
                        {
                            GL.DrawElements(BeginMode.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (int)pcmd.IdxOffset * sizeof(ushort));
                        }
                        Util.CheckGLError("Draw");
                    }

                    idx_offset += (int)pcmd.ElemCount;
                }
                vtx_offset += cmd_list.VtxBuffer.Size;
            }

            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.ScissorTest);
        }

        /// <summary>
        /// Frees all graphics resources used by the renderer.
        /// </summary>
        public void Dispose()
        {
            _fontTexture.Dispose();
            _shader.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
