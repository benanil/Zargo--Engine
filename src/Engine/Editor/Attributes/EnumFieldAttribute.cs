using ImGuiNET;
using System;

namespace ZargoEngine.Editor.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class EnumFieldAttribute : Attribute
    {
        public string Header;
        public string OnSellect;
        public ImGuiComboFlags ImGuiComboFlags;

        public EnumFieldAttribute(string header, string onSellect = "", ImGuiComboFlags imGuiComboFlags = ImGuiComboFlags.None)
        {
            Header = header;
            OnSellect = onSellect;
            ImGuiComboFlags = imGuiComboFlags;
        }
    }
}
