using OpenTK.Mathematics;
using System;

namespace ZargoEngine.AnilTools
{
    public static class Ease3D
    {
        public static Vector3 QuadraticLerp(Vector3 a, Vector3 b, Vector3 c, float interpolateAmount)
        {
            Vector3 ab = Vector3.Lerp(a, b, 1);
            Vector3 bc = Vector3.Lerp(b, c, 1);

            return Vector3.Lerp(ab, bc, interpolateAmount);
        }

        /// <summary>
        /// 2 nokta arasında bombeli gidiş
        /// </summary>
        /// <param name="a">gidecek olan</param>
        /// <param name="b">baslangıc noktasi</param>
        /// <param name="c">yukari noktasi</param>
        /// <param name="d">bitis noktasi</param>
        /// <param name="interpolateAmount"></param>
        public static Vector3 CubicLerp(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float interpolateAmount)
        {
            Vector3 ab_bc = QuadraticLerp(a, b, c, interpolateAmount);
            Vector3 bc_cd = QuadraticLerp(b, c, d, interpolateAmount);
            return Vector3.Lerp(ab_bc, bc_cd, interpolateAmount);
        }

        /// <summary>
        /// 2 nokta arasında bombeli gidiş
        /// </summary>
        /// <param name="transform">gidecek olan</param>
        /// <param name="b">baslangıc noktasi</param>
        /// <param name="c">yukari noktasi</param>
        /// <param name="d">bitis noktasi</param>
        /// <param name="senderId">for debug or multiple times</param>
        /// <param name="interpolateAmount"></param>
        public static UpdateTask CubicLerpAnim(this Transform transform, Vector3 b, Vector3 c, Vector3 d, float speed, AnimationCurve curve = null, Action then = null, int senderId = 0)
        {
            /*
            Tuple<float> currentAmount = new Tuple<float>(0);
            if (curve == null)
            {
                curve = AnimationCurve.Linear(0, 0, 1, 1);
            }

            var task = new UpdateTask(
                () =>
                {
                    currentAmount.value = Mathmatic.Max(currentAmount.value + speed * Time.deltaTime, 1);
                    transform.position = CubicLerp(transform.position, b, c, d, curve.Evaluate(currentAmount.value));
                },
                () => transform.Distance(d) > 0.01f, then, senderId);

            AnilUpdate.Register(task);
            return task;
            */
            return default;
        }

    }
}
