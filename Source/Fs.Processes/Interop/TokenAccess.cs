using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal partial class Interop
{
    internal static partial class TokenAccess
    {
        internal const uint AssignPrimary = 0x0001;
        internal const uint Duplicate = 0x0002;
        internal const uint Impersonate = 0x0004;
        internal const uint Query = 0x0008;
        internal const uint QuerySource = 0x0010;
        internal const uint AdjustPrivileges = 0x0020;
        internal const uint AdjustGroups = 0x0040;
        internal const uint AdjustDefault = 0x0080;
        internal const uint AdjustSessionId = 0x0100;
    }
}
