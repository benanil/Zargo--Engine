
using ImGuiNET;

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
    }
}
