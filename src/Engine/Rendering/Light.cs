
using ImGuiNET;
using ZargoEngine.Editor;
using ZargoEngine.Editor.Attributes;

namespace ZargoEngine.Rendering
{
    public enum LightMode : byte { 
        none, point, spot
    }

    public unsafe class Light : Companent
    {
        [EnumField("LightMode")]
        public LightMode lightMode = LightMode.point;
        /// <summary>if spot light this represents angle</summary>
        public float intensity = 0.09f;
        public System.Numerics.Vector3 color = new (1, 1,.8f);

        public override void DrawWindow()
        {
            if (!ImGui.CollapsingHeader(name, ImGuiTreeNodeFlags.CollapsingHeader)) return;
            ImGui.Text(name);

            GUI.EnumField(ref lightMode, nameof(LightMode));
            GUI.ColorEdit3(ref color, nameof(color));
            GUI.FloatField(ref intensity, nameof(intensity));
        }

        public Light(GameObject gameObject) : base(gameObject)
        {
            RenderConfig.lights.Add(this);
        }

        public override void Dispose()
        {
            RenderConfig.lights.Remove(this);
            base.Dispose();
        }

    }
}
