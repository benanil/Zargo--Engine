using System.Numerics;
using System.Runtime.InteropServices;

namespace ZargoEngine
{
    public static class Delegates
    {
        public delegate void SysValueChanged2([In] Vector2 position);
        public delegate void SysValueChanged3([In] Vector3 position);
        public delegate void SysValueChanged4([In] Vector4 position);

        public delegate void PositionChanged2([In] OpenTK.Mathematics.Vector2 position);
        public delegate void PositionChanged3([In] OpenTK.Mathematics.Vector3 position);
        public delegate void PositionChanged4([In] OpenTK.Mathematics.Vector4 position);
    }
}
