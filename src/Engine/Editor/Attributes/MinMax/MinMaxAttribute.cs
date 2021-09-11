using OpenTK.Mathematics;
using ZargoEngine.Mathmatics;

namespace ZargoEngine.Editor.Attributes
{
    public class MinMaxAttribute : GUIMinMaxBase
    {
        // todo add diffrent imgui drawing 
        public float min, max;

        public override ref object Get(ref object value)
        {
            if (value is Vector3 vector3){
                Mathmatic.Clamp(ref vector3, min, max);
                value = vector3;
            }
            if (value is Vector2 vector2){
                Mathmatic.Clamp(ref vector2, min, max);
                value = vector2;
            }
            if (value is float floatingPoint){
                Mathmatic.Clamp(ref floatingPoint, min, max);
                value = floatingPoint;
            }
            return ref value;
        }

        public MinMaxAttribute(float min, float max)
        {
            this.min = min;
            this.max = max;
        }
    }
}
