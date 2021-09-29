
using BulletSharp;

namespace ZargoEngine
{
    public abstract class MonoBehaviour : Companent
    {
        public MonoBehaviour(GameObject go) : base(go) { }

        public override void Start() {}
        public override void Update() {}

        public void OnTriggerEnter(CollisionObject other, AlignedManifoldArray details){}
        public void OnTriggerExit(CollisionObject other) {}
        public void OnTriggerStay(CollisionObject other, AlignedManifoldArray details){}
    }
}