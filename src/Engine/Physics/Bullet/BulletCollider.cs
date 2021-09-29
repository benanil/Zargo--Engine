#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE0044 // Add readonly modifier
using System;
using System.Windows.Forms;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

using ZargoEngine.Mathmatics;
using ZargoEngine.Editor.Attributes;
using ZargoEngine.Rendering;
using BulletSharp;

namespace ZargoEngine.Physics
{
    using OTKvector3 = OpenTK.Mathematics.Vector3;
    using OTKmatrix = OpenTK.Mathematics.Matrix4;
    using Bvector3 = BulletSharp.Math.Vector3;
    using Bmatrix = BulletSharp.Math.Matrix;
    
    // todo add gizmos for collisions
    public unsafe class BulletCollider : Companent
    {
        private MeshRenderer meshRenderer;

        private CollisionObject collisionObject;
        private RigidBody rigidBody;
        private CollisionShape shape;

        [EnumField("ActivationState", "ActivationStateChanged")]
        public ActivationState activationState;

        public void ActivationStateChanged() {
            rigidBody.ActivationState = activationState;
        }
        
        [EnumField("ShapeType","ShapeTypeChanged")] // inspectorda görmek ve editlemek için
        public ShapeType shapeType;

        public void ShapeTypeChanged() {

            if (DebugWire != null) Gizmos.Remove(DebugWire);
            switch (shapeType)
            {
                case ShapeType.box:      MakeCube();     break;
                case ShapeType.sphere:   MakeSphere();   break;
                case ShapeType.capsule:  MakeCylinder(); /*not defined yet*/ break;
                case ShapeType.cylinder: MakeCylinder(); break;
                default: MakeCube(); break;
            }
        }
        private GizmoBase DebugWire;
        private GizmoBase BulletDebug;

        [EnumField("Collision Flags","CollisionFlagsChanged")] 
        public CollisionFlags collisionFlags;

        private void CollisionFlagsChanged() => rigidBody.CollisionFlags = collisionFlags;

        public float Mass = 5;

        private GhostObject GhostObject => (GhostObject)collisionObject;

        private Bvector3 inertia = Bvector3.Zero;

        public bool UpdatePhysics = true;

        public override void OnValidate() {
            rigidBody.SetMassPropsRef(Mass, ref inertia);
        }

        public BulletCollider(GameObject gameObject, in float mass = 0, bool dynamic = false) : base(gameObject)
        {
            this.Mass = mass / 3; // for now 3

            if (transform.gameObject.HasComponent<BulletCollider>()) {
                MessageBox.Show("gameobject alredy has collider!");
                return;
            }
            
            if (!gameObject.TryGetComponent(out meshRenderer) || meshRenderer.mesh == MeshCreator.CreateCube() || meshRenderer.mesh == MeshCreator.CreateQuad()) {
                MakeCube();
            }
            else if (meshRenderer.mesh == MeshCreator.CreateSphere())  MakeSphere();
            else MakeConvex();// mesh creator'a default cylinder capsule mesh eklenebilir 

            shape.CalculateLocalInertia(mass, out inertia);

            shape.LocalScaling = Unsafe.As<OTKvector3, Bvector3>(ref transform.position);

            Bmatrix convertedMatrix = transform.Translation.ToBullet();

            collisionObject = new CollisionObject()
            {
                CollisionShape = shape,
                UserObject = transform,
                WorldTransform = convertedMatrix,
            };

            DefaultMotionState motionstate = new DefaultMotionState(convertedMatrix);
            using RigidBodyConstructionInfo constructionInfo = new(mass, motionstate, shape, inertia);
            rigidBody = new RigidBody(constructionInfo);

            rigidBody.WorldTransform = convertedMatrix;

            BulletPhysics.dynamicsWorld.AddRigidBody(rigidBody);

            rigidBody.CollisionFlags = dynamic ? CollisionFlags.None : CollisionFlags.KinematicObject; 

            BulletDebug = new WireCube(rigidBody.WorldTransform.ExtractTranslationFromMatrix(), shape.LocalScaling.Bullet2TK(), rigidBody.WorldTransform.ExtractRotationFromMatrix(),new System.Numerics.Vector4(1,0,0,1));
            // if we edit on inspector we can control objects
            transform.OnTransformChanged += TransformationChanged;
            
            collisionFlags = rigidBody.CollisionFlags;
        }

        // if we edit on inspector we can control objects
        private unsafe void TransformationChanged(ref OTKmatrix matrix) {
            // rigidBody.WorldTransform = Unsafe.AsRef<Bmatrix>(matrix);
            // ref var extractedMatrix = ref Unsafe.AsRef<Bmatrix>(matrix);
            shape.LocalScaling = matrix.ExtractScaleFromMatrix().Tk2Bullet();
            BulletDebug.GetModelMatrix() = matrix;
        }
        
        HashSet<CollisionObject> objsIWasInContactWithLastFrame = new HashSet<CollisionObject>();
        HashSet<CollisionObject> objsCurrentlyInContactWith = new HashSet<CollisionObject>();

        public override void PhysicsUpdate()
        {
            if (!UpdatePhysics) return;

            #region events
            //Todo: Trigger events 
            objsCurrentlyInContactWith.Clear();
            for (int i = 0; i < GhostObject.NumOverlappingObjects; i++)
            {
                CollisionObject otherObj = GhostObject.GetOverlappingObject(i);
                objsCurrentlyInContactWith.Add(otherObj);
                if (!objsIWasInContactWithLastFrame.Contains(otherObj))
                {
                    gameObject.OnTriggerEnter(otherObj, null);
                }
                else
                {
                    gameObject.OnTriggerStay(otherObj, null);
                }
            }
            objsIWasInContactWithLastFrame.ExceptWith(objsCurrentlyInContactWith);
            
            foreach (CollisionObject co in objsIWasInContactWithLastFrame)
            {
                gameObject.OnTriggerExit(co);
            }
            
            //swap the hashsets so objsIWasInContactWithLastFrame now contains the list of objs.
            HashSet<CollisionObject> temp = objsIWasInContactWithLastFrame;
            objsIWasInContactWithLastFrame = objsCurrentlyInContactWith;
            objsCurrentlyInContactWith = temp; 
            
            #endregion
            transform.SetPosition(rigidBody.WorldTransform.ExtractTranslationFromMatrix(), true);
            //transform.SetMatrix(rigidBody.WorldTransform);
        }
        
        public static BulletCollider CreateDynamic(GameObject gameObject, in float mass = 5){
            return new BulletCollider(gameObject, mass, true);
        }

        public static BulletCollider CreateStatic(GameObject gameObject){
            return new BulletCollider(gameObject);
        }

        private void MakeCube() { 
            //DebugWire   = new WireCube(transform); 
            shape       = new BoxShape(gameObject.transform.scale.Tk2Bullet());
            shapeType   = ShapeType.box;// inspectorda görmek ve editlemek için
        }

        private void MakeSphere() {
            DebugWire   = new WireSphere(transform.scale.Length / 3, gameObject.transform);
            shape       = new SphereShape(gameObject.transform.scale.Length / 3);
            BulletDebug = new WireSphere(shape.LocalScaling.Bullet2TK().LengthFast / 3, gameObject.transform);
            shapeType = ShapeType.sphere;// inspectorda görmek ve editlemek için
        }

        private void MakeCylinder() {
            DebugWire = new WireSphere(gameObject.transform.scale.Magnitude(), transform); 
            shape     = new CylinderShape(gameObject.transform.scale.Tk2BulletRef());
            shapeType = ShapeType.sphere;// inspectorda görmek ve editlemek için
        }

        private void MakeConvex()
        {
            Bvector3[] vertexes = meshRenderer.mesh.GetPositions<Bvector3>();
            
            TriangleMesh triangleMesh = new TriangleMesh();

            for (int i = 0; i < vertexes.Length / 3; i++)
            {
                triangleMesh.AddTriangle(vertexes[i + 0], vertexes[i + 1], vertexes[i + 2]);
            }

            shape = new ConvexTriangleMeshShape(triangleMesh);

            DebugWire = new PointMesh(meshRenderer.mesh.GetPositions<OTKvector3>(), gameObject.transform); 
        }

        public override void OnComponentAdded() {

        }

        // component overrides
        // at scene starts
        public override void Start()  {}
        public override void Update() {}
        public override void Render() {}

        public override void DrawWindow() {
            base.DrawWindow();
        }

        public override void Dispose() {
            shape.Dispose();
            BulletPhysics.dynamicsWorld.RemoveCollisionObject(collisionObject);
            collisionObject.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
