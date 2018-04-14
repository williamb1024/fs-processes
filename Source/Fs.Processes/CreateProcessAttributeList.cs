using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Fs.Processes
{
    /// <summary>
    /// A list of attributes for process and thread creation.
    /// </summary>
    public class CreateProcessAttributeList : ICollection<CreateProcessAttributeList.CreateProcessAttribute>
    {
        private readonly Dictionary<Type, CreateProcessAttribute> _attributes = new Dictionary<Type, CreateProcessAttribute>();

        /// <summary>
        /// Creates a new instance of <see cref="CreateProcessAttributeList"/>.
        /// </summary>
        public CreateProcessAttributeList ()
        {
        }

        /// <summary>
        /// Adds the <paramref name="item"/> attribute to the attribute list. Any previous item of the same type is replaced.
        /// </summary>
        /// <param name="item">The attribute to add.</param>
        public void Add ( CreateProcessAttribute item )
        {
            // this type works more like a dictionary, where the key is the item's type than a collection. When an
            // item is added, any existing item of the same type is removed. We do not attempt to combine items, for 
            // example a InheritHandlesAttribute list will replace, not append, to an existing list.

            if (item == null)
                throw new ArgumentNullException(nameof(item));

            lock (_attributes)
                _attributes[item.GetType()] = item;
        }

        /// <summary>
        /// Determines if the <paramref name="item"/> attribute is part of the list.
        /// </summary>
        /// <param name="item">The attribute to locate.</param>
        /// <returns><c>true</c> if the attribute is contained by the list; otherwise, <c>false</c>.</returns>
        public bool Contains ( CreateProcessAttribute item )
        {
            if (item == null)
                return false;

            lock (_attributes)
                if ((_attributes.TryGetValue(item.GetType(), out var existingItem)) &&
                    (Object.ReferenceEquals(existingItem, item)))
                    return true;

            return false;
        }

        /// <summary>
        /// Removes the <paramref name="item"/> attribute from the list.
        /// </summary>
        /// <param name="item">The attribute to remove.</param>
        /// <returns><c>true</c> if the attribute was contained by the list and removed; otherwise, <c>false</c>.</returns>
        public bool Remove ( CreateProcessAttribute item )
        {
            lock (_attributes)
                if ((_attributes.TryGetValue(item.GetType(), out var existingItem)) &&
                    (Object.ReferenceEquals(existingItem, item)))
                {
                    _attributes.Remove(item.GetType());
                    return true;
                }

            return false;
        }

        /// <summary>
        /// Removes all attributes from the list.
        /// </summary>
        public void Clear ()
        {
            lock (_attributes)
                _attributes.Clear();
        }

        void ICollection<CreateProcessAttribute>.CopyTo ( CreateProcessAttribute[] array, int arrayIndex )
        {
            lock (_attributes)
                _attributes.Values.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets an enumerator for the list of attributes.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public IEnumerator<CreateProcessAttribute> GetEnumerator ()
        {
            return _attributes.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator ()
        {
            return GetEnumerator();
        }

        internal static void GetAttributesList ( ref IntPtr attributeList, ref bool attributesInitialized, IReadOnlyList<CreateProcessAttribute> attributes )
        {
            if (attributeList != IntPtr.Zero)
                throw new ArgumentException("attributeList must be initialized to IntPtr.Zero", nameof(attributeList));

            if (attributesInitialized)
                throw new ArgumentException("attributesInitialized must be initialized to false.", nameof(attributesInitialized));

            if (attributes == null)
                throw new ArgumentNullException(nameof(attributes));

            if (attributes.Count == 0)
                return;

            // NOTE: The caller is responsible for wrapping this method in a try/finally that releases the
            // HGLOBAL memory if attributeList is not IntPtr.Zero..

            int totalAttributeCount = 0;
            int totalAttributeStorage = 0;

            foreach (var attribute in attributes)
            {
                // get the number of attributes produced by this attribute..
                int attributeCount = attribute.GetAttributeCount();

                // get the size of each of those attributes, rounding to a multiple of 16
                // for each block..

                for (int attributeIndex = 0; attributeIndex < attributeCount; attributeIndex++)
                    totalAttributeStorage += (attribute.GetAttributeSize(attributeIndex) + 15) & (~15);

                totalAttributeCount += attributeCount;
            }

            IntPtr requiredSize = IntPtr.Zero;

            // use InitializeProcThreadAttributeList to determine the number of bytes we need to allocate
            // to hold the number of attributes that we have in the list..

            if (!Interop.Kernel32.InitializeProcThreadAttributeList(IntPtr.Zero, (uint)totalAttributeCount, 0, ref requiredSize))
            {
                // this is expected to fail, make sure it fails correctly...
                int errorCode = Marshal.GetLastWin32Error();
                if (errorCode != Interop.Errors.ERROR_INSUFFICIENT_BUFFER)
                    throw Errors.Win32Error(errorCode);
            }

            // allocate storage for the list and the attribute datas. The list comes first in memory, because
            // that is what is expected for our output .. the attribute data is stored following the list.

            int listRequiredSize = ((int)requiredSize + 15) & (~15);
            attributeList = Marshal.AllocHGlobal(listRequiredSize + totalAttributeStorage);
            IntPtr attributesData = attributeList + listRequiredSize;

            // actually initialize the list..
            if (!Interop.Kernel32.InitializeProcThreadAttributeList(attributeList, (uint)totalAttributeCount, 0, ref requiredSize))
                throw Errors.Win32Error();

            attributesInitialized = true;

            foreach (var attribute in attributes)
            {
                int attributeCount = attribute.GetAttributeCount();

                for (int attributeIndex = 0; attributeIndex < attributeCount; attributeIndex++)
                {
                    // get the size of the attribute's data and have it write its data to our 
                    // storage ..

                    int attributeSize = attribute.GetAttributeSize(attributeIndex);
                    attribute.SetAttributeData(attributeIndex, attributesData, attributeList);

                    // adjust the attribute data pointer..
                    attributesData += ((attributeSize + 15) & (~15));
                }
            }
        }

        /// <summary>
        /// Gets the number of attributes in the list.
        /// </summary>
        public int Count => _attributes.Count;
        bool ICollection<CreateProcessAttribute>.IsReadOnly => false;

        /// <summary>
        /// Abstract base class for all attributes added to a <see cref="CreateProcessAttributeList"/>.
        /// </summary>
        public abstract class CreateProcessAttribute
        {
            private protected CreateProcessAttribute ()
            {
                // exists purely to prevent other assemblies from creating descendants
            }

            internal virtual int GetAttributeCount () => 1;

            internal abstract int GetAttributeSize ( int attributeIndex );

            internal abstract void SetAttributeData ( int attributeIndex, IntPtr dataPtr, IntPtr listPtr );

            internal void SetAttributeData ( IntPtr listPtr, int attributeId, IntPtr dataPtr, int cbData )
            {
                if (!Interop.Kernel32.UpdateProcThreadAttribute(listPtr,
                                                                0,
                                                                (IntPtr)attributeId,
                                                                dataPtr,
                                                                (IntPtr)cbData,
                                                                IntPtr.Zero,
                                                                IntPtr.Zero))
                    throw Errors.Win32Error();
            }
        }
    }
}


//#define PROC_THREAD_ATTRIBUTE_PARENT_PROCESS \
//#define PROC_THREAD_ATTRIBUTE_HANDLE_LIST \
//#define PROC_THREAD_ATTRIBUTE_GROUP_AFFINITY \
//#define PROC_THREAD_ATTRIBUTE_PREFERRED_NODE \
//#define PROC_THREAD_ATTRIBUTE_IDEAL_PROCESSOR \
//#define PROC_THREAD_ATTRIBUTE_UMS_THREAD \
//#define PROC_THREAD_ATTRIBUTE_MITIGATION_POLICY \
//#define PROC_THREAD_ATTRIBUTE_SECURITY_CAPABILITIES \
//#define PROC_THREAD_ATTRIBUTE_PROTECTION_LEVEL \
