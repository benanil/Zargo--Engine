using System;

using BepuPhysics;
using BepuUtilities.Memory;
using BepuUtilities;
using System.Threading;
using BepuPhysics.Collidables;

using System.Diagnostics;
using ZargoEngine.Helper;
using System.Linq;

namespace ZargoEngine.Physics
{
    using OTKvector3 = OpenTK.Mathematics.Vector3;
    using SYSvector3 = System.Numerics.Vector3;
    
    public static class BepuHandle
    {
        // general
        public readonly static Simulation simulation;
        public readonly static BufferPool bufferPool;
        public readonly static Shapes shapes;
        // time step
        public static readonly float TargetTimeStep = 0.01f;
        public static long physicsElapsed = 0;
        public static long totalElapsed = 0;
        
        private readonly static Thread thread;
        private static bool isThreadActive = true;
        
        static BepuHandle()
        {
            bufferPool = new BufferPool();
            shapes = new Shapes(bufferPool, 50);

            var narrow = new NarrowPhaseCallbacks();
            narrow.Initialize(simulation);
                 
            simulation = Simulation.Create(bufferPool,narrow, new PoseIntegratorCallbacks(new SYSvector3(0,-9.3f,0)),new PositionLastTimestepper());
            thread = new Thread(Update);
            thread.Start();
        }


        public unsafe static bool Raycast(in Ray ray, out HitHandler hitHandler, in float maxDistance = 500)
        {
            hitHandler = new HitHandler();
            if (simulation == null) return false;

            simulation.RayCast(ray.origin, ray.direction, maxDistance, ref hitHandler);

            if (hitHandler.Hits == null || hitHandler.Hits.Count < 1 ) return false;

            return hitHandler.Hits.Any(x => x.hit);
        }

        internal static void Update()
        {
            Stopwatch stopWatch = new Stopwatch();
            var dispatcher = new SimpleThreadDispatcher(Environment.ProcessorCount);
            long timeRunned;
            while (isThreadActive)
            {
                stopWatch.Reset();
                simulation.Timestep(TargetTimeStep, dispatcher);
                timeRunned = stopWatch.ElapsedMilliseconds;
                physicsElapsed = timeRunned;
                Thread.Sleep(Convert.ToInt32(MathF.Max(0, (TargetTimeStep * 1000) - timeRunned)));
                totalElapsed = stopWatch.ElapsedMilliseconds;
                stopWatch.Stop();
            }
        }

        public class SimpleThreadDispatcher : IThreadDispatcher, IDisposable
        {
            int threadCount;
            public int ThreadCount => threadCount;
            struct Worker
            {
                public Thread Thread;
                public AutoResetEvent Signal;
            }

            Worker[] workers;
            AutoResetEvent finished;

            BufferPool[] bufferPools;

            public SimpleThreadDispatcher(int threadCount)
            {
                this.threadCount = threadCount;
                workers = new Worker[threadCount - 1];
                for (int i = 0; i < workers.Length; ++i)
                {
                    workers[i] = new Worker { Thread = new Thread(WorkerLoop), Signal = new AutoResetEvent(false) };
                    workers[i].Thread.IsBackground = true;
                    workers[i].Thread.Start(workers[i].Signal);
                }
                finished = new AutoResetEvent(false);
                bufferPools = new BufferPool[threadCount];
                for (int i = 0; i < bufferPools.Length; ++i)
                {
                    bufferPools[i] = new BufferPool();
                }
            }

            void DispatchThread(int workerIndex)
            {
                workerBody(workerIndex);

                if (Interlocked.Increment(ref completedWorkerCounter) == threadCount)
                {
                    finished.Set();
                }
            }

            volatile Action<int> workerBody;
            int workerIndex;
            int completedWorkerCounter;

            void WorkerLoop(object untypedSignal)
            {
                var signal = (AutoResetEvent)untypedSignal;
                while (true)
                {
                    signal.WaitOne();
                    if (disposed)
                        return;
                    DispatchThread(Interlocked.Increment(ref workerIndex) - 1);
                }
            }

            void SignalThreads()
            {
                for (int i = 0; i < workers.Length; ++i)
                {
                    workers[i].Signal.Set();
                }
            }

            public void DispatchWorkers(Action<int> workerBody)
            {
                workerIndex = 1; //Just make the inline thread worker 0. While the other threads might start executing first, the user should never rely on the dispatch order.
                completedWorkerCounter = 0;
                this.workerBody = workerBody;
                SignalThreads();
                //Calling thread does work. No reason to spin up another worker and block this one!
                DispatchThread(0);
                finished.WaitOne();
                this.workerBody = null;
            }

            volatile bool disposed;
            public void Dispose()
            {
                if (!disposed)
                {
                    disposed = true;
                    SignalThreads();
                    for (int i = 0; i < bufferPools.Length; ++i)
                    {
                        bufferPools[i].Clear();
                    }
                    foreach (var worker in workers)
                    {
                        worker.Thread.Join();
                        worker.Signal.Dispose();
                    }
                }
            }

            public BufferPool GetThreadMemoryPool(int workerIndex)
            {
                return bufferPools[workerIndex];
            }
        }

        public static void Dispose()
        {
            isThreadActive = false;
            simulation.Dispose();
            bufferPool.Clear();
            shapes.Clear();
        }
    }

    public readonly struct Ray
    {
        public readonly SYSvector3 origin;
        public readonly SYSvector3 direction;

        public Ray(OTKvector3 origin, OTKvector3 direction)
        {
            this.origin = origin.ToSystemRef();
            this.direction = direction.ToSystemRef();
        }
    }
}
