
using BulletSharp;

namespace ZargoEngine
{
    public abstract class MonoBehaviour : Component
    {
        public MonoBehaviour(GameObject go) : base(go) { }

        public override void Start() {}
        public override void Update() {}

        public virtual void OnTriggerEnter(CollisionObject other, AlignedManifoldArray details){}
        public virtual void OnTriggerExit(CollisionObject other) {}
        public virtual void OnTriggerStay(CollisionObject other, AlignedManifoldArray details){}
    }
}