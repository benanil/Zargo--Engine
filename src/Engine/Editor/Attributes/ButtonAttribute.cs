
using System;
using System.Numerics;

namespace ZargoEngine.Editor
{
    [AttributeUsage(AttributeTargets.Method)]
    public unsafe class ButtonAttribute : Attribute
    {
        public Vector2 size;
        public string name = string.Empty;

        public ButtonAttribute()
        {
            size = new Vector2(60, 20);
        }

        public ButtonAttribute(Vector2 size)
        {
            this.size = size;
        }

        public ButtonAttribute(float sizex = 60,float sizeY = 20, string name = "")
        {
            this.size = new Vector2(sizex,sizeY);
            this.name = name;
        }
    }
}