using ImGuiNET;
using OpenTK.Mathematics;
using System;
using System.Reflection;
using ZargoEngine.Helper;

namespace ZargoEngine.Editor.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    [Obsolete("dont use cause it is not allows you recive input via keyboard")]
    public class InputAttribute : GUIAttributeBase
    {
        public float step;
        public float step_fast;
        public new string format = "value: {0}";
        public ImGuiInputTextFlags flags = ImGuiInputTextFlags.None;

        public override bool Proceed(FieldInfo field, object value, Companent @object)
        {
            switch (value)
            {
                case float val:
                    if (ImGui.InputFloat(field.Name, ref val, step, step_fast, DefaultFormat, flags))
                    {
                        field.SetValue(this, val);
                        @object.OnValidate();
                    }
                    return true;
                case int intVal:
                    ImGui.InputInt(field.Name, ref intVal);
                    field.SetValue(this, intVal);
                    return true;
                case Vector2 vector2:
                    System.Numerics.Vector2 vec2Val = vector2.ToSystemRef();
                    if (ImGui.InputFloat2(field.Name, ref vec2Val, DefaultFormat, flags))
                    {
                        field.SetValue(this, vec2Val.ToOpenTKRef());
                        @object.OnValidate();
                    }
                    return true;
                case Vector3 vector3:
                    System.Numerics.Vector3 vec3Val = vector3.ToSystemRef();
                    if (ImGui.InputFloat3(field.Name, ref vec3Val,DefaultFormat , flags))
                    {
                        field.SetValue(this, vec3Val.ToOpenTKRef());
                        @object.OnValidate();
                    }
                    return true;
                case Vector4 vector4:
                    System.Numerics.Vector4 vec4Val = vector4.ToSystemRef();
                    if (ImGui.InputFloat4(field.Name, ref vec4Val, format, flags))
                    {
                        field.SetValue(this, vec4Val.ToOpenTKRef());
                        @object.OnValidate();
                    }
                    return true;
            }
            return false;
        }

        public InputAttribute(float step = .5f, float step_fast = 3, ImGuiInputTextFlags flags = ImGuiInputTextFlags.None, float speed = 0.1F, float min = 0, float max = 10, string format = "", ImGuiSliderFlags sliderFlags = ImGuiSliderFlags.Logarithmic) 
            : base(speed, min, max, format, sliderFlags)
        {
            this.format = format == "" ? "%.1f" : format;
            this.step = step;
            this.step_fast = step_fast;
            this.flags = flags;
        }
    }
}
