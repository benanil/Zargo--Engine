

using OpenTK.Mathematics;
using System;
using ZargoEngine.Mathmatics;

namespace ZargoEngine.AnilTools
{
    public class ScaleTask : TransformTask
    {
        private readonly Vector3 targetScale;

        public override bool IsFinished()
        {
            if (Vector3.Distance(from.scale, targetScale) < Mathmatic.twoZeroOne)
            {
                from.scale = targetScale; // for now
            }
            return false;
        }

        public override bool Proceed()
        {
            if (moveType == MoveType.lerp)        from.position = Vector3.Lerp(from.position, targetScale, Time.DeltaTime * speed);
            if (moveType == MoveType.MoveTowards) from.position = Mathmatic.MoveTowards(from.position, targetScale, Time.DeltaTime * speed);
            if (moveType == MoveType.curve) { } // comining
            return IsFinished();
        }

        public ScaleTask(Transform from, TransformationFlags flags, float speed, UpdateType updateType,
                                MoveType moveType, AnimationCurve animationCurve, Action endAction, Vector3 targetScale) : base(from, flags, speed, updateType, moveType, animationCurve, endAction)
        {
            currentTask = this;
            this.targetScale = targetScale;
        }
    }
}
