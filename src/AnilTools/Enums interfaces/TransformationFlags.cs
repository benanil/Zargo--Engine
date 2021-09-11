using System;

namespace ZargoEngine.AnilTools
{
    [Flags]
    public enum TransformationFlags : byte
    {
        none = 0, isLocal = 1 , isPlus = 2,
        position = 4,rotation = 8, scale = 16
    }
}
