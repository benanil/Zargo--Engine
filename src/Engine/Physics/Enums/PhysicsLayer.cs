
using System;

namespace ZargoEngine
{ 
    [Flags]
    public enum PhysicsLayer : byte
    { 
        none      = 1 << 0,
        player    = 1 << 1,
        water     = 1 << 2,
        wood      = 1 << 3,
        enemy     = 1 << 4, 
        stone     = 1 << 5,
        editor    = 1 << 6, 
        aditional = 1 << 7
    }
}