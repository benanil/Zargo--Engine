using OpenTK.Mathematics;
using System.Text.RegularExpressions;

namespace ZargoEngine
{
    using Color3 = System.Numerics.Vector3;
    using Color4 = System.Numerics.Vector4;

    public static class Parser
    {
        public const string FindNumbers = @"\b(\d+(?:\.\d+)?)\b";
        /// <summary>finds floats and integers</summary>

        public static Vector2 ParseVec2(string value)
        {
            Match match = Regex.Match(value, FindNumbers);
            float x = float.Parse(match.Groups[0].Value);
            float y = float.Parse(match.NextMatch().Groups[0].Value);
            
            return new Vector2(x, y);
        }

        public static Vector3 ParseVec3(string value)
        {
            Match match = Regex.Match(value, FindNumbers);
            
            float x = float.Parse(match.Groups[0].Value);
            float y = float.Parse((match = match.NextMatch()).Groups[0].Value);
            float z = float.Parse(match.NextMatch().Groups[0].Value);
            return new Vector3(x, y, z);
        }

        public static Color3 ParseColor3(string value)
        {
            Match match = Regex.Match(value, FindNumbers);
            float x = float.Parse(match.Groups[0].Value);
            float y = float.Parse((match = match.NextMatch()).Groups[0].Value);
            float z = float.Parse(match.NextMatch().Groups[0].Value);
            return new Color3(x, y, z);
        }

        public static Color4 ParseColor4(string value)
        {
            Match match = Regex.Match(value, FindNumbers);
            float x = float.Parse(match.Groups[0].Value);
            float y = float.Parse((match = match.NextMatch()).Groups[0].Value);
            float z = float.Parse((match = match.NextMatch()).Groups[0].Value);
            float w = float.Parse(match.NextMatch().Groups[0].Value);

            return new Color4(x, y, z, w);
        }
    }
}
