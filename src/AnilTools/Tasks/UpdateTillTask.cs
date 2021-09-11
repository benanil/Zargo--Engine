using System;

namespace ZargoEngine.AnilTools
{
    public class UpdateTillTask : TaskBase
    {
        private readonly float targetTime;
        private readonly Action action;

        public override bool IsFinished()
        {
            return Time.time >= targetTime;
        }

        public override bool Proceed()
        {
            action.Invoke();
            return IsFinished();
        }

        public UpdateTillTask(float duration, Action action,
                              bool isPlus = true, Action endAction = null, 
                              string name = "") : base(UpdateType.Update, endAction,name)
        {
            currentTask = this;
            targetTime = isPlus ? Time.time + duration : duration;
            this.action = action;
        }
    }
}
