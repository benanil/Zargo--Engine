
using ImGuiNET;
using ZargoEngine.Editor;

namespace ZargoEngine.Rendering
{
    public enum LightMode : byte { 
        point, spot = 60
    }

    public unsafe class Light : Component
    {
        public LightMode lightMode;
        /// <summary>if spot light this represents angle</summary>
        public float intensity = 0.09f;
        public System.Numerics.Vector3 color = new (1, 1,.8f);

        public override void DrawWindow()
        {
            ImGui.Text(name);

            GUI.EnumField(ref lightMode, nameof(LightMode), onSellect: OnLightModeChanged);
            GUI.ColorEdit3(ref color, nameof(color));

            GUI.FloatField(ref intensity, nameof(intensity));
        }

        private void OnLightModeChanged()
        { 
            if (lightMode == LightMode.point)
            {
                
            }
            else if (lightMode == LightMode.spot){
            
            }
        }

        public Light(GameObject gameObject) : base(gameObject)
        {
            name = "Light";
            RenderHandeller.instance.lights.Add(this);
        }
    }
}
