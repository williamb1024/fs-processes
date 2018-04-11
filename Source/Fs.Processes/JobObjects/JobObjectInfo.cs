using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Fs.Processes.JobObjects
{
    /// <summary>
    /// A based class for <see cref="JobObject"/> that provides access to information about
    /// the JobObject.
    /// </summary>
    public abstract class JobObjectInfo
    {
        internal static readonly bool _jobsSupported = false;
        internal static readonly bool _jobsGroupSupported = false;
        internal static readonly bool _jobsGroupExSupported = false;
        internal static readonly bool _jobsLimitViolationSupported = false;
        internal static readonly bool _jobsLimitViolation2Supported = false;

        static JobObjectInfo ()
        {
            // create a JobObject and determine what options are supported on this OS..
            SafeJobObjectHandle jobHandle = null;
            try
            {
                jobHandle = Interop.Kernel32.CreateJobObject(IntPtr.Zero, null);
                if (jobHandle.IsInvalid)
                    return;

                _jobsSupported = true;

                // notification limit 
                // notification limit 2
                // net rate control
                // violation limit
                // violation limit 2
                // job object group info
                // cpu rate control
                // group information ex

                IntPtr memoryPtr = IntPtr.Zero;
                try
                {
                    int memorySize = 1024;
                    memoryPtr = Marshal.AllocHGlobal(memorySize);

                    // ask for specific structures to determine the level of support..
                    _jobsGroupSupported = IsSupported(jobHandle, Interop.Kernel32.JobObjectInformationClass.GroupInformation, memoryPtr, memorySize);
                    _jobsGroupExSupported = IsSupported(jobHandle, Interop.Kernel32.JobObjectInformationClass.GroupInformationEx, memoryPtr, memorySize);
                    _jobsLimitViolationSupported = IsSupported(jobHandle, Interop.Kernel32.JobObjectInformationClass.LimitViolationInformation, memoryPtr, memorySize);
                    _jobsLimitViolation2Supported = IsSupported(jobHandle, Interop.Kernel32.JobObjectInformationClass.LimitViolationInformation2, memoryPtr, memorySize);
                }
                finally
                {
                    if (memoryPtr != IntPtr.Zero) Marshal.FreeHGlobal(memoryPtr);
                }
            }
            finally
            {
                jobHandle?.Dispose();
            }
        }

        internal JobObjectInfo ()
        {
            // exists to prevent descendants in other assemblies..
        }

        private protected void CheckDisposed ()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        /// <summary>
        /// Gets accounting information for the JobObject.
        /// </summary>
        /// <returns>A <see cref="JobAccounting"/> structure.</returns>
        public JobAccounting GetAccounting ()
        {
            CheckDisposed();

            var handle = GetHandle();
            bool handleLocked = false;
            try
            {
                handle.DangerousAddRef(ref handleLocked);
                if (!handleLocked)
                    throw new ObjectDisposedException(GetType().Name);

                if (!Interop.Kernel32.QueryInformationJobObject(handle,
                                                                Interop.Kernel32.JobObjectInformationClass.BasicAndIoAccountingInformation,
                                                                out Interop.Kernel32.JOBOBJECT_BASIC_AND_IO_ACCOUNTING_INFORMATION accountingInfo,
                                                                Interop.Kernel32.JOBOBJECT_BASIC_AND_IO_ACCOUNTING_INFORMATION.SizeOf,
                                                                IntPtr.Zero))
                    throw Errors.Win32Error();

                if (!Interop.Kernel32.QueryInformationJobObject(handle,
                                                                Interop.Kernel32.JobObjectInformationClass.ExtendedLimitInformation,
                                                                out Interop.Kernel32.JOBOBJECT_EXTENDED_LIMIT_INFORMATION limitInfo,
                                                                Interop.Kernel32.JOBOBJECT_EXTENDED_LIMIT_INFORMATION.SizeOf,
                                                                IntPtr.Zero))
                    throw Errors.Win32Error();

                return new JobAccounting(accountingInfo, limitInfo);
            }
            finally
            {
                if (handleLocked) handle.DangerousRelease();
            }
        }

        /// <summary>
        /// Gets limits assigned to the JobObject.
        /// </summary>
        /// <returns>A <see cref="JobLimits"/> structure.</returns>
        public JobLimits GetLimits ()
        {
            CheckDisposed();

            Interop.Kernel32.JOBOBJECT_EXTENDED_LIMIT_INFORMATION limitInformation;
            Interop.Kernel32.JOBOBJECT_END_OF_JOB_TIME_INFORMATION endInformation;
            Interop.Kernel32.JOBOBJECT_BASIC_UI_RESTRICTIONS uiInformation;
            Interop.Kernel32.JOBOBJECT_CPU_RATE_CONTROL_INFORMATION cpuInformation;

            var handle = GetHandle();
            bool handleLocked = false;
            try
            {
                handle.DangerousAddRef(ref handleLocked);
                if (!handleLocked)
                    throw new ObjectDisposedException(GetType().Name);

                if (!Interop.Kernel32.QueryInformationJobObject(handle,
                                                                Interop.Kernel32.JobObjectInformationClass.ExtendedLimitInformation,
                                                                out limitInformation,
                                                                Interop.Kernel32.JOBOBJECT_EXTENDED_LIMIT_INFORMATION.SizeOf,
                                                                IntPtr.Zero))
                    throw Errors.Win32Error();

                if (!Interop.Kernel32.QueryInformationJobObject(handle,
                                                                Interop.Kernel32.JobObjectInformationClass.EndOfJobTimeInformation,
                                                                out endInformation,
                                                                Interop.Kernel32.JOBOBJECT_END_OF_JOB_TIME_INFORMATION.SizeOf,
                                                                IntPtr.Zero))
                    throw Errors.Win32Error();

                if (!Interop.Kernel32.QueryInformationJobObject(handle,
                                                                Interop.Kernel32.JobObjectInformationClass.BasicUIRestrictions,
                                                                out uiInformation,
                                                                Interop.Kernel32.JOBOBJECT_BASIC_UI_RESTRICTIONS.SizeOf,
                                                                IntPtr.Zero))
                    throw Errors.Win32Error();

                if (_jobsLimitViolationSupported)
                {
                    if (!Interop.Kernel32.QueryInformationJobObject(handle,
                                                                    Interop.Kernel32.JobObjectInformationClass.CpuRateControlInformation,
                                                                    out cpuInformation,
                                                                    Interop.Kernel32.JOBOBJECT_CPU_RATE_CONTROL_INFORMATION.SizeOf,
                                                                    IntPtr.Zero))
                        throw Errors.Win32Error();
                }
                else
                    // CPU rate control not supported..
                    cpuInformation = default;
            }
            finally
            {
                if (handleLocked) handle.DangerousRelease();
            }

            return new JobLimits(limitInformation, endInformation, uiInformation, cpuInformation);
        }

        /// <summary>
        /// Gets notifications configured for the JobObject.
        /// </summary>
        /// <returns>A <see cref="JobNotifications"/> structure.</returns>
        public JobNotifications GetNotifications ()
        {
            CheckDisposed();

            if (_jobsLimitViolationSupported)
            {
                if (!Interop.Kernel32.QueryInformationJobObject(GetHandle(),
                                                                Interop.Kernel32.JobObjectInformationClass.NotificationLimitInformation,
                                                                out Interop.Kernel32.JOBOBJECT_NOTIFICATION_LIMIT_INFORMATION notifyInfo,
                                                                Interop.Kernel32.JOBOBJECT_NOTIFICATION_LIMIT_INFORMATION.SizeOf,
                                                                IntPtr.Zero))
                    throw Errors.Win32Error();

                return new JobNotifications(notifyInfo);
            }
            else
                // notifications not supported..
                return default;
        }

        /// <summary>
        /// Gets an array of process identifiers associated with the JobObject.
        /// </summary>
        /// <returns>An array of process identifiers.</returns>
        public int[] GetProcessIds ()
        {
            CheckDisposed();

            IntPtr bufferPtr = IntPtr.Zero;
            try
            {
                int bufferJobCount = 16;
                int bufferSize = Interop.Kernel32.JOBOBJECT_BASIC_PROCESS_ID_LIST.SizeOf + (UIntPtr.Size * bufferJobCount);
                bufferPtr = Marshal.AllocHGlobal(bufferSize);

                while (true)
                {
                    if (!Interop.Kernel32.QueryInformationJobObject(GetHandle(),
                                                                    Interop.Kernel32.JobObjectInformationClass.BasicProcessIdList,
                                                                    bufferPtr,
                                                                    bufferSize,
                                                                    IntPtr.Zero))
                    {
                        if (Marshal.GetLastWin32Error() != Interop.Errors.ERROR_MORE_DATA)
                            throw Errors.Win32Error();

                        bufferSize = Interop.Kernel32.JOBOBJECT_BASIC_PROCESS_ID_LIST.SizeOf + (UIntPtr.Size * (bufferJobCount += 16));
                        bufferPtr = Marshal.ReAllocHGlobal(bufferPtr, (IntPtr)bufferSize);
                        continue;
                    }

                    break;
                }

                int processesCount = Marshal.ReadInt32(bufferPtr);
                int processesListed = Marshal.ReadInt32(bufferPtr, sizeof(Int32));

                if (processesListed == 0)
                    return Array.Empty<int>();

                IntPtr processList = bufferPtr + (int)Interop.Kernel32.JOBOBJECT_BASIC_PROCESS_ID_LIST.ProcessIdsOffset;
                int[] processIds = new int[processesListed];

                for (int iIndex = 0; iIndex < processesListed; iIndex++)
                    processIds[iIndex] = (int)Marshal.ReadIntPtr(processList, iIndex * IntPtr.Size);

                return processIds;
            }
            finally
            {
                if (bufferPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(bufferPtr);
            }
        }

        private static bool IsSupported ( SafeJobObjectHandle jobHandle, Interop.Kernel32.JobObjectInformationClass informationClass, IntPtr bufferPtr, int bufferSize )
        {
            bool queryResult = Interop.Kernel32.QueryInformationJobObject(jobHandle,
                                                                          informationClass,
                                                                          bufferPtr,
                                                                          bufferSize,
                                                                          out int bufferUsed);

            if (queryResult)
                return true;

            // if the result indicates we need a bigger buffer, then the information class is supported..
            int errorCode = Marshal.GetLastWin32Error();
            if ((errorCode == Interop.Errors.ERROR_MORE_DATA) ||
                (errorCode == Interop.Errors.ERROR_BAD_LENGTH))
                return true;

            // assert that we got ERROR_INVALID_PARAMETER, which means it really isn't supported..
            System.Diagnostics.Debug.Assert(errorCode == Interop.Errors.ERROR_INVALID_PARAMETER);

            // any error code that isn't ERROR_MORE_DATA is considered not supported..
            return false;
        }

        private protected abstract SafeJobObjectHandle GetHandle ();

        private protected abstract bool IsDisposed { get; }
    }
}
