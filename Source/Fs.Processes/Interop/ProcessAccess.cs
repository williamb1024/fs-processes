using System;
using System.Collections.Generic;
using System.Text;

internal partial class Interop
{
    internal static partial class ProcessAccess
    {
        internal const int Terminate = 0x0001;
        internal const int VmRead = 0x0010;
        internal const int SetQuota = 0x0100;
        internal const int SetInformation = 0x0200;
        internal const int QueryInformation = 0x0400;
        internal const int QueryLimitedInformation = 0x1000;
        internal const int AllAccess = StandardRightsRequired | Synchronize | 0xFFF;

        internal const int StandardRightsRequired = 0x000F0000;
        internal const int Synchronize = 0x00100000;
    }
}
