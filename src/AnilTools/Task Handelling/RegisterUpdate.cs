using System;

namespace ZargoEngine.AnilTools
{
    public static class RegisterUpdate
    {
        public static UpdateTillTask UpdateTill(float duration, Action action, bool isPlus = true, 
                                            Action endAction = null, UpdateType updateType = UpdateType.Update)
        {
            var task = new UpdateTillTask(duration,action,isPlus, endAction);
            ZargoUpdate.Register(task, updateType);
            return task;
        }

        public static UpdateTask UpdateWhile(Action action, Func<bool> condition, 
                                             UpdateType updateType = UpdateType.Update, 
                                             Action endAction = null)
        {
            var task = new UpdateTask(action, condition, updateType, endAction);
            ZargoUpdate.Register(task, updateType);
            return task;
        }
    }
}
