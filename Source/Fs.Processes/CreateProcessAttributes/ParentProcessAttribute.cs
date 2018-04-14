using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace Fs.Processes
{
    /// <summary>
    /// Specifies the parent process to use when creating a new process.
    /// </summary>
    public class ParentProcessAttribute : CreateProcessAttributeList.CreateProcessAttribute
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ParentProcessAttribute"/> class.
        /// </summary>
        /// <param name="processHandle">The handle of the process to use as the parent of the newly created process.</param>
        /// <remarks>The <see cref="ParentProcessAttribute"/> instance does not take ownership of the <paramref name="processHandle"/> handle. It
        /// is the caller's responsibility to ensure the handle remains valid until it is no longer needed and to eventually dispose of the
        /// handle.</remarks>
        public ParentProcessAttribute ( SafeProcessHandle processHandle )
        {
            if (processHandle == null)
                throw new ArgumentNullException(nameof(processHandle));

            Handle = processHandle;
        }

        internal override int GetAttributeSize ( int attributeIndex )
        {
            return IntPtr.Size;
        }

        internal override void SetAttributeData ( int attributeIndex, IntPtr dataPtr, IntPtr listPtr )
        {
            Marshal.WriteIntPtr(dataPtr, Handle.DangerousGetHandle());

            SetAttributeData(listPtr, Interop.Kernel32.ProcThreadAttributes.ParentProcess, dataPtr, IntPtr.Size);
        }

        /// <summary>
        /// Gets the handle of the process to use as the parent of the newly created process.
        /// </summary>
        public SafeProcessHandle Handle { get; }
    }
}
