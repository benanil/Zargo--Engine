using ImGuiNET;
using OpenTK.Mathematics;
using System;
using System.Reflection;
using ZargoEngine.Helper;

namespace ZargoEngine.Editor.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    /// <summary>it is only working for float values <summary/>
    public class SliderAttribute : GUIAttributeBase
    {
        public override bool Proceed(FieldInfo field, object value, Companent @object)
        {
            switch (value)
            {
                case float val:
                    if (ImGui.SliderFloat(field.Name, ref val, min, max, format, sliderFlags))
                    {
                        field.SetValue(value, val);
                        @object.OnValidate();
                    }
                    return true;
                case int intVal:
                    if (ImGui.DragInt(field.Name, ref intVal))
                    {
                        field.SetValue(value, intVal);
                        @object.OnValidate();
                    }
                    return true;
                case Vector2 vector2:
                    System.Numerics.Vector2 vec2Val = vector2.ToSystemRef();
                    if (ImGui.DragFloat2(field.Name, ref vec2Val, speed, min, max, format, sliderFlags))
                    {
                        field.SetValue(value, vec2Val.ToOpenTKRef());
                        @object.OnValidate();
                    }
                    return true;
                case Vector3 vector3:
                    System.Numerics.Vector3 vec3Val = vector3.ToSystemRef();
                    if (ImGui.DragFloat3(field.Name, ref vec3Val, speed, min, max, format, sliderFlags))
                    {
                        field.SetValue(value, vec3Val.ToOpenTKRef());
                        @object.OnValidate();
                    }
                    return true;
                case Vector4 vector4:
                    System.Numerics.Vector4 vec4Val = vector4.ToSystemRef();
                    if (ImGui.DragFloat4(field.Name, ref vec4Val, speed, min, max, format, sliderFlags))
                    {
                        field.SetValue(value, vec4Val.ToOpenTKRef());
                        @object.OnValidate();
                    }
                    return true;
            }
            return false;
        }

        public SliderAttribute(float speed = 0.1F, float min = 0, float max = 10, string format = "", ImGuiSliderFlags sliderFlags = ImGuiSliderFlags.Logarithmic) : base(speed, min, max, format, sliderFlags)
        {

        }
    }
}
