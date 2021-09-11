using OpenTK.Mathematics;
using System;
using ZargoEngine.Mathmatics;

namespace ZargoEngine.Editor.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class MaxAttribute : GUIMinMaxBase
    {
        public float Max;

        public override ref object Get(ref object value)
        {
            if (value is Vector3 vector3){
                Mathmatic.Min(ref vector3, Max);
                value = vector3;
            }
            if (value is Vector2 vector2){
                Mathmatic.Min(ref vector2, Max);
                value = vector2;
            }
            if (value is float floatingPoint){
                value = Mathmatic.Max(floatingPoint, Max);
            }
            return ref value;
        }

        public MaxAttribute(float max)
        {
            this.Max = max;
        }
    }
}
