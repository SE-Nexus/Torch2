using System;
using System.Collections.Generic;
using System.Text;

namespace Torch2API.Models.Commands
{
    [Flags]
    public enum CommandTypeEnum
    {
        None = 0,

        Ingame = 1 << 0, // 1
        Console = 1 << 1, // 2
        Discord = 1 << 2, // 4
        WebPanel = 1 << 3, // 8
        Debug = 1 << 4, // 16 Not sure if this is needed

        // Context combinations
        All = Ingame | Console | Discord | WebPanel | Debug,

        // Admin commands are allowed everywhere EXCEPT ingame
        AdminOnly = All & ~Ingame
    }
}
