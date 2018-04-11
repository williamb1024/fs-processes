using System;
using System.Collections.Generic;
using System.Text;

namespace Fs.Processes.JobObjects
{
    /// <summary>
    /// Accounting information for a <see cref="JobObject"/>.
    /// </summary>
    public class JobAccounting
    {
        internal JobAccounting ( in Interop.Kernel32.JOBOBJECT_BASIC_AND_IO_ACCOUNTING_INFORMATION accountingInfo,
                                 in Interop.Kernel32.JOBOBJECT_EXTENDED_LIMIT_INFORMATION limitInfo )
        {
            TotalUserTime = TimeSpan.FromTicks(accountingInfo.BasicInformation.TotalUserTime.QuadPart);
            TotalKernelTime = TimeSpan.FromTicks(accountingInfo.BasicInformation.TotalKernelTime.QuadPart);
            ThisPeriodTotalUserTime = TimeSpan.FromTicks(accountingInfo.BasicInformation.ThisPeriodTotalUserTime.QuadPart);
            ThisPeriodTotalKernelTime = TimeSpan.FromTicks(accountingInfo.BasicInformation.ThisPeriodTotalKernelTime.QuadPart);
            TotalPageFaults = accountingInfo.BasicInformation.TotalPageFaultCount;
            TotalProcesses = accountingInfo.BasicInformation.TotalProcesses;
            ActiveProcesses = accountingInfo.BasicInformation.ActiveProcesses;
            TerminatedProcesses = accountingInfo.BasicInformation.TotalTerminatedProcesses;

            ReadOperations = accountingInfo.IoInfo.ReadOperationCount;
            WriteOperations = accountingInfo.IoInfo.WriteOperationCount;
            OtherOperations = accountingInfo.IoInfo.OtherOperationCount;
            ReadBytes = accountingInfo.IoInfo.ReadTransferCount;
            WriteBytes = accountingInfo.IoInfo.WriteTransferCount;
            OtherBytes = accountingInfo.IoInfo.OtherTransferCount;

            PeakMemoryUsed = (ulong)limitInfo.PeakJobMemoryUsed;
            PeakProcessMemoryUsed = (ulong)limitInfo.PeakProcessMemoryUsed;
        }

        /// <summary>
        /// The total amount of user-mode time for all processes ever associated with the job.
        /// </summary>
        public TimeSpan TotalUserTime { get; }

        /// <summary>
        /// The total amount of kernel-mode time for all processes ever associated with the job.
        /// </summary>
        public TimeSpan TotalKernelTime { get; }

        /// <summary>
        /// The total amount of user-mode time for all processes associated with the job, since the last call that set a 
        /// per-job user-mode time limit.
        /// </summary>
        public TimeSpan ThisPeriodTotalUserTime { get; }

        /// <summary>
        /// The total amount of kernel-mode time for all processes associated with the job, since the last
        /// call that set a per-job kernel-mode time limit.
        /// </summary>
        public TimeSpan ThisPeriodTotalKernelTime { get; }

        /// <summary>
        /// The total number of page faults for all processes ever associated with the job.
        /// </summary>
        public uint TotalPageFaults { get; }

        /// <summary>
        /// The total number of processes that have been associated with the job.
        /// </summary>
        public uint TotalProcesses { get; }

        /// <summary>
        /// The total number of processes currently associated with the job.
        /// </summary>
        public uint ActiveProcesses { get; }

        /// <summary>
        /// Thet total number of processes associated with the job that have been terminated due to
        /// limit violations.
        /// </summary>
        public uint TerminatedProcesses { get; }

        /// <summary>
        /// The number of read operations performed.
        /// </summary>
        public ulong ReadOperations { get; }

        /// <summary>
        /// The number of write operations performed.
        /// </summary>
        public ulong WriteOperations { get; }

        /// <summary>
        /// The number of I/O operations performed, other than read and write operations.
        /// </summary>
        public ulong OtherOperations { get; }

        /// <summary>
        /// The number of bytes read.
        /// </summary>
        public ulong ReadBytes { get; }

        /// <summary>
        /// The number of bytes written.
        /// </summary>
        public ulong WriteBytes { get; }

        /// <summary>
        /// The number of bytes transferred during operations other than read and write operations.
        /// </summary>
        public ulong OtherBytes { get; }

        /// <summary>
        /// The peak memory usage of all processes currently associated with the job.
        /// </summary>
        public ulong PeakMemoryUsed { get; }

        /// <summary>
        /// The peak memory used by any process ever associated with the job.
        /// </summary>
        public ulong PeakProcessMemoryUsed { get; }
    }
}
