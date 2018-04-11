using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Win32.SafeHandles
{
#if NET45
    /// <summary>
    /// A wrapper for a Windows Process handle.
    /// </summary>
    public class SafeProcessHandle: SafeHandleZeroOrMinusOneIsInvalid
    {
        /// <summary>
        /// Constructs a new <see cref="SafeProcessHandle"/>.
        /// </summary>
        public SafeProcessHandle ()
            : base(true)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="SafeProcessHandle"/>.
        /// </summary>
        /// <param name="existingHandle">The process handle.</param>
        /// <param name="ownsHandle"><c>true</c> if the wrapper should release the handle.</param>
        public SafeProcessHandle ( IntPtr existingHandle, bool ownsHandle )
            :base(ownsHandle)
        {
            SetHandle(existingHandle);
        }

        /// <summary>
        /// Releases the handle.
        /// </summary>
        /// <returns><c>true</c> if the handle was released.</returns>
        protected override bool ReleaseHandle ()
        {
            return Interop.Kernel32.CloseHandle(handle);
        }
    }
#endif
}
