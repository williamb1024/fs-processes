using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.Win32.SafeHandles
{
    /// <summary>
    /// A <see cref="SafeHandle"/> for Windows IoCompletionPort.
    /// </summary>
    public class SafeIoCompletionPortHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        /// <summary>
        /// Creates a new <see cref="SafeIoCompletionPortHandle"/>.
        /// </summary>
        public SafeIoCompletionPortHandle()
            :base(true)
        {
        }

        /// <summary>
        /// Creates a new <see cref="SafeIoCompletionPortHandle"/> instance for a specific handle.
        /// </summary>
        /// <param name="existingHandle">The handle</param>
        /// <param name="ownsHandle"><c>true</c> if the instance should release the handle.</param>
        public SafeIoCompletionPortHandle ( IntPtr existingHandle, bool ownsHandle )
            : base(ownsHandle)
        {
            SetHandle(existingHandle);
        }

        /// <summary>
        /// Releases the associated handle.
        /// </summary>
        /// <returns><c>true</c> if the handle was released.</returns>
        protected override bool ReleaseHandle ()
        {
            return Interop.Kernel32.CloseHandle(handle);
        }
    }
}
