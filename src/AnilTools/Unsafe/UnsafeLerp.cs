
using OpenTK.Mathematics;
using System;
using ZargoEngine.Mathmatics;

namespace ZargoEngine.AnilTools
{
    public unsafe static class UnsafeLerp
    {
        public static void FloatLerp(float* value,float targetValue, float speed, Action endAction = null)
        {
            RegisterUpdate.UpdateWhile(
                () => value->Set(Mathmatic.Lerp(*value, targetValue, Time.DeltaTime * speed)),
                () => value->Diffrance(targetValue) < 0.001f, UpdateType.Update,
                () =>
                {
                    value->Set(targetValue);
                    endAction.Invoke();
                });
        }

        public static void FloatTowards(float* value, float targetValue, float speed, Action endAction = null)
        {
            RegisterUpdate.UpdateWhile(
                () => value->Set(Mathmatic.MoveTowards(*value, targetValue, Time.DeltaTime * speed)),
                () => value->Diffrance(targetValue) < 0.001f, UpdateType.Update,
                () =>
                {
                    value->Set(targetValue);
                    endAction.Invoke();
                });
        }

        public static void VectorLerp(Vector3* value, Vector3 targetValue, float speed, Action endAction = null)
        {
            RegisterUpdate.UpdateWhile(
                () => value->Set(Vector3.Lerp(*value, targetValue, Time.DeltaTime * speed)),
                () => Vector3.Distance(*value,targetValue) < 0.001f, UpdateType.Update,
                () =>
                {
                    value->Set(targetValue);
                    endAction.Invoke();
                });
        }

        public static void VectorTowards(Vector3* value, Vector3 targetValue, float speed, Action endAction = null)
        {
            RegisterUpdate.UpdateWhile(
                () => value->Set(Mathmatic.MoveTowards(*value, targetValue, Time.DeltaTime * speed)),
                () => Vector3.Distance(*value, targetValue) < 0.001f, UpdateType.Update,
                () =>
                {
                    value->Set(targetValue);
                    endAction.Invoke();
                });
        }

        public static void Set(this ref float value, float target)
        {
            value = target;
        }

        public static void Set(this ref Vector3 value, Vector3 target)
        {
            value = target;
        }
    }
}
