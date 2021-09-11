

using System;

namespace ZargoEngine.AnilTools
{
    public interface ITickable : IDisposable
    {
        public void Tick();
    }
}
