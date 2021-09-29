
using ImGuiNET;
using OpenTK.Mathematics;
using System;

namespace ZargoEngine.Editor
{
    public abstract class EditorWindow : IDrawable
    {
        public bool Hovered { get; protected set; }
        public bool Focused { get; protected set; }

        private protected bool windowActive = false;
        public string title = "Editor Window";
        protected ImGuiWindowFlags flags = ImGuiWindowFlags.None;

        public virtual void DrawWindow()
        {
            if (ImGui.Begin(title, ref windowActive, flags))
            {
                Hovered = ImGui.IsWindowHovered();
                Focused = ImGui.IsWindowFocused();
            
                OnGUI();
            }
            ImGui.End();
        }

        protected abstract void OnGUI();
        public void Dispose() { }
    }

    public class TempraryWindow : IDrawable, IDisposable
    {
        private bool active;
        private readonly string title;
        private readonly Func<object> onGuı;
        public Vector2i scale;
        public Action<Vector2i> windowScaleChanged;
        public Action<object> onClosed;
        public object closeObject;

        public TempraryWindow(string title, Func<object> onGuı)
        {
            this.active = true;
            this.title = title;
            this.onGuı = onGuı;

            Coroutine.CoroutineHandler.InvokeLater(new Coroutine.Wait(0.2f), () =>
            {
                Engine.AddWindow(this);
            });
        }

        public void DrawWindow()
        {
            Vector2i newScale = new Vector2i((int)ImGui.GetWindowWidth(), (int)ImGui.GetWindowHeight());
            
            if (scale != newScale) {
                windowScaleChanged?.Invoke(newScale);
                scale = newScale;
            }

            if (ImGui.Begin(title, ref active, ImGuiWindowFlags.None))
            {
                if ((closeObject = onGuı()) != null) {
                    Dispose();
                    return;
                }
                ImGui.End();
            }
            else
            {
                Dispose();
            }
        }

        public void Dispose()
        {
            ImGui.End();
            onClosed?.Invoke(closeObject);

            active = false;
            Coroutine.CoroutineHandler.InvokeLater(new Coroutine.Wait(0.2f), () =>
            {
                GC.SuppressFinalize(this);
                Engine.RemoveWindow(this);
            });
        }
    }

}
