#nullable disable
using OpenTK.Mathematics;
using System;
using ZargoEngine.Bindings;
using ZargoEngine.Editor;

namespace ZargoEngine.UI
{
    using SysVec4 = System.Numerics.Vector4;
    public abstract class UIbase : Companent
    {
        static UIbase()
        {
            Program.MainGame.OnWindowScaleChanged += (args) =>
            {
                ScreenScale = args.Size;
                WindowBounds = Program.MainGame.Bounds;
                CalculateWindowBottom();
            };
            Engine.OnWindowPositionChanged += delegate (in Box2i bounds) 
            { 
                WindowBounds = bounds;
                CalculateWindowBottom();
            };
            ScreenScale = Program.MainGame.ClientSize;
        }

        public UIbase(GameObject go, string path) : base(go)
        {
            if (!CanInitialize(path)) return;
            InitializeEvents();
        }

        private void InitializeEvents()
        {
            void calculate() {
                CalculateBounds();
                CalculateWindowBottom();
            }
            Engine.OnHud += RenderHUD;
            Engine.OnWindowPositionChanged += delegate (in Box2i position) { calculate(); };
            SceneViewWindow.OnPositionChanged += (position) => { calculate(); ; };
            SceneViewWindow.OnScaleChanged += (position) => { calculate(); ; };
            transform.OnPositionChanged += PositionChanged;
        }

        public UIbase(GameObject go) : base(go)
        {
            go.AddComponent(this);

            InitializeEvents();
        }
        
        static void CalculateWindowBottom()
        {
            var clinetRectangle = Program.MainGame.ClientRectangle.Min;
            WindowBottom = new((int)SceneViewWindow.PanelPosition.X + WindowBounds.Min.X,
            Screen.MonitorHeight - (WindowBounds.Min.Y + (int)SceneViewWindow.Height + (int)SceneViewWindow.PanelPosition.Y + clinetRectangle.Y));
        }

        public Box2 bounds;
        public SysVec4 color = new(1, 1, 1, 1);

        /// <summary>bottom left corner of window</summary>
        private static Vector2i windowBottom;
        public static Vector2i WindowBottom
        {
            get => windowBottom;
            private set => windowBottom = value;
        }

        protected static Vector2 BoundsMultipler { get; } = new Vector2(10, 10); // 10px its helps for choosing easly
        protected static Vector2i ScreenScale { get; private set; }
        protected static Box2i WindowBounds { get; private set; }

        /// <summary>for mouse picking</summary>
        protected abstract void CalculateBounds();
        protected abstract void RenderHUD();

        protected virtual bool CanInitialize(in string path) => true;
        protected virtual void PositionChanged(ref Vector3 position) {
            CalculateBounds();
        }

        protected virtual void UIInput() {
            Hovered = CheckHovered();
            color = Hovered ? new SysVec4(0, 1, 0,1) : new SysVec4(1, 0, 0,1); // for debug
            
            if (Hovered)
            {
                if (Input.MouseButtonDown(OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Left)) {
                    OnMouseDown?.Invoke();
                }

                if (Input.MouseButtonUp(OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Left)) {
                    OnMouseUp?.Invoke();
                }
            }
        }

        private bool hovered;
        public bool Hovered
        {
            get => hovered;
            set
            {
                if (value != hovered) { // value changed
                    if (value) {
                        OnMouseEnter?.Invoke();
                    }
                    else {
                        OnMouseExit?.Invoke();
                    }
                }
                hovered = value;
            }
        }

        public event Action OnMouseDown;
        public event Action OnMouseUp;
        public event Action OnMouseEnter;
        public event Action OnMouseExit;

        protected virtual bool CheckHovered() {
            MouseBindings.GetCursorPos(out MouseBindings.POINT mousepoint);
            Vector2 mouseReversed = new Vector2(mousepoint.X, Screen.MonitorHeight - mousepoint.Y); // y ekseni aşşağı kısım 0 olacak hale getirme(normalde yukarı kısım 0)
            return bounds.Contains(mouseReversed);
        }

        public override void Update()
        {
            UIInput();
        }

        public override void DrawWindow()
        {
            GUI.Header(ref name);
            GUI.ColorEdit4(ref color, nameof(color), OnValidate);
        }
        
        public override void Dispose() {
            DeleteBuffers();
            GC.SuppressFinalize(this);
        }
    }
}
