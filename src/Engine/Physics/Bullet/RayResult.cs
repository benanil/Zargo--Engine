using BulletSharp;
using BulletSharp.Math;

namespace ZargoEngine
{
    public class RayResult : ClosestRayResultCallback
    {
        public RayResult(ref Vector3 rayFromWorld, ref Vector3 rayToWorld) : base(ref rayFromWorld, ref rayToWorld)
        {

        }

        public Transform GetTransform()
        {
            return (Transform)CollisionObject.UserObject;
        }
    }
}
