using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

internal partial class Interop
{
    [System.Security.SuppressUnmanagedCodeSecurity]
    internal static partial class User32
    {
        [DllImport("user32.dll", SetLastError = true, EntryPoint = "UserHandleGrantAccess")]
        internal static extern bool UserHandleGrantAccess ( SafeHandle hUserHandle, SafeJobObjectHandle hJob, BOOL bGrant );

        [DllImport("user32.dll", SetLastError = true, EntryPoint = "UserHandleGrantAccess")]
        internal static extern bool UserHandleGrantAccess ( IntPtr hUserHandle, SafeJobObjectHandle hJob, BOOL bGrant );
    }
}
