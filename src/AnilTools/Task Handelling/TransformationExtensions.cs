using OpenTK.Mathematics;
using System;

namespace ZargoEngine.AnilTools
{
    public static class TransformationExtensions
    {
        public static PositionTask Move(this Transform transform, in Vector3 targetPosition, float speed = 3,
                                        TransformationFlags flags = TransformationFlags.position, MoveType moveType = MoveType.MoveTowards, UpdateType updateType = UpdateType.Update,
                                        AnimationCurve animationCurve = null, Action endAction = null)
        {
            var task = new PositionTask(transform, flags, speed, updateType, moveType, animationCurve, endAction, targetPosition);
            ZargoUpdate.Register(task,updateType);
            return task;
        }

        public static ScaleTask Scale(this Transform transform, in Vector3 targetScale, float speed = 3,
                                        TransformationFlags flags = TransformationFlags.scale, MoveType moveType = MoveType.MoveTowards, UpdateType updateType = UpdateType.Update,
                                        AnimationCurve animationCurve = null, Action endAction = null)
        {
            var task = new ScaleTask(transform, flags, speed, updateType, moveType, animationCurve, endAction, targetScale);
            ZargoUpdate.Register(task, updateType);
            return task;
        }

        public static RotateTask Rotate(this Transform transform, in Quaternion targetRotation, in float speed = 3,
                                        TransformationFlags flags = TransformationFlags.rotation, MoveType moveType = MoveType.MoveTowards, UpdateType updateType = UpdateType.Update,
                                        AnimationCurve animationCurve = null, Action endAction = null
        ){
            var task = new RotateTask(transform,targetRotation,flags,speed,updateType,moveType,animationCurve,endAction);
            ZargoUpdate.Register(task, updateType);
            return task;
        }


        /// <summary> dont forget Start();</summary>       
        public static TranslateTask Translate(this Transform transform, in RTS rts, TransformationFlags flags = TransformationFlags.rotation, MoveType moveType = MoveType.MoveTowards, UpdateType updateType = UpdateType.Update,
                                        AnimationCurve animationCurve = null, Action endAction = null)
        {
            PositionTask positionTask = null;
            ScaleTask scaleTask = null;
            RotateTask rotateTask = null;

            if (flags.HasFlag(TransformationFlags.position))
            {
                positionTask = new PositionTask(transform, flags, rts.positionSpeed, updateType, moveType, animationCurve, endAction, rts.position);
            }
            if (flags.HasFlag(TransformationFlags.scale))
            {
                scaleTask = new ScaleTask(transform, flags, rts.scaleSpeed, updateType, moveType, animationCurve, endAction, rts.scale);
            }
            if (flags.HasFlag(TransformationFlags.rotation))
            {
                rotateTask = new RotateTask(transform, rts.rotation, flags, rts.rotationSpeed, updateType, moveType, animationCurve, endAction);
            }
            var task = new TranslateTask(rotateTask, scaleTask, positionTask, endAction,UpdateType.Update);
            return task;
        }
    }

    public struct RTS
    {
        public readonly Vector3 position;
        public readonly Vector3 scale;
        public readonly Quaternion rotation;

        public float positionSpeed, rotationSpeed ,scaleSpeed;

        public void SetTime(float value)
        {
            positionSpeed = value; rotationSpeed = value; scaleSpeed = value;
        }

        public RTS(Vector3 position, Vector3 scale, Quaternion rotation,
            float positionSpeed = 3, float rotationSpeed = 3, float scaleSpeed = 3)
        {
            this.positionSpeed = positionSpeed;
            this.rotationSpeed = rotationSpeed;
            this.scaleSpeed = scaleSpeed;
            this.position = position;
            this.scale = scale;
            this.rotation = rotation;
        }

        public RTS(Transform transform, float positionSpeed = 3, float rotationSpeed = 3, float scaleSpeed = 3)
        {
            this.positionSpeed = positionSpeed;
            this.rotationSpeed = rotationSpeed;
            this.scaleSpeed = scaleSpeed;
            position = transform.position;
            scale = transform.scale;
            rotation = transform.rotation;
        }
    }
}
