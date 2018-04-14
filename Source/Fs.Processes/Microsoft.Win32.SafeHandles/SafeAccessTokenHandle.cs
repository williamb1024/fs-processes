#if NETSTANDARD2_0
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Win32.SafeHandles
{
    /// <summary>
    /// A safe handle wrapper for a Windows Token handle.
    /// </summary>
    public class SafeAccessTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeAccessTokenHandle ()
            : base(true)
        {
        }

        /// <summary>
        /// Initializes and instance of the class.
        /// </summary>
        /// <param name="handle">The handle to wrap</param>
        public SafeAccessTokenHandle ( IntPtr handle )
            : base(true)
        {
            SetHandle(handle);
        }

        /// <summary>
        /// Releases the handle
        /// </summary>
        /// <returns><c>true</c> if the handle was released</returns>
        protected override bool ReleaseHandle ()
        {
            return Interop.Kernel32.CloseHandle(handle);
        }
    }
}
#endif