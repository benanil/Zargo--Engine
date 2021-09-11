using System;

namespace ZargoEngine.Editor.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class NonSerializedAttribute : Attribute
    {
        public NonSerializedAttribute()
        {

        }
    }
}
