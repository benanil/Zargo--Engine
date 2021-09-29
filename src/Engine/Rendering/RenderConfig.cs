#pragma warning disable CA2211 // Non-constant fields should not be visible

using ImGuiNET;
using ZargoEngine.Editor;
using System.Numerics;
using System;
using System.Collections.Generic;

#nullable disable warnings
namespace ZargoEngine.Rendering
{
    using OTkVec3 = OpenTK.Mathematics.Vector3;

    public enum TonemappingMode : byte
    { 
        aces, filmic, lottes, reinhard, reinhard2, uchimura, uncharted2, unreal, AMDTonemapper, DX11DSK, none
    }

    public class RenderConfig : IDrawable
    {
        public static RenderConfig instance;

        public static bool Hovered { get; private set; }
        private static bool _windowActive = false;

        public static Sun sun = Sun.Default;
        public static Vector3 ambientColor = new Vector3(1, 1, 1);
        public static float ambientStrength = .25f;
        public static float gamma = 2.2f;
        public static float saturation = 1.2f;
        [Editor.Attributes.EnumField("Tonemap Mode")]
        public static TonemappingMode tonemappingMode = TonemappingMode.uchimura;

        public static List<Light> lights = new List<Light>();

        static RenderConfig()
        {
            instance = new RenderConfig();
            RenderSaveData.Default.Apply();
        }

        public static void SetData(RenderSaveData data)
        {
            sun = data.sun; ambientColor = data.ambientColor; ambientStrength = data.ambientStrength; 
        }

        public static OTkVec3 GetSunDirection()
        {
            return new OTkVec3(0, -MathF.Sin(sun.angle), -MathF.Cos(sun.angle));
        }

        public static OTkVec3 GetSunPosition()
        {
            return new OTkVec3(0, MathF.Sin(sun.angle) * 700, MathF.Cos(sun.angle) * 700);
        }

        public void DrawWindow()
        {
            const string title = "Render Handeller";
            if (ImGui.Begin(title, ref _windowActive, ImGuiWindowFlags.None))
            {
                Hovered = ImGui.IsWindowHovered();
                OnGUI();
            }
            ImGui.End();
        }

        private static void OnGUI()
        {
            GUI.HeaderIn("Render settings");
            GUI.EnumField(ref tonemappingMode, "Tonemap Mode");
            GUI.ColorEdit3(ref ambientColor, nameof(ambientColor), null, ImGuiColorEditFlags.NoAlpha);
            GUI.FloatField(ref ambientStrength, nameof(ambientStrength), null, Companent.ImguiDragSpeed);
            GUI.FloatField(ref saturation, nameof(saturation));
            
            sun.DrawWindow();

            Shadow.DrawShadowSettings();
        }

        public void Dispose() { GC.SuppressFinalize(this); }
    }

    [Serializable]
    public struct Sun
    {
        public static readonly Sun Default = new Sun
        {
            angleDegree = 45,
            intensity = 1.2f,
            sunColor = Vector4.One
        };

        public float angleDegree;
        public float angle => OpenTK.Mathematics.MathHelper.DegreesToRadians(angleDegree);
        public float   intensity;
        public Vector4 sunColor;

        public Sun(in float angle, in float intensity, in Vector4 color)
        {
            this.angleDegree = angle;
            this.intensity = intensity;
            this.sunColor = color;
        }
        internal void DrawWindow()
        {
            GUI.HeaderIn(nameof(Sun));
            GUI.FloatField(ref angleDegree, nameof(angle), ValueChanged, 0.1f);
            GUI.FloatField(ref intensity, nameof(intensity), ValueChanged, 0.02f);
            ImGui.ColorEdit4(nameof(sunColor), ref sunColor, ImGuiColorEditFlags.None);
        }

        private void ValueChanged()
        {
            Shadow.UpdateShadows();
        }

    }

    [Serializable]
    public class RenderSaveData
    {
        public static readonly RenderSaveData Default = new RenderSaveData()
        {
            sun = new Sun(45, 3.14f, Vector4.One),
            ambientColor = new Vector3(1, 1, 1),
            ambientStrength = .2f
        };

        public Sun sun ;
        public Vector3 ambientColor;
        public float ambientStrength;

        public void Apply()
        {
            RenderConfig.sun = sun;
            RenderConfig.ambientColor    =  ambientColor   ;
            RenderConfig.ambientStrength =  ambientStrength;
        }

        /// <summary> takes settings from current confing</summary>
        internal static RenderSaveData TakeFromCurrent() 
        {
            return new RenderSaveData()
            {
                sun = RenderConfig.sun,
                ambientColor = RenderConfig.ambientColor,
                ambientStrength = RenderConfig.ambientStrength
            };
        }
    }

}
