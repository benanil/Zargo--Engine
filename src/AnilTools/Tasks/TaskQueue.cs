

using System;
using System.Collections.Generic;

namespace ZargoEngine.AnilTools
{
    public class TaskQueue <T> where T : IDisposable
    {
        private readonly Queue<T> tasks = new Queue<T>();

        public T RequestTask()
        {
            if (tasks.Count > 0) return tasks.Dequeue();
            return default;
        }

        public void AddTask(T task)
        {
            tasks.Enqueue(task);
        }
        
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
