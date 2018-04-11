using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.Win32.SafeHandles
{
    /// <summary>
    /// A <see cref="SafeHandle"/> for a Windows Thread handle.
    /// </summary>
    public class SafeThreadHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        /// <summary>
        /// Creates a new <see cref="SafeThreadHandle"/> instance.
        /// </summary>
        public SafeThreadHandle ()
            :base(true)
        {
        }

        /// <summary>
        /// Creates a new <see cref="SafeThreadHandle"/> instance for a specific handle.
        /// </summary>
        /// <param name="existingHandle">The handle</param>
        /// <param name="ownsHandle"><c>true</c> if the instance should release the handle.</param>
        public SafeThreadHandle ( IntPtr existingHandle, bool ownsHandle ) 
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
