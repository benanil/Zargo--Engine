using OpenTK.Mathematics;
using System;
using ZargoEngine.Mathmatics;

namespace ZargoEngine.Editor.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class MinAttribute : GUIMinMaxBase
    {
        public float min;

        public override ref object Get(ref object value)
        {
            if (value is Vector3 vector3){
                Mathmatic.Min(ref vector3, min);
                value = vector3;
            }
            if (value is Vector2 vector2){
                Mathmatic.Min(ref vector2, min);
                value = vector2;
            }
            if (value is float floatingPoint){
                value = Mathmatic.Min(floatingPoint, min);
            }
            return ref value;
        }

        public MinAttribute(float min)
        {
            this.min = min;
        }
    }
}
