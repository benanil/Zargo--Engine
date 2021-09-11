
using System.Collections.Generic;

namespace ZargoEngine.AnilTools
{
    public static class ZargoUpdate
    {
        static readonly List<ITickable> updateables = new List<ITickable>();
        static readonly List<ITickable> lateUpdateables = new List<ITickable>();
        static readonly List<ITickable> fixedUpdateables = new List<ITickable>();

        /// <summary>DONT Call it other than game main Loop </summary>
        internal static void Update()
        {
            for (int i = 0; i < updateables.Count; i++){
                updateables[i].Tick();
            }
        }

        /// <summary>DONT Call it other than game main Loop </summary>
        internal static void LateUpdate()
        {
            
            for (int i = 0; i < updateables.Count; i++)
            {
                updateables[i].Tick();
            }
        }

        /// <summary>DONT Call it other than game main Loop </summary>
        internal static void FixedUpdate()
        {
            for (int i = 0; i < updateables.Count; i++)
            {
                updateables[i].Tick();
            }
        }

        public static void Register(ITickable tickable, UpdateType updateType = UpdateType.Update)
        {
            switch (updateType)
            {
                case UpdateType.Update:      updateables.Add(tickable);      break;
                case UpdateType.LateUpdate:  lateUpdateables.Add(tickable);  break;
                case UpdateType.fixedUpdate: fixedUpdateables.Add(tickable); break;
            }
        }

        public static void Remove(ITickable tickable, UpdateType updateType = UpdateType.Update)
        {
            switch (updateType)
            {
                case UpdateType.Update:     updateables.Remove(tickable);       break;
                case UpdateType.LateUpdate: lateUpdateables.Remove(tickable);   break;                 
                case UpdateType.fixedUpdate: fixedUpdateables.Remove(tickable); break;
            }
        }
    }
}
