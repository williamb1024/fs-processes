using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace Fs.Processes
{
    internal static class Errors
    {
        internal static Exception Win32Error ()
        {
            return Win32Error(Marshal.GetLastWin32Error());
        }

        internal static Exception Win32Error ( int errorCode )
        {
            switch (errorCode)
            {
                case Interop.Errors.ERROR_ACCESS_DENIED:
                    return new UnauthorizedAccessException();

                default:
                    return new Win32Exception(errorCode);
            }
        }
    }
}
