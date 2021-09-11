#define Editor

using BulletSharp;
using ZargoEngine.Rendering;
using ZargoEngine.Editor;

using System.Runtime.CompilerServices;
using System;

namespace ZargoEngine.Physics
{
    using OTKvector3 = OpenTK.Mathematics.Vector3;
    using OTKvector4 = OpenTK.Mathematics.Vector4;
    using OTKmatrix  = OpenTK.Mathematics.Matrix4;
    using OTKquaternion = OpenTK.Mathematics.Quaternion;
    using Bvector3 = BulletSharp.Math.Vector3;
    using Bvector4 = BulletSharp.Math.Vector4;
    using Bquaternion = BulletSharp.Math.Quaternion;
    
    // layer for raycasting
    public static unsafe partial class BulletPhysics
    {
        private static Bvector3 _gravity = new(0, -5f, 0);
        public static Bvector3 Gravity
        {
            get => _gravity;
            set
            {
                dynamicsWorld.SetGravity(ref _gravity);
            }
        }

        public static readonly BroadphaseInterface broadphase;
        public static readonly CollisionDispatcher dispatcher;
        public static readonly SequentialImpulseConstraintSolver solver;
        public static readonly DiscreteDynamicsWorld dynamicsWorld;
        public static readonly CollisionWorld collisionWorld;

        public static readonly DefaultCollisionConfiguration collisionConfiguration;

        static BulletPhysics()
        {
            broadphase = new DbvtBroadphase();
            
            collisionConfiguration = new DefaultCollisionConfiguration();
            dispatcher = new CollisionDispatcher(collisionConfiguration);
            
            // The actual physics solver
            solver = new SequentialImpulseConstraintSolver();
            
            // dynamic world.
            dynamicsWorld = new DiscreteDynamicsWorld(dispatcher, broadphase, solver, collisionConfiguration);
            // static world.
            collisionWorld = new CollisionWorld(dispatcher, broadphase, collisionConfiguration);
            
            dynamicsWorld.SetGravity(ref _gravity);
            

            // StaticPlaneShape staticPlane = new StaticPlaneShape(Bvector3.UnitY, 500);
            // 
            // CollisionObject plane = new CollisionObject()
            // {
            //     CollisionShape = staticPlane
            // };
            // 
            // collisionWorld.AddCollisionObject(plane);
        }

        /// <summary>
        /// for reaching hit objects transform just call rayResultCallback.GetTransform
        /// </summary>
        /// <param name="origin">self explaining</param>
        /// <param name="direction">it must be large value (non normalized)</param>
        /// <param name="rayResultCallback">contains ray hit point info (hit normal, hit position etc.)</param>
        /// <returns></returns>
        public static bool RayCastScreenPoint(out ClosestRayResultCallback rayResultCallback)
        {
            // http://www.opengl-tutorial.org/miscellaneous/clicking-on-objects/picking-with-a-physics-library/  
#if Editor
            OTKvector4 rayStartNdc = new OTKvector4
            (
                (Input.MousePosition().X / Screen.GetMainWindowSizeTupple().Item1 - 0.5f) / GameViewWindow.MousePosScaler.X * 2,
                (Input.MousePosition().Y / Screen.GetMainWindowSizeTupple().Item2 - 0.5f) / GameViewWindow.MousePosScaler.Y * 2,
                -1, 1
            );

            OTKvector4 rayEndNdc = new OTKvector4
            (
                (Input.MousePosition().X / Screen.GetMainWindowSizeTupple().Item1 - 0.5f) / GameViewWindow.MousePosScaler.X * 2,
                (Input.MousePosition().Y / Screen.GetMainWindowSizeTupple().Item2 - 0.5f) / GameViewWindow.MousePosScaler.Y * 2,
                0, 1
            );
#else
            OTKvector4 rayStartNdc = new OTKvector4
            (
                (Input.MousePosition().X / Screen.GetMainWindowSize().Item1 - 0.5f) * 2,
                (Input.MousePosition().Y / Screen.GetMainWindowSize().Item2 - 0.5f) * 2,
                -1,1
            );

            OTKvector4 rayEndNdc = new OTKvector4
            (
                (Input.MousePosition().X / Screen.GetMainWindowSize().Item1 - 0.5f) * 2,
                (Input.MousePosition().Y / Screen.GetMainWindowSize().Item2 - 0.5f) * 2,
                0, 1
            );
#endif
            OTKmatrix.Invert(Camera.main.GetGetProjectionMatrix(), out OTKmatrix invertedProjection);
            OTKmatrix.Invert(Camera.main.GetViewMatrix()         , out OTKmatrix invertedView);

            OTKvector4 rayStartCamera = invertedProjection * rayStartNdc;    rayStartCamera /= rayStartCamera.W;
            OTKvector4 rayStartWorld  = invertedView       * rayStartCamera; rayStartWorld  /= rayStartWorld.W;
            OTKvector4 rayEndCamera   = invertedProjection * rayEndNdc;      rayEndCamera   /= rayEndCamera.W;
            OTKvector4 rayEndWorld    = invertedView       * rayEndCamera;   rayEndWorld    /= rayEndWorld.W;

            // Faster way (just one inverse) try later cause wee need to control is it working or not
            // OTKmatrix.Invert(Camera.main.GetGetProjectionMatrix() * Camera.main.GetViewMatrix(), out OTKmatrix M);
            // OTKvector4 rayStartWorld = M * rayStartNdc; rayStartWorld /= rayStartWorld.W;
            // OTKvector4 rayEndWorld   = M * rayEndNdc;   rayEndWorld   /= rayEndWorld.W;

            Bvector3 rayDirection = (rayEndWorld.Xyz - rayStartWorld.Xyz).Tk2Bullet();
            Bvector3 rayOrigin    = rayStartWorld.Xyz.Tk2Bullet(); 

            rayResultCallback = new RayResult(ref rayOrigin, ref rayDirection);

            dynamicsWorld.RayTestRef(ref rayOrigin, ref rayDirection, rayResultCallback);

            return rayResultCallback.HasHit;
        }

        public static bool RayCastCameraMiddle(out RayResult rayResultCallback, float maxDistance = 10000)
        {
            OTKvector3 direction = (Camera.main.Front * maxDistance);
            return RayCast(ref Camera.main.Position, ref direction, out rayResultCallback);
        }

        /// <summary>
        /// for reaching hit objects transform just call rayResultCallback.GetTransform
        /// </summary>
        /// <param name="origin">self explaining</param>
        /// <param name="direction">it must be large value (non normalized)</param>
        /// <param name="rayResultCallback">contains ray hit point info (hit normal, hit position etc.)</param>
        /// <returns></returns>
        public static bool RayCast(ref OTKvector3 origin, ref OTKvector3 direction, out RayResult rayResultCallback)
        {
            Bvector3 originRef =  origin.Tk2Bullet();
            Bvector3 directionRef = direction.Tk2Bullet();

            rayResultCallback = new RayResult(ref originRef, ref directionRef);

            dynamicsWorld.RayTestRef(ref originRef, ref directionRef, rayResultCallback);

            return rayResultCallback.HasHit;
        }

        private static readonly float dt = .01f;
        private static float accumulator;

        internal static void Update(float frametime)
        {
            // dynamicsWorld.StepSimulation(frametime);

            accumulator += MathF.Min(frametime, .25f);
            
            while (accumulator >= dt)
            {
                dynamicsWorld.StepSimulation(dt);
                accumulator -= dt;
            }
        }

        #region convertations
        /// /ToBullet//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static ref Bvector3 Tk2Bullet(this OTKvector3 tk)    => ref Unsafe.AsRef<Bvector3>(Unsafe.AsPointer(ref tk));
        public static ref Bvector4 Tk2Bullet(this OTKvector4 tk)    => ref Unsafe.AsRef<Bvector4>(Unsafe.AsPointer(ref tk));
        public static ref Bquaternion Tk2Bullet(this OTKquaternion tk) => ref Unsafe.AsRef<Bquaternion>(Unsafe.AsPointer(ref tk));
        // public static ref Bmatrix Tk2Bullet(this OTKmatrix tk)        => ref Unsafe.AsRef<Bmatrix>(Unsafe.AsPointer(ref tk));
        /// /ToTK//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static ref OTKvector3 Bullet2TK(this Bvector3 tk)    => ref Unsafe.AsRef<OTKvector3>(Unsafe.AsPointer(ref tk));
        public static ref OTKvector4 Bullet2TK(this Bvector4 tk)    => ref Unsafe.AsRef<OTKvector4>(Unsafe.AsPointer(ref tk));
        public static ref OTKquaternion Bullet2TK(this Bquaternion tk) => ref Unsafe.AsRef<OTKquaternion>(Unsafe.AsPointer(ref tk));
        // public static ref OTKmatrix Bullet2TK(this Bmatrix tk)       => ref Unsafe.AsRef<OTKmatrix>(Unsafe.AsPointer(ref tk));
        /// /ToBulletRef///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static ref Bvector3 Tk2BulletRef(this ref OTKvector3 tk)     => ref Unsafe.AsRef<Bvector3>(Unsafe.AsPointer(ref tk));
        public static ref Bvector4 Tk2BulletRef(this ref OTKvector4 tk)     => ref Unsafe.AsRef<Bvector4>(Unsafe.AsPointer(ref tk));
        public static ref Bquaternion Tk2BulletRef(this ref OTKquaternion tk) => ref Unsafe.AsRef<Bquaternion>(Unsafe.AsPointer(ref tk));
        // public static ref Bmatrix Tk2BulletRef(this ref OTKmatrix tk)           => ref Unsafe.AsRef<Bmatrix>(Unsafe.AsPointer(ref tk));
        /// /ToTKRef///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static ref OTKvector3 Bullet2TKRef(this ref Bvector3 tk)    => ref Unsafe.AsRef<OTKvector3>(Unsafe.AsPointer(ref tk));
        public static ref OTKvector4 Bullet2TKRef(this ref Bvector4 tk)    => ref Unsafe.AsRef<OTKvector4>(Unsafe.AsPointer(ref tk));
        public static ref OTKquaternion Bullet2TKRef(this ref Bquaternion tk) => ref Unsafe.AsRef<OTKquaternion>(Unsafe.AsPointer(ref tk));
        // public static ref OTKmatrix Bullet2TKRef(this ref Bmatrix tk)         => ref Unsafe.AsRef<OTKmatrix>(Unsafe.AsPointer(ref tk));
        /// ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #endregion// convertations

        // dispose cause physics has native codes they need to be cleaned
        public static void Dispose()
        {
            CollisionObject collisionObject;
            RigidBody rigidBody;

            for (int i = 0; i < dynamicsWorld.NumCollisionObjects; i++)
            {
                collisionObject = dynamicsWorld.CollisionObjectArray[i];
                rigidBody       = RigidBody.Upcast(collisionObject);
                dynamicsWorld.RemoveCollisionObject(collisionObject);
                collisionObject.Dispose();
                rigidBody?.Dispose();
            }

            // maybe do: you can clear collision shapes here

            collisionWorld.Dispose();
            dynamicsWorld.Dispose();
            solver.Dispose();
            dispatcher.Dispose();
            broadphase.Dispose();
            collisionConfiguration.Dispose();
        }
    }
}
