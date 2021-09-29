// this code manages all of the game codes main loops are here
// render management also here 

// this define symbol will be removed at game release
#define Editor 
#nullable disable warnings

// OpenTK
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
// external
using Coroutine;
using Dear_ImGui_Sample;
using System;
using ImGuiNET;
using System.Windows.Forms;
using ZargoEngine.AnilTools;

namespace ZargoEngine
{
    // Zargo
    using Rendering;
    using Editor;
    using Analysis;
    using ZargoEngine.Media.Sound;
    using Physics;
    using Shader  = Rendering.Shader;
    using Texture = Rendering.Texture;
    using static SystemInformation;
    using System.Collections.Generic;
    using System.IO;
    using Keys = OpenTK.Windowing.GraphicsLibraryFramework.Keys;

    public class Engine : GameWindow
    {
        public static bool IsEditor;
        private static List<IDrawable> editorWindows;
        public static void AddWindow(IDrawable drawable) => editorWindows.Add(drawable);
        public static void RemoveWindow(IDrawable drawable) => editorWindows.Remove(drawable);

        private ImGuiController _Imguicontroller;

        private RenderConfig renderHandeller;
        private SceneViewWindow GameViewWindow;
        private Skybox skybox;

        // rendering
        public FrameBuffer SceneFrameBuffer;
        public FrameBuffer ScreenFrameBuffer;// for sceneWiew
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
            new Camera(new Vector3(0, 0, 1), ClientRectangle.Size.X / ClientRectangle.Size.Y, -Vector3.UnitZ);
            renderHandeller = new RenderConfig();

            if (!IsEditor)
            {
                // todo: load last scene player played
                SceneManager.LoadScene(0);
            }

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
            editorWindows = new List<IDrawable>
            {
                new Inspector(),
                new Hierarchy(),
                new Profiler(),
                new EditorResources(),
                new SettingsWindow()
            };
            
            _Imguicontroller    = new ImGuiController(ClientSize.X, ClientSize.Y);
            GameViewWindow = new SceneViewWindow(this);
            
            SceneFrameBuffer  = new FrameBuffer(PrimaryMonitorMaximizedWindowSize.Width, PrimaryMonitorMaximizedWindowSize.Height, PixelInternalFormat.Rgb);
            ScreenFrameBuffer = new FrameBuffer(PrimaryMonitorMaximizedWindowSize.Width, PrimaryMonitorMaximizedWindowSize.Height, PixelInternalFormat.Rgb);

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
        
        string sceneName = string.Empty;

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            // actually we are requesting shadow calculation we are not calculating
            Shadow.CalculateShadows();

            // first pass render whole scene
            SceneFrameBuffer.Bind();
            {
                PrepareRenderingScene();

                skybox.Use(Camera.main);
                GL.Enable(EnableCap.DepthTest);

                Renderer3D.RenderMaterials(Camera.main);
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

                    GL.Uniform1(1, RenderConfig.gamma);
                    GL.Uniform1(2, RenderConfig.saturation);
                    GL.Uniform1(3, (int)RenderConfig.tonemappingMode);
                    
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


            if (SceneManager.currentScene != null)
            {
                _Imguicontroller.GenerateDockspace(EditorWindow);
            }
            else if (IsEditor)
            {
                ImGui.Begin("Welcome");

                if (!Directory.Exists(AssetManager.AssetsPath + "Scenes")) Directory.CreateDirectory(AssetManager.AssetsPath + "Scenes");
                string[] scenes = Directory.GetFiles(AssetManager.AssetsPath + "Scenes");
                
                for (ushort i = 0; i < scenes.Length; i++)
                {
                    if (ImGui.Button(Path.GetFileNameWithoutExtension(scenes[i])))
                    {
                        SceneManager.LoadScene(scenes[i]);
                        AssimpImporter.ImportAssimpScene(AssetManager.AssetsPath + "Models/sponza.obj");
                    }
                    ImGui.SameLine();
                    ImGui.PushStyleColor(ImGuiCol.Button, new System.Numerics.Vector4(1, 0, 0, 1));
                    if (ImGui.Button("X"))
                    {
                        if (MessageBox.Show("are you sure to delete it ?", "Warning", MessageBoxButtons.OKCancel) == DialogResult.OK)
                        {
                            File.Delete(scenes[i]);
                        }
                    }
                    ImGui.PopStyleColor();
                }

                GUI.TextField(ref sceneName, "SceneName");
                ImGui.SameLine();
            
                if (ImGui.Button("Create New Scene"))
                {
                    new Scene(RenderSaveData.Default, sceneName);// this line also adds scene to scene manager
                }
            
                ImGui.Separator();
            
                ImGui.Text("       welcome to Zargo Engine!                  \n" +
                           "f5- Start Scene & stop scene                     \n" +
                           "right click material and scene for open          \n" +
                           "drag and drop models to the scene for render them\n" +
                           "drag drop models to the engine for adding them       ");
                
                ImGui.End();
            }

            _Imguicontroller.Render();

            ImGui.UpdatePlatformWindows();
            ImGui.RenderPlatformWindowsDefault();

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
            foreach (var window in editorWindows)
            {
                window.DrawWindow();
            }
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

            SceneManager.currentScene?.Update();
            MainInput();
        }

        private Vector2i oldPosition;

        public static event PositionChanged OnWindowPositionChanged = delegate { };
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
                Camera.SceneCamera.AspectRatio = AspectRatio;
            }
            GL.Viewport(0, 0, e.Width, e.Height);
            OnWindowScaleChanged(e);
            base.OnResize(e);
            _Imguicontroller?.WindowResized(e.Width, e.Height);
        }

        private unsafe void MainInput()
        {
            if (IsKeyPressed(Keys.F11)) Screen.FullScreen = !Screen.FullScreen; // full screen    

            if (!IsEditor) return;

            if (IsKeyDown(Keys.LeftControl) && IsKeyPressed(Keys.Z)) {
                Undo.undo();
            }

            if (IsKeyDown(Keys.LeftControl) && IsKeyPressed(Keys.Y)) {
                Undo.Redo();
            }

            if (IsKeyDown(Keys.Delete)) { Inspector.TryDelete(); }
            if (IsKeyReleased(Keys.Enter) || IsKeyReleased(Keys.KeyPadEnter)) LogGame();
            if (IsKeyDown(Keys.LeftControl) && IsKeyPressed(Keys.G)) {
                Console.Clear();
            }
        }

        public FrameBuffer GetFrameBuffer()
        {
            return ScreenFrameBuffer;
        }

        public static void LogGame() {
            GC.Collect();
        }


        protected override void OnFocusedChanged(FocusedChangedEventArgs e)
        {
            base.OnFocusedChanged(e);
            Debug.LogWarning("Focus Changed");
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
            var result = MessageBox.Show("do you want to close, wana save?", "Warning", MessageBoxButtons.YesNoCancel);
            if (result == DialogResult.Cancel) {
                e.Cancel = true;
            }
            else if (result == DialogResult.Yes){
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
