using ImGuiNET;
using ZargoEngine.Editor;
using System.Numerics;
using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace ZargoEngine.Rendering
{
    using OTkVec3 = OpenTK.Mathematics.Vector3;

    public enum TonemappingMode : byte
    { 
        aces, filmic, lottes, reinhard, reinhard2, uchimura, uncharted2, unreal, AMDTonemapper, DX11DSK, none
    }

    public class RenderHandeller : EditorWindow
    {
        public static RenderHandeller instance;
        public Sun sun;
        public Camera camera;
        public Vector3 ambientColor = new Vector3(1, 1, 1);
        public float ambientStrength = .25f;
        public float gamma = 2.2f;
        public float saturation = 1.2f;
        [Editor.Attributes.EnumField("Tonemap Mode")]
        public TonemappingMode tonemappingMode = TonemappingMode.uchimura;

        public List<Light> lights = new List<Light>();

        public RenderHandeller(Camera camera)
        {
            instance = this;
            title = "Render Handeller";
            sun = new Sun(1.2f, -60, new Vector4(1, 1, .98f, 1));
            this.camera = camera;
        }

        public OTkVec3 GetSunDirection()
        {
            return new OTkVec3(0, -MathF.Sin(sun.angle), -MathF.Cos(sun.angle));
        }

        public OTkVec3 GetSunPosition()
        {
            return new OTkVec3(0, MathF.Sin(sun.angle) * 700, MathF.Cos(sun.angle) * 700);
        }

        protected override void OnGUI()
        {
            GUI.HeaderIn("Render settings");
            GUI.EnumField(ref tonemappingMode, "Tonemap Mode");
            ImGui.ColorEdit3(nameof(ambientColor), ref ambientColor, ImGuiColorEditFlags.NoAlpha);
            ImGui.DragFloat(nameof(ambientStrength), ref ambientStrength, Component.ImguiDragSpeed);
            GUI.FloatField(ref saturation, nameof(saturation));
            
            sun.DrawWindow();

            Renderer3D.DrawShadowSettings();
        }
    }

    [Serializable]
    public class Sun
    {
        public float angleDegree;
        [XmlIgnore]
        public float angle => OpenTK.Mathematics.MathHelper.DegreesToRadians(angleDegree);
        public float intensity = 1.2f;
        public Vector4 sunColor = Vector4.One;

        public Sun(in float angle, in float intensity, in Vector4 color)
        {
            this.angleDegree = angle;
            this.intensity = intensity;
            this.sunColor = color;
        }
        internal void DrawWindow()
        {
            ImGui.Text(nameof(Sun));
            GUI.FloatField(ref angleDegree, nameof(angle), ValueChanged);
            ImGui.DragFloat(nameof(intensity), ref intensity, 0.02f);
            ImGui.ColorEdit4(nameof(sunColor), ref sunColor, ImGuiColorEditFlags.None);
        }

        private void ValueChanged()
        {
            Renderer3D.UpdateShadows();
        }

        internal Sun() { }

    }

    [Serializable]
    [XmlRoot("RenderSaveData")]
    public class RenderSaveData
    {
        public Sun sun;
        public Vector3 ambientColor;
        public float ambientStrength = .5f;

        public void Apply(in RenderHandeller renderHandeller)
        {
            renderHandeller.sun = sun;
            renderHandeller.ambientColor    =  ambientColor   ;
            renderHandeller.ambientStrength =  ambientStrength;
        }

        public RenderSaveData(RenderHandeller renderHandeller)
        {
            sun = renderHandeller.sun;
            ambientColor    = renderHandeller.ambientColor;
            ambientStrength = renderHandeller.ambientStrength;
        }

        internal RenderSaveData() { }
    }

}
