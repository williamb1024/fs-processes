using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.Win32.SafeHandles
{
    /// <summary>
    /// A <see cref="SafeHandle"/> for Windows JobObjects.
    /// </summary>
    public class SafeJobObjectHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        /// <summary>
        /// Creates a new <see cref="SafeJobObjectHandle"/>.
        /// </summary>
        public SafeJobObjectHandle()
            :base(true)
        {
        }

        /// <summary>
        /// Creates a new <see cref="SafeJobObjectHandle"/> for a specific handle.
        /// </summary>
        /// <param name="existingHandle">The handle to wrap.</param>
        /// <param name="ownsHandle"><c>true</c> if this instance should release the handle.</param>
        public SafeJobObjectHandle ( IntPtr existingHandle, bool ownsHandle )
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
