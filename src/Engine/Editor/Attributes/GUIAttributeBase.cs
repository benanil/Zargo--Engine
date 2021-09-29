using ImGuiNET;
using System;
using System.Reflection;

namespace ZargoEngine.Editor.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public abstract class GUIAttributeBase : Attribute
    {
        protected const string DefaultFormat = "½.1f";

        public float min;
        public float max;
        public float speed;
        public string format;
        public ImGuiSliderFlags sliderFlags;

        protected GUIAttributeBase(float speed = .1f, float min = 0, float max = 10, string format = "", ImGuiSliderFlags sliderFlags = ImGuiSliderFlags.Logarithmic)
        {
            this.min = min;
            this.max = max;
            this.speed = speed;
            this.format = format;
            this.sliderFlags = sliderFlags;
        }

        /// <returns>continue</returns>
        public virtual bool Proceed(FieldInfo field, object value, Companent @object) 
        {
            return false;
        }

    }
}
