using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
                case Interop.Errors.ERROR_FILE_NOT_FOUND:
                    return new FileNotFoundException();

                case Interop.Errors.ERROR_PATH_NOT_FOUND:
                    return new DirectoryNotFoundException();

                case Interop.Errors.ERROR_ACCESS_DENIED:
                    return new UnauthorizedAccessException();

                case Interop.Errors.ERROR_FILENAME_EXCED_RANGE:
                    return new PathTooLongException();

                case Interop.Errors.ERROR_INVALID_DRIVE:
                    return new DriveNotFoundException();

                case Interop.Errors.ERROR_OPERATION_ABORTED:
                    return new OperationCanceledException();

                default:
                    return new Win32Exception(errorCode);
            }
        }
    }
}
