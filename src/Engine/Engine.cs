// this code manages all of the game codes main loops are here
// render management also here 

// this define symbol will be removed at game release
#define Editor 
#nullable disable warnings

// OpenTK
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
// external
using Coroutine;
using Dear_ImGui_Sample;
using System;

namespace ZargoEngine
{
    // Zargo
    using ZargoEngine.Rendering;
    using ZargoEngine.Editor;
    using ZargoEngine.Analysis;
    using ZargoEngine.Helper;
    using ZargoEngine.AnilTools;
    using ZargoEngine.Media.Sound;
    using ZargoEngine.Physics;
    using Shader  = Rendering.Shader;
    using Texture = Rendering.Texture;
    using static System.Windows.Forms.SystemInformation;

    public class Engine : GameWindow
    {
        public static Engine instance;
        public static bool IsEditor;
        private static EditorWindow[] editorWindows;
        private ImGuiController _Imguicontroller;
        private Camera camera;

        private RenderHandeller renderHandeller;
        private GameViewWindow GameViewWindow;
        private Skybox skybox;
        public GameObject firstObject;

        // rendering
        public FrameBuffer SceneFrameBuffer;
        public FrameBuffer ScreenFrameBuffer;

        private Shader PostProcessingShader;

        public Action<ResizeEventArgs> OnWindowScaleChanged = (scale) => { };

        private float _aspectRatio = 16 / 9;
        public float AspectRatio
        {
            get
            {
                if (ClientRectangle.Size.X > 0 && ClientRectangle.Size.Y > 0)
                    _aspectRatio = ClientRectangle.Size.X / ClientRectangle.Size.Y;

                return _aspectRatio;
            }
        }

        public Engine(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings) {}

        protected override void OnLoad()
        {
            base.OnLoad();
            GL.ClearColor(Color4.Gray);

#if Editor
            IsEditor = true;
#else
            IsEditor = false;
#endif
            LoadScene();
            LoadGUI();
        }

        private int screenVao, screenVbo;

        private void LoadScene()
        {
            instance = this;

            AssetManager.LoadDefaults();

            PostProcessingShader = AssetManager.GetShader("Shaders/PostProcessing.vert", "Shaders/PostProcessing.glsl");

            // initialize screen buffer
            { 
                float[] VertexData =
                {
                   // positions    // texture Coords
                    -1.0f,  1.0f   , 0.0f, 1.0f,
                    -1.0f, -1.0f   , 0.0f, 0.0f,
                     1.0f,  1.0f   , 1.0f, 1.0f,
                     1.0f, -1.0f   , 1.0f, 0.0f
                };
            
                screenVao = GL.GenVertexArray();
                GL.BindVertexArray(screenVao);
            
                screenVbo = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, screenVbo);
                GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 16, VertexData, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, sizeof(float) * 4, IntPtr.Zero);
            }

            new AudioContext();

            skybox = new Skybox();
            camera = new Camera(new Vector3(0, 0, 1), ClientRectangle.Size.X / ClientRectangle.Size.Y, -Vector3.UnitZ);

            renderHandeller = new RenderHandeller(camera);

            var scene = new Scene(renderHandeller, "first scene");  

            SceneManager.AddScene(scene);
            SceneManager.LoadScene(0);

            // for now we are testing animation stuff
            // GameObject atillaGO    = new GameObject("Animated tnim matrix");
            // 
            // AssimpImporter.LoadVladh(AssetManager.AssetsPath + "Models/Animated_Model.dae", out SkinnedMesh atillaMesh, out Animator animation);
            // animation.AsignGameObject(atillaGO);
            // 
            // var material = new Material(AssetManager.GetShader("Shaders/Skinned.vert", "Shaders/Basic.frag"));
            // new SkinnedMeshRenderer(atillaMesh, atillaGO, animation, material);
        }

        private unsafe void LoadGUI()
        {
            editorWindows = new EditorWindow[]
            {
                new Inspector(),
                new Hierarchy(),
                new Profiler(),
                new EditorResources(),
                new SettingsWindow()
            };
            
            _Imguicontroller    = new ImGuiController(ClientSize.X, ClientSize.Y);
            GameViewWindow = new GameViewWindow(this);
            
            SceneFrameBuffer  = new FrameBuffer(PrimaryMonitorMaximizedWindowSize.Width, PrimaryMonitorMaximizedWindowSize.Height, PixelInternalFormat.Rgba16f);
            ScreenFrameBuffer = new FrameBuffer(PrimaryMonitorMaximizedWindowSize.Width, PrimaryMonitorMaximizedWindowSize.Height);

            EditorWindowSaving.Load();
        }


        protected override void OnTextInput(TextInputEventArgs e)
        {
            base.OnTextInput(e);
            _Imguicontroller.PressChar(e.AsString[0]);
        }

        private static void PrepareRenderingScene()
        { 
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            //GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
        }

        private void PrepareScreenQuad()
        { 
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            GL.Disable(EnableCap.DepthTest);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            // actually we are requesting shadow calculation we are not calculating
            Renderer3D.CalculateShadows();

            // first pass render whole scene
            SceneFrameBuffer.Bind();
            {
                PrepareRenderingScene();

                skybox.Use(camera);
                GL.Enable(EnableCap.DepthTest);

                Renderer3D.RenderMaterials(renderHandeller);
                SceneManager.currentScene?.Render();
                Gizmos.Render();
                OnHud.Invoke();
            }
            SceneFrameBuffer.unbind();

            // post processing effects
            ScreenFrameBuffer.Bind();
            {
                PostProcessingShader.Use();
                {
                    //prepare
                    PrepareScreenQuad();
                    GL.BindVertexArray(screenVao);
                    GL.EnableVertexAttribArray(0);

                    GL.Uniform1(1, renderHandeller.gamma);
                    GL.Uniform1(2, renderHandeller.saturation);
                    GL.Uniform1(3, (int)renderHandeller.tonemappingMode);
                    
                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, SceneFrameBuffer.texID);
                    
                    // draw
                    GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

                    //dispose
                    Texture.UnBind();
                    GL.DisableVertexAttribArray(0);
                    GL.BindVertexArray(0);
                }
                PostProcessingShader.Detach();
            }
            ScreenFrameBuffer.unbind();

            SceneManager.currentScene?.PhysicsUpdate();

            // frame buffer changes the Viewport size(smaller value) we need to fix it back
            GL.Viewport(0, 0, ClientRectangle.Size.X, ClientRectangle.Size.Y);

            _Imguicontroller.GenerateDockspace(EditorWindow);
            _Imguicontroller.Render();

            GL.Enable(EnableCap.DepthTest);

            GL.Flush();
            SwapBuffers();
            base.OnRenderFrame(args);
        }

        public static event Action OnHud = () => { };

        public void EditorWindow()
        {
            GameViewWindow.Render();
            renderHandeller.DrawWindow();
            editorWindows.Foreach(x => x.DrawWindow());
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            Time.Tick((float)args.Time);

            ZargoUpdate.Update();
            CoroutineHandler.Tick(args.Time);

            _Imguicontroller.Update(this, Time.DeltaTime);

            if (oldPosition != Bounds.Min) OnWindowPositionChanged(Bounds);
            oldPosition = Bounds.Min;

            SceneManager.currentScene.Update();
            MainInput();
        }

        private Vector2i oldPosition;

        public static event PositionChanged OnWindowPositionChanged = delegate(in Box2i position) { };
        public delegate void PositionChanged(in Box2i position);

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            Input.ScrollY = e.OffsetY;
            CoroutineHandler.InvokeLater(new Wait(0.1f), () => Input.ScrollY = 0);
            _Imguicontroller.MouseScroll(e.Offset);
            base.OnMouseWheel(e);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            if (ClientRectangle.Size.X > 0 && ClientRectangle.Size.Y > 0) {
                camera.AspectRatio = AspectRatio;
            }
            GL.Viewport(0, 0, e.Width, e.Height);
            OnWindowScaleChanged(e);
            base.OnResize(e);
            _Imguicontroller?.WindowResized(e.Width, e.Height);
        }

        private unsafe void MainInput()
        {
            if (IsKeyReleased(Keys.Enter) || IsKeyReleased(Keys.KeyPadEnter)) LogGame();
            if (IsKeyPressed(Keys.F11)) Screen.FullScreen = !Screen.FullScreen; // full screen    
            if (IsKeyDown(Keys.LeftControl) && IsKeyPressed(Keys.G))
            {
                Console.Clear();
            }
        }

        public FrameBuffer GetFrameBuffer()
        {
            return ScreenFrameBuffer;
        }

        public static void LogGame() {
            Renderer3D.DebugMatrix();
            GC.Collect();
        }

        protected override void OnFileDrop(FileDropEventArgs e) {
            for (ushort i = 0; i < e.FileNames.Length; i++) 
            {
                AssetImporter.Import(e.FileNames[i]);
            }
            base.OnFileDrop(e);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            var result = System.Windows.Forms.MessageBox.Show("do you want to close, wana save?", "Warning", System.Windows.Forms.MessageBoxButtons.YesNoCancel);
            if (result == System.Windows.Forms.DialogResult.Cancel) {
                e.Cancel = true;
            }
            else if (result == System.Windows.Forms.DialogResult.Yes){
                SceneManager.currentScene.SaveScene();
            }

            base.OnClosing(e);
        }

        protected override void OnClosed()
        {
            SceneManager.Dispose();
            AssetManager.ClearAllAssets();
            EditorWindowSaving.Save();
            BepuHandle.Dispose();
        }
    }
}
