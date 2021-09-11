using ImGuiNET;
using System.Numerics;
using System;
using ImGuizmoNET;
using OpenTK.Windowing.GraphicsLibraryFramework;
using ZargoEngine.Rendering;

#nullable disable warnings

namespace ZargoEngine.Editor
{
    using static ZargoEngine.Delegates;
    
    public class GameViewWindow 
    {
        public const float MaxFrameBufferSize = 8024;

        public static GameViewWindow instance { get; private set; }

        public bool Hovered { get; private set; }
        public bool Focused { get; private set; }

        public static float Width => PanelSize.X;
        public static float Height => PanelSize.Y;
        /// <summary>normalized </summary>
        public static Vector2 MousePosScaler { get => mousePosScaler; set => mousePosScaler = value; }

        private bool isOpen = true;

        private readonly Engine window;

        OPERATION operation = OPERATION.TRANSLATE;

        public static Vector2 PanelSize { get; private set; }
        public static Vector2 PanelPosition
        {
            get => panelOldPosition;
            set { 
                if (value != panelOldPosition)  OnPositionChanged(PanelPosition);
                panelOldPosition = value;
            }
        }
        private static Vector2 panelOldPosition;

        private static Vector2 mousePosScaler;

        public static event SysValueChanged2 OnScaleChanged = (scale) => { };
        public static event SysValueChanged2 OnPositionChanged = (scale) => { };

        public GameViewWindow(Engine window)
        {
            instance = this;
            this.window = window;
            OnScaleChanged(PanelSize); 
        }

        public unsafe void Render()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
            ImGui.Begin("Game Window", ref isOpen, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

            Hovered = ImGui.IsWindowHovered();
            Focused = ImGui.IsWindowFocused();

            PanelPosition = ImGui.GetWindowPos();

            // game wiew window scale
            var contentSize = ImGui.GetContentRegionAvail();
                                                          // magnitude                    // magnitude
            if (PanelSize != contentSize &&  contentSize.X + contentSize.Y > 2 && contentSize.X + contentSize.Y < MaxFrameBufferSize)
            {
                PanelSize = contentSize;
                window.GetFrameBuffer().Invalidate((int)contentSize.X,(int)contentSize.Y);
                Camera.main.AspectRatio = contentSize.X / contentSize.Y; // we need to change aspect ratio when resizing
                OnScaleChanged(PanelSize);
            }

            mousePosScaler = new Vector2(Screen.GetMainWindowSizeTupple().Item1, Screen.GetMainWindowSizeTupple().Item2) / ImGui.GetWindowSize();

            // this is the Viewport we bind framebuffers texture here
            ImGui.Image((IntPtr)window.GetFrameBuffer().GetTextureId(), PanelSize, Vector2.UnitY, Vector2.UnitX);

            GUI.DropUIElementString(EditorResources.MODEL, (file) => AssetImporter.LoadFileToScene(file));

            #region gizmo stuff
            if (Inspector.currentObject != null && Hovered && Focused)
            {
                if (Inspector.currentObject is GameObject go)
                {
                    var camera = SceneManager.currentScene.renderHandeller.camera;

                    ImGuizmo.Enable(true);
                    ImGuizmo.SetOrthographic(false);
                    ImGuizmo.SetDrawlist();
                    ImGuizmo.SetRect(ImGui.GetWindowPos().X, ImGui.GetWindowPos().Y, PanelSize.X, PanelSize.Y);
            
                    if (Hovered)
                    {
                        if (Input.GetKeyDown(Keys.W)) operation = OPERATION.TRANSLATE;
                        if (Input.GetKeyDown(Keys.Q)) operation = OPERATION.ROTATE;
                        if (Input.GetKeyDown(Keys.R)) operation = OPERATION.SCALE;
                    }

                    // manipulate thing sonunda düzelttim amına koyiym
                    ImGuizmo.Manipulate(ref camera.ViewMatrix.Row0.X, ref camera.projectionMatrix.Row0.X, operation, MODE.LOCAL, ref go.transform.Translation.Row0.X);

                    if (ImGuizmo.IsUsing())
                    {
                        go.transform.SetPosition (go.transform.Translation.ExtractTranslation(), false);
                        go.transform.SetQuaterion(go.transform.Translation.ExtractRotation(), false);
                        go.transform.SetScale    (go.transform.Translation.ExtractScale(), false);

                        // rotating in gizmo is not working for now 
                        go.transform.UpdateTranslation();
                    }
                }
            }
            #endregion

            ImGui.End();

            ImGui.PopStyleVar();// window padding
        }
    }
}