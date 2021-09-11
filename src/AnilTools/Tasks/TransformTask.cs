
using System;

namespace ZargoEngine.AnilTools
{
    public abstract class TransformTask : TaskBase 
    {
        // if animatipn curve using speed is time
        protected readonly float speed;
        protected readonly MoveType moveType;
        protected readonly TransformationFlags flags;
        protected readonly Transform from;
        protected readonly AnimationCurve animationCurve;

        protected TransformTask(Transform from, TransformationFlags transformationFlags, float speed, UpdateType updateType,
                                MoveType moveType, AnimationCurve animationCurve,Action endAction,string name = "") : base(updateType,endAction,name)
        {
            currentTask = this;
            this.speed = speed;
            this.moveType = moveType;
            this.flags = transformationFlags;
            this.from = from;
            this.animationCurve = animationCurve;
        }
    }
}
