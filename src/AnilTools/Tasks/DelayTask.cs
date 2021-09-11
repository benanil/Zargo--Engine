using System;

namespace ZargoEngine.AnilTools
{
    public class DelayTask : TaskBase
    {
        private float delay;

        public DelayTask(float delay, Action endAction, string name = "") : base(UpdateType.Update, endAction, name)
        {
            currentTask = this;
            this.delay = delay;
        }

        public override bool IsFinished()
        {
            return delay <= 0;
        }

        public override bool Proceed()
        {
            delay -= Time.DeltaTime;
            return IsFinished();
        }
    }

    public static class Delay
    {
        public static DelayTask delay(float time, Action endAction)
        {
            var task = new DelayTask(time, endAction);
            return task;
        }
    }

}
