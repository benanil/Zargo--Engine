using OpenTK.Mathematics;
using System;
using ZargoEngine.Mathmatics;

namespace ZargoEngine.AnilTools
{
    public class PositionTask : TransformTask
    {
        private const float PositionTolerance = Mathmatic.twoZeroOne;

        private readonly Vector3 targetPosition;

        public override bool Proceed()
        {
            if (moveType == MoveType.lerp) from.position = Vector3.Lerp(from.position, targetPosition, Time.DeltaTime * speed);
            if (moveType == MoveType.MoveTowards) from.position = Mathmatic.MoveTowards(from.position, targetPosition, Time.DeltaTime * speed);
            if (moveType == MoveType.curve) { } // comining
            return IsFinished();
        }

        public override bool IsFinished()
        {
            if (from.Distance(targetPosition) < PositionTolerance){
                from.position = targetPosition;
                return true;
            }
            return false;
        }
        
        public PositionTask(Transform from, TransformationFlags flags, float speed, UpdateType updateType,
                                MoveType moveType, AnimationCurve animationCurve, Action endAction,Vector3 targetPosition) : base(from,flags,speed,updateType,moveType,animationCurve,endAction)
        {
            currentTask = this;
            this.targetPosition = targetPosition;
            this.targetPosition = GetTargetPosition();
        }

        private Vector3 GetTargetPosition()
        {
            if ((flags & TransformationFlags.isPlus & TransformationFlags.isLocal) == 0) {
                // later on
                return from.localPosition + targetPosition;
            }

            if (flags.HasFlag(TransformationFlags.isPlus)){
                return from.position + targetPosition; 
            }
            return targetPosition;
        }
    }
}
