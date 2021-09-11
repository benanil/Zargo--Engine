using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using BepuPhysics.Trees;
using Collections.Pooled;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace ZargoEngine.Physics
{
    unsafe struct NarrowPhaseCallbacks : INarrowPhaseCallbacks,IDisposable
    {
        /// <summary>
        /// Performs any required initialization logic after the Simulation instance has been constructed.
        /// </summary>
        /// <param name="simulation">Simulation that owns these callbacks.</param>
        public void Initialize(Simulation simulation)
        {
            //Often, the callbacks type is created before the simulation instance is fully constructed, so the simulation will call this function when it's ready.
            //Any logic which depends on the simulation existing can be put here.
        }

        /// <summary>
        /// Chooses whether to allow contact generation to proceed for two overlapping collidables.
        /// </summary>
        /// <param name="workerIndex">Index of the worker that identified the overlap.</param>
        /// <param name="a">Reference to the first collidable in the pair.</param>
        /// <param name="b">Reference to the second collidable in the pair.</param>
        /// <returns>True if collision detection should proceed, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b)
        {
            //Before creating a narrow phase pair, the broad phase asks this callback whether to bother with a given pair of objects.
            //This can be used to implement arbitrary forms of collision filtering. See the RagdollDemo or NewtDemo for examples.
            //Here, we'll make sure at least one of the two bodies is dynamic.
            //The engine won't generate static-static pairs, but it will generate kinematic-kinematic pairs.
            //That's useful if you're trying to make some sort of sensor/trigger object, but since kinematic-kinematic pairs
            //can't generate constraints (both bodies have infinite inertia), simple simulations can just ignore such pairs.
            return a.Mobility == CollidableMobility.Dynamic || b.Mobility == CollidableMobility.Dynamic;
        }

        /// <summary>
        /// Chooses whether to allow contact generation to proceed for the children of two overlapping collidables in a compound-including pair.
        /// </summary>
        /// <param name="pair">Parent pair of the two child collidables.</param>
        /// <param name="childIndexA">Index of the child of collidable A in the pair. If collidable A is not compound, then this is always 0.</param>
        /// <param name="childIndexB">Index of the child of collidable B in the pair. If collidable B is not compound, then this is always 0.</param>
        /// <returns>True if collision detection should proceed, false otherwise.</returns>
        /// <remarks>This is called for each sub-overlap in a collidable pair involving compound collidables. If neither collidable in a pair is compound, this will not be called.
        /// For compound-including pairs, if the earlier call to AllowContactGeneration returns false for owning pair, this will not be called. Note that it is possible
        /// for this function to be called twice for the same subpair if the pair has continuous collision detection enabled; 
        /// the CCD sweep test that runs before the contact generation test also asks before performing child pair tests.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB)
        {
            //This is similar to the top level broad phase callback above. It's called by the narrow phase before generating
            //subpairs between children in parent shapes. 
            //This only gets called in pairs that involve at least one shape type that can contain multiple children, like a Compound.
            return true;
        }

        /// <summary>
        /// Provides a notification that a manifold has been created between the children of two collidables in a compound-including pair.
        /// Offers an opportunity to change the manifold's details. 
        /// </summary>
        /// <param name="workerIndex">Index of the worker thread that created this manifold.</param>
        /// <param name="pair">Pair of collidables that the manifold was detected between.</param>
        /// <param name="childIndexA">Index of the child of collidable A in the pair. If collidable A is not compound, then this is always 0.</param>
        /// <param name="childIndexB">Index of the child of collidable B in the pair. If collidable B is not compound, then this is always 0.</param>
        /// <param name="manifold">Set of contacts detected between the collidables.</param>
        /// <returns>True if this manifold should be considered for constraint generation, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ConfigureContactManifold(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB, ref ConvexContactManifold manifold)
        {
           
            return true;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair pair, ref TManifold manifold, out PairMaterialProperties pairMaterial) where TManifold : struct, IContactManifold<TManifold>
        {
            pairMaterial.FrictionCoefficient = 1f;
            pairMaterial.MaximumRecoveryVelocity = 2f;
            pairMaterial.SpringSettings = new SpringSettings(30, 1);
            return true;
        }
    }

    public struct RayHit
    {
        public Vector3 normal;
        public float T;
        public CollidableReference collidable;
        public bool hit;

        public RayHit(Vector3 Normal,CollidableReference collidable, float t, bool hit)
        {
            normal = Normal; this.collidable = collidable; this.T = t; this.hit = hit;
        }
    }

    public unsafe struct HitHandler : IRayHitHandler
    {
        public PooledList<RayHit> Hits;
        
        public override string ToString() {
            StringBuilder sb = new StringBuilder("HitHandeller");
            sb.Append($"\n Hits.Length: {Hits}");
            return sb.ToString();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowTest(CollidableReference collidable) {
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowTest(CollidableReference collidable, int childIndex) {
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnRayHit(in RayData ray, ref float maximumT, float t, in Vector3 normal, CollidableReference collidable, int childIndex)
        {
            maximumT = t;

            if (Hits == null) Hits = new PooledList<RayHit>();

            RayHit hit = new RayHit(normal, collidable, t, true);
            
            Hits.Add(hit);
        }
    }

    struct SceneSweepHitHandler : ISweepHitHandler
    {
        public Vector3 HitLocation;
        public Vector3 HitNormal;
        public float T;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowTest(CollidableReference collidable)
        {
            return collidable.Mobility == CollidableMobility.Static;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool AllowTest(CollidableReference collidable, int child)
        {
            return collidable.Mobility == CollidableMobility.Static;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnHit(ref float maximumT, float t, in Vector3 hitLocation, in Vector3 hitNormal, CollidableReference collidable)
        {
            //Changing the maximum T value prevents the traversal from visiting any leaf nodes more distant than that later in the traversal.
            //It is effectively an optimization that you can use if you only care about the time of first impact.
            if (t < maximumT)
                maximumT = t;
            if (t < T)
            {
                T = t;
                HitLocation = hitLocation;
                HitNormal = hitNormal;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnHitAtZeroT(ref float maximumT, CollidableReference collidable)
        {
            maximumT = 0;
            T = 0;
            HitLocation = new Vector3();
            HitNormal = new Vector3();
        }
    
    }

    // Note that the engine does not require any particular form of gravity- it, like all the contact callbacks, is managed by a callback.
    public struct PoseIntegratorCallbacks : IPoseIntegratorCallbacks
    {
        public Vector3 gravity;
        private Vector3 gravityDt;

        /// <summary>
        /// Gets how the pose integrator should handle angular velocity integration.
        /// </summary>
        public AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.Nonconserving; //Don't care about fidelity in this demo!

        public PoseIntegratorCallbacks(Vector3 _gravity) : this()
        {
            gravity = _gravity;
        }

        /// <summary>
        /// Called prior to integrating the simulation's active bodies. When used with a substepping timestepper, this could be called multiple times per frame with different time step values.
        /// </summary>
        /// <param name="dt">Current time step duration.</param>
        public void PrepareForIntegration(float dt)
        {
            // No reason to recalculate gravity * dt for every body; just cache it ahead of time.
            gravityDt = gravity * dt;
        }

        /// <summary>
        /// Callback called for each active body within the simulation during body integration.
        /// </summary>
        /// <param name="bodyIndex">Index of the body being visited.</param>
        /// <param name="pose">Body's current pose.</param>
        /// <param name="localInertia">Body's current local inertia.</param>
        /// <param name="workerIndex">Index of the worker thread processing this body.</param>
        /// <param name="velocity">Reference to the body's current velocity to integrate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IntegrateVelocity(int bodyIndex, in RigidPose pose, in BodyInertia localInertia, int workerIndex, ref BodyVelocity velocity)
        {
            // Note that we avoid accelerating kinematics. Kinematics are any body with an inverse mass of zero (so a mass of ~infinity). No force can move them.
            if (localInertia.InverseMass > 0)
            {
                velocity.Linear = velocity.Linear + gravityDt;
            }
        }

        public void Initialize(Simulation simulation)
        {

        }
    }
}
