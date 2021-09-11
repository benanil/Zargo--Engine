
using System;

namespace ZargoEngine.AnilTools
{
    // todo tasklar için özel editor window yap
    public abstract class TaskBase : ITickable
    {
        protected bool update = true;
        protected readonly UpdateType updateType;
        public event Action endAction;

        protected readonly TaskQueue<TaskBase> taskQueue = new TaskQueue<TaskBase>();

        protected TaskBase currentTask;

        public readonly string name; // for detecting task by name

        public void Add<Task>(Task task) where Task : TaskBase
        {
            taskQueue.AddTask(task);
        }

        public virtual void Tick()
        {
            if (!update) return;

            if (currentTask.Proceed()){
                currentTask = taskQueue.RequestTask();
                if(currentTask == null) Dispose();
            }
        }

        public abstract bool IsFinished();

        public virtual bool Proceed()
        {
            return IsFinished();
        }

        public void SetUpdate(bool value){
            update = value;
        }

        protected TaskBase(UpdateType updateType, Action endAction,string name = "")
        {
            this.name = name;
            currentTask = this;
            this.endAction += endAction;
            this.updateType = updateType;
        }
        
        public void Dispose()
        {
            endAction.Invoke();
            ZargoUpdate.Remove(this, updateType);
            GC.SuppressFinalize(this);
        }
    }
}
