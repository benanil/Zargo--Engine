using System;

namespace ZargoEngine.Editor.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class GUIMinMaxBase : Attribute
    {
        public virtual ref object Get(ref object value)
        {
            return ref value;
        }
    }
}
