
using ImGuiNET;
using System;

namespace ZargoEngine.Editor
{
    public abstract class EditorWindow : IDrawable
    {
        public bool Hovered { get; protected set; }
        public bool Focused { get; protected set; }

        public bool windowActive = false;
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
    }

    public class TempraryWindow : IDrawable, IDisposable
    {
        private bool active;
        private readonly string title;
        private readonly Func<bool> onGuı;

        public TempraryWindow(string title, Func<bool> onGuı) {
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
            if (ImGui.Begin(title, ref active, ImGuiWindowFlags.None)) 
            {
                if (onGuı()) Dispose();

                ImGui.End();
            }
            else {
                Dispose();
            }
        }

        public void Dispose()
        {
            active = false;
            //Engine.RemoveWindow(this);
            GC.SuppressFinalize(this);
        }
    }

}
