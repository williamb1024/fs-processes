using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Fs.Processes
{
    /// <summary>
    /// Specifies a set of handles to be inherited by the child process.
    /// </summary>
    public class InheritHandlesAttribute : CreateProcessAttributeList.CreateProcessAttribute, IEnumerable<IntPtr>
    {
        private readonly List<IntPtr> _handles;

        /// <summary>
        /// Creates a new instance of the <see cref="InheritHandlesAttribute"/> class.
        /// </summary>
        public InheritHandlesAttribute ()
        {
            _handles = new List<IntPtr>();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="InheritHandlesAttribute"/> class with the specified inherited handles.
        /// </summary>
        /// <param name="safeHandles">The handles to be inherited by the child process.</param>
        public InheritHandlesAttribute ( IEnumerable<SafeHandle> safeHandles )
        {
            if (safeHandles == null)
                throw new ArgumentNullException(nameof(safeHandles));

            _handles = safeHandles.Select(h => h.DangerousGetHandle()).ToList();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="InheritHandlesAttribute"/> class with the specified inherited handles.
        /// </summary>
        /// <param name="handles">The handles to be inherited by the child process.</param>
        public InheritHandlesAttribute ( IEnumerable<IntPtr> handles )
        {
            if (handles == null)
                throw new ArgumentNullException(nameof(handles));

            _handles = handles.ToList();
        }

        /// <summary>
        /// Adds the <paramref name="handle"/> handle to the list of inherited handles.
        /// </summary>
        /// <param name="handle">The handle to add.</param>
        public void Add ( IntPtr handle )
        {
            _handles.Add(handle);
        }

        /// <summary>
        /// Adds the <paramref name="handle"/> handle to the list of inherited handles.
        /// </summary>
        /// <param name="handle">The handle to add.</param>
        public void Add ( SafeHandle handle )
        {
            if (handle == null)
                throw new ArgumentNullException(nameof(handle));

            Add(handle.DangerousGetHandle());
        }

        /// <summary>
        /// Gets an enumerator for the collection of handles.
        /// </summary>
        /// <returns>An enumerator.</returns>
        public IEnumerator<IntPtr> GetEnumerator ()
        {
            return _handles.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
        {
            return GetEnumerator();
        }

        internal override int GetAttributeSize ( int attributeIndex )
        {
            // the required size is the number of handles times the size of a handle..
            return _handles.Count * IntPtr.Size;
        }

        internal override void SetAttributeData ( int attributeIndex, IntPtr dataPtr, IntPtr listPtr )
        {
            for (int iIndex = 0; iIndex < _handles.Count; iIndex++)
                Marshal.WriteIntPtr(dataPtr, iIndex * IntPtr.Size, _handles[iIndex]);

            SetAttributeData(listPtr, Interop.Kernel32.ProcThreadAttributes.HandleList, dataPtr, _handles.Count * IntPtr.Size);
        }

        /// <summary>
        /// Gets the list of handles to be inherited by a child process.
        /// </summary>
        public IReadOnlyList<IntPtr> Handles
        {
            get
            {
                return _handles;
            }
        }
    }
}
