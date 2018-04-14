using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

internal partial class Interop
{
    internal static partial class Advapi32
    {
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true, BestFitMapping = false, EntryPoint = "CreateProcessWithLogonW")]
        internal static extern bool CreateProcessWithLogonW (
            string userName,
            string domain,
            IntPtr password,
            LogonFlags logonFlags,
            string appName,
            StringBuilder cmdLine,
            int creationFlags,
            IntPtr environmentBlock,
            string lpCurrentDirectory,
            ref Interop.Kernel32.STARTUPINFO lpStartupInfo,
            ref Interop.Kernel32.PROCESS_INFORMATION lpProcessInformation );

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true, BestFitMapping = false, EntryPoint = "CreateProcessWithLogonW")]
        internal static extern bool CreateProcessWithLogonW (
            string userName,
            string domain,
            IntPtr password,
            LogonFlags logonFlags,
            string appName,
            StringBuilder cmdLine,
            int creationFlags,
            IntPtr environmentBlock,
            string lpCurrentDirectory,
            ref Interop.Kernel32.STARTUPINFOEX lpStartupInfo,
            ref Interop.Kernel32.PROCESS_INFORMATION lpProcessInformation );

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true, BestFitMapping = false, EntryPoint = "CreateProcessWithTokenW")]
        internal static extern bool CreateProcessWithTokenW (
            SafeAccessTokenHandle hToken,
            LogonFlags logonFlags,
            string lpApplicationName,
            StringBuilder lpCommandLine,
            int dwCreationFlags,
            IntPtr environmentBlock,
            string lpCurrentDirectory,
            ref Interop.Kernel32.STARTUPINFOEX lpStartupInfo,
            ref Interop.Kernel32.PROCESS_INFORMATION lpProcessInformation );

        [Flags]
        internal enum LogonFlags
        {
            LOGON_WITH_PROFILE = 0x00000001,
            LOGON_NETCREDENTIALS_ONLY = 0x00000002
        }

        [DllImport("advapi32.dll", SetLastError = true, ExactSpelling = true, EntryPoint = "OpenProcessToken")]
        internal static extern bool OpenProcessToken ( IntPtr ProcessHandle, uint DesiredAccess, out SafeAccessTokenHandle TokenHandle );

        [DllImport("advapi32.dll", SetLastError = true, ExactSpelling = true, EntryPoint = "OpenProcessToken")]
        internal static extern bool OpenProcessToken ( SafeProcessHandle ProcessHandle, uint DesiredAccess, out SafeAccessTokenHandle TokenHandle );
    }
}
