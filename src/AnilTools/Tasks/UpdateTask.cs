using System;

namespace ZargoEngine.AnilTools
{
    public class UpdateTask : TaskBase
    {
        private event Action action;
        private readonly Func<bool> condition;

        public override bool IsFinished()
        {
            return !condition.Invoke();
        }

        public override bool Proceed()
        {
            action.Invoke();
            return IsFinished();
        }

        public UpdateTask(Action action, Func<bool> condition,
                          UpdateType updateType, Action endAction, 
                          string name = "") : base(updateType, endAction,name)
        {
            currentTask = this;
            this.action = action;
            this.condition = condition;
            this.condition = condition;
        }
    }
}
