#pragma warning disable IDE0051 // Remove unused private members
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using System.Runtime.CompilerServices;
using ZargoEngine.Editor.Attributes;
using ZargoEngine.Helper;
using ZargoEngine.Rendering;

namespace ZargoEngine.Physics
{
    using SysVec3 = System.Numerics.Vector3;
    using SysQuat = System.Numerics.Quaternion;
    using TkVec3  = OpenTK.Mathematics.Vector3;
    using TKQuat  = OpenTK.Mathematics.Quaternion; 
    using BepuMesh = BepuPhysics.Collidables.Mesh;
    
    public unsafe class BepuCollider : Component
    {
        private MeshRenderer meshRenderer;
        public IShape? shape;
        public TypedIndex ShapeIndex;
        public BodyHandle handle;
        public StaticHandle staticHandle;

        [EnumField("ShapeType","ShapeTypeChanged")]
        public ShapeType shapeType; // enum
        public CollidableMobility mobility;// enum

        public float mass = 0;

        public void ShapeTypeChanged()
        {
            ref var scale = ref transform.scale.ToSystemRef();

            switch (shapeType)
            {
                case ShapeType.sphere:   UpdateBox    (ref scale, out _);  break;
                case ShapeType.capsule:  UpdateConvex (ref scale, out _);  break;
                case ShapeType.cylinder: UpdateSphere (ref scale, out _);  break;
                default:                 UpdateBox    (ref scale, out _);  break;
            }
        }

        GizmoBase DebugWire;

        public bool UpdatePhysics = true;

        public BepuCollider(GameObject gameObject, in float mass = 0 , CollidableMobility mobility = CollidableMobility.Kinematic) : base(gameObject)
        {
            if (gameObject.HasComponent<BepuCollider>()) {
                Debug.LogWarning("game object alredy has collider");
                return;
            }

            this.meshRenderer = gameObject.GetComponent<MeshRenderer>();
            this.mobility = mobility; this.mass = mass;

            gameObject.AddComponent(this);

            BodyInertia inertia;

            ref SysVec3 scalePtr = ref transform.scale.ToSystemRef();

            if (meshRenderer.mesh == MeshCreator.CreateCube() || meshRenderer.mesh == MeshCreator.CreateQuad() || meshRenderer == null) {
                UpdateBox(ref scalePtr, out inertia);
            }
            else if (meshRenderer.mesh == MeshCreator.CreateSphere()) UpdateSphere(ref scalePtr, out inertia);
            else UpdateConvex(ref scalePtr, out inertia);

            var collidableDescription = new CollidableDescription(ShapeIndex, 0.01f);
            var activity   = new BodyActivityDescription(0.01f);

            ref SysVec3 position = ref transform.position.ToSystemRef();
            ref SysQuat rotation = ref transform.rotation.ToSystemRef();

            lock (BepuHandle.simulation)
            { 
                switch (mobility)
                {
                    case CollidableMobility.Dynamic:
                        handle = BepuHandle.simulation.Bodies.Add(BodyDescription.CreateDynamic(new RigidPose(position,rotation), inertia, collidableDescription, activity));
                        break;
                    case CollidableMobility.Kinematic:
                        handle = BepuHandle.simulation.Bodies.Add(BodyDescription.CreateKinematic(new RigidPose(position, rotation), collidableDescription, activity));
                        break;
                    case CollidableMobility.Static:
                        staticHandle = BepuHandle.simulation.Statics.Add(new StaticDescription(position, rotation, collidableDescription));
                        break;
                }
            }

            SetPositionAndRotation(ref position,ref rotation);

            transform.OnScaleChanged += ScaleChangedFromEditor;
            transform.OnPositionChanged += PositionChanged;
            transform.OnRotationChanged += RotationChanged;
        }

        private void RotationChanged(ref TKQuat rotation)
        {
            SetRotation(ref rotation.ToSystemRef());
        }

        private void PositionChanged(ref TkVec3 position)
        {
            SetPosition(ref position.ToSystemRef());
        }

        private void UpdateBox(ref SysVec3 scale, out BodyInertia inertia) {
            lock (BepuHandle.simulation)
            { 
                if (!ShapeIndex.Exists || shape == null)
                {
                    var box = new Box(scale.X * 2, scale.Y * 2, scale.Z * 2);  shape = box;
                    ShapeIndex = BepuHandle.simulation.Shapes.Add(box);
                    box.ComputeInertia(mass, out inertia);
                    //DebugWire?.Dispose();
                    //DebugWire = new WireCube(transform);
                }
                else
                {
                    ref var box = ref BepuHandle.simulation.Shapes.GetShape<Box>(ShapeIndex.Index);
                    box.Width = scale.X; box.Height = scale.Y; box.Length = scale.Z;
                    inertia = default;
                }
            }
        }

        private void UpdateSphere(ref SysVec3 scale, out BodyInertia inertia) {

            lock (BepuHandle.simulation)
            { 
                if (!ShapeIndex.Exists || shape == null)
                {
                    float radius = scale.Length() / 3; 
                    var sphere = new Sphere(radius); shape = sphere;
                    ShapeIndex = BepuHandle.simulation.Shapes.Add(sphere);
                    sphere.ComputeInertia(mass, out inertia);
                    DebugWire?.Dispose();
                    DebugWire = new WireSphere(radius, transform);
                }
                else
                {
                    BepuHandle.simulation.Shapes.GetShape<Sphere>(ShapeIndex.Index).Radius = scale.Length() / 3;
                    inertia = default;
                }
            }
        }

        private void UpdateConvex(ref SysVec3 scale, out BodyInertia inertia) 
        {
            lock (BepuHandle.simulation)
            { 
                if (!ShapeIndex.Exists || shape == null)
                {
                    SysVec3[] positions = meshRenderer.mesh.GetPositions<SysVec3>();
                    BepuHandle.bufferPool.Take<Triangle>(positions.Length / 3, out Buffer<Triangle> triangleBuffer);

                    for (int i = 0; i < triangleBuffer.Length; i++)
                    {
                        triangleBuffer[i] = new Triangle(positions[i + 0], positions[i + 1], positions[i + 2]);
                    }

                    var convex = new BepuMesh(triangleBuffer,scale,BepuHandle.bufferPool); shape = convex;
                    ShapeIndex = BepuHandle.simulation.Shapes.Add(convex);
                    
                    convex.ComputeClosedInertia(mass, out inertia);
                    DebugWire?.Dispose();
                    DebugWire = new PointMesh(meshRenderer.mesh.GetPositions<TkVec3>(), transform);
                }
                else
                {
                    BepuHandle.simulation.Shapes.GetShape<BepuMesh>(ShapeIndex.Index).Scale = scale;
                    inertia = default;
                }
            }
        }

        private void ScaleChangedFromEditor(ref TkVec3 scale)
        {
            ref SysVec3 convertedScale = ref Unsafe.AsRef<SysVec3>(Unsafe.AsPointer(ref scale));

            switch (shape)
            {
                case Box:        UpdateBox    (ref convertedScale, out _); break;
                case ConvexHull: UpdateConvex (ref convertedScale, out _); break;
                case Sphere:     UpdateSphere (ref convertedScale, out _); break;
                default:         UpdateBox    (ref convertedScale, out _); break;
            }
        }

        public void SetPosition(ref SysVec3 position)
        {
            lock (BepuHandle.simulation)
            { 
                if (mobility == CollidableMobility.Dynamic || mobility == CollidableMobility.Kinematic)
                {
                    var referance = BepuHandle.simulation.Bodies.GetBodyReference(handle);
                    referance.Pose.Position = position;
                }
                else if (mobility == CollidableMobility.Static)
                {
                    var referance = BepuHandle.simulation.Statics.GetStaticReference(staticHandle);
                    referance.Pose.Position = position;
                }
            }
        }

        public void SetRotation(ref SysQuat quat)
        {
            lock (BepuHandle.simulation)
            { 
                if (mobility == CollidableMobility.Dynamic || mobility == CollidableMobility.Kinematic)
                {
                    var referance = BepuHandle.simulation.Bodies.GetBodyReference(handle);
                    referance.Pose.Orientation = quat;
                }
                else if (mobility == CollidableMobility.Static)
                {
                    var referance = BepuHandle.simulation.Statics.GetStaticReference(staticHandle);
                    referance.Pose.Orientation = quat;
                }
            }
        }

        public void SetPositionAndRotation(ref SysVec3 position,ref SysQuat rotation)
        {
            SetPosition(ref position); SetRotation(ref rotation);
        }

        // Updates every frame
        public override void Update()
        {
            if (!UpdatePhysics) return;

            if (mobility == CollidableMobility.Dynamic)
            {
                lock (BepuHandle.simulation)
                {
                    BepuHandle.simulation.Bodies.GetDescription(handle, out BodyDescription description);
                    transform.SetPosition(description.Pose.Position.ToOpenTK(), false);
                    transform.SetQuaterion(description.Pose.Orientation.ToOpenTK(), true, false);
                }
            }
        }

        public override void Render()
        {
            DebugWire?.Render();
        }
    }
}
