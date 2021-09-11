
using System;

namespace ZargoEngine.AnilTools
{
    public class TranslateTask
    {
        private readonly RotateTask rotateTask;
        private readonly ScaleTask scaleTask;
        private readonly PositionTask positionTask;

        private Action endAction;

        private bool rotated, scaled, moved;

        private UpdateType updateType;

        public void Start()
        {
            if (scaleTask != null)    ZargoUpdate.Register(scaleTask, updateType);
            if (positionTask != null) ZargoUpdate.Register(positionTask, updateType);
            if (rotateTask != null)   ZargoUpdate.Register(rotateTask, updateType);
        }

        private void CheckEnd(){
            if (rotated & scaled & moved){
                endAction.Invoke();
            }
        }

        public TranslateTask(RotateTask rotateTask, ScaleTask scaleTask,
                             PositionTask positionTask,Action endAction, 
                             UpdateType updateType)
        {
            moved = positionTask == null;
            scaled = scaleTask == null;
            rotated = rotateTask == null;
            scaleTask.endAction += CheckEnd;
            positionTask.endAction += CheckEnd;
            rotateTask.endAction += CheckEnd;
            this.rotateTask = rotateTask;
            this.scaleTask = scaleTask;
            this.positionTask = positionTask;
            this.endAction = endAction;
            this.updateType = updateType;
        }
    }
}
