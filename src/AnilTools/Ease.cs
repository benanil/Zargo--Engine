using ZargoEngine.Mathmatics;

namespace ZargoEngine2.src.AnilTools
{
    public static class Ease
    {
        public static float QuadIn(float from, float to, float time)
        {
            return Mathmatic.Lerp(from, to, time * time);
        }

        public static float QuadOut(float from, float to, float time)
        {
            return Mathmatic.Lerp(from, to, -time * (time - 2f));
        }

    }
}
