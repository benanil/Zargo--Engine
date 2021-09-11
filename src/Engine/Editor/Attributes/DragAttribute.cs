using ImGuiNET;
using OpenTK.Mathematics;
using System;
using System.Reflection;
using ZargoEngine.Helper;

namespace ZargoEngine.Editor.Attributes
{
    [AttributeUsage(AttributeTargets.Field,AllowMultiple = false)]
    [Obsolete("dont use cause it is not allows you recive input via keyboard")]
    public class DragAttribute : GUIAttributeBase
    {
        /// <returns>continue</returns>
        public override bool Proceed(FieldInfo field, object value, Component @object)
        {
            switch (value)
            {
                case float val:
                    if (ImGui.DragFloat(field.Name, ref val, speed, min, max, format, sliderFlags)){
                        field.SetValue(this, val);
                        @object.OnValidate();
                    }
                    return true;
                case int intVal:
                    if (ImGui.DragInt(field.Name, ref intVal))
                    {
                        field.SetValue(this, intVal);
                        @object.OnValidate();
                    }
                    return true;
                case Vector2 vector2:
                    System.Numerics.Vector2 vec2Val = vector2.ToSystemRef();
                    if (ImGui.DragFloat2(field.Name, ref vec2Val, speed, min, max, format, sliderFlags))
                    {
                        field.SetValue(this, vec2Val.ToOpenTKRef());
                        @object.OnValidate();
                    }
                    return true;
                case Vector3 vector3:
                    System.Numerics.Vector3 vec3Val = vector3.ToSystemRef();
                    if (ImGui.DragFloat3(field.Name, ref vec3Val, speed, min, max, format, sliderFlags))
                    {
                        field.SetValue(this, vec3Val.ToOpenTKRef());
                        @object.OnValidate();
                    }
                    return true;
                case Vector4 vector4:
                    System.Numerics.Vector4 vec4Val = vector4.ToSystemRef();
                    if (ImGui.DragFloat4(field.Name, ref vec4Val, speed, min, max, format, sliderFlags))
                    {
                        field.SetValue(this, vec4Val.ToOpenTKRef());
                        @object.OnValidate();
                    }
                    return true;
            }
            return false;
        }

        public DragAttribute(float speed = .1f,float min = 0, float max = 10,string format = DefaultFormat,ImGuiSliderFlags sliderFlags = ImGuiSliderFlags.Logarithmic) 
            : base(speed,min,max,format,sliderFlags)
        {
            this.sliderFlags = sliderFlags;
            this.format = format == "" ? "%.1f" : format;
            this.speed = speed;
            this.min = min;
            this.max = max;
        }
    }
}
