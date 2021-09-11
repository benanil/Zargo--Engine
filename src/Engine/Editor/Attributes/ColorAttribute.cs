using ImGuiNET;
using System;

namespace ZargoEngine.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ColorAttribute : Attribute
    {
        public ImGuiColorEditFlags flags;

        public ColorAttribute(ImGuiColorEditFlags flags)
        {
            this.flags = flags;
        }
    }
}
