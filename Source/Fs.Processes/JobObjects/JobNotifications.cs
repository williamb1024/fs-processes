using System;
using System.Collections.Generic;
using System.Text;

namespace Fs.Processes.JobObjects
{
    /// <summary>
    /// Configures notifications associated with a <see cref="JobObject"/>.
    /// </summary>
    public struct JobNotifications
    {
        internal JobNotifications ( in Interop.Kernel32.JOBOBJECT_NOTIFICATION_LIMIT_INFORMATION notifyLimits )
        {
            IoBytesRead = null;
            if ((notifyLimits.LimitFlags & Interop.Kernel32.JobObjectNotificationLimits.JobReadBytes) != 0)
                IoBytesRead = notifyLimits.IoReadBytesLimit;

            IoBytesWritten = null;
            if ((notifyLimits.LimitFlags & Interop.Kernel32.JobObjectNotificationLimits.JobWriteBytes) != 0)
                IoBytesWritten = notifyLimits.IoWriteBytesLimit;

            TimeLimit = null;
            if ((notifyLimits.LimitFlags & Interop.Kernel32.JobObjectBasicLimits.JobTime) != 0)
                TimeLimit = TimeSpan.FromTicks(notifyLimits.PerJobUserTimeLimit.QuadPart);

            MaximumMemory = null;
            if ((notifyLimits.LimitFlags & Interop.Kernel32.JobObjectExtendedLimits.JobMemory) != 0)
                MaximumMemory = notifyLimits.JobMemoryLimit;

            CpuRate = null;
            if ((notifyLimits.LimitFlags & Interop.Kernel32.JobObjectNotificationLimits.CpuRateControl) != 0)
                CpuRate = new RateControl((RateControlInterval)notifyLimits.RateControlToleranceInterval,
                                          (RateControlTolerance)notifyLimits.RateControlToleranceInterval);
        }

        internal void GetLimits ( out Interop.Kernel32.JOBOBJECT_NOTIFICATION_LIMIT_INFORMATION notifyLimits )
        {
            notifyLimits = default;

            if (IoBytesRead.HasValue)
            {
                notifyLimits.IoReadBytesLimit = IoBytesRead.Value;
                notifyLimits.LimitFlags |= Interop.Kernel32.JobObjectNotificationLimits.JobReadBytes;
            }

            if (IoBytesWritten.HasValue)
            {
                notifyLimits.IoWriteBytesLimit = IoBytesWritten.Value;
                notifyLimits.LimitFlags |= Interop.Kernel32.JobObjectNotificationLimits.JobWriteBytes;
            }

            if (TimeLimit.HasValue)
            {
                notifyLimits.PerJobUserTimeLimit.QuadPart = TimeLimit.Value.Ticks;
                notifyLimits.LimitFlags |= Interop.Kernel32.JobObjectBasicLimits.JobTime;
            }

            if (MaximumMemory.HasValue)
            {
                notifyLimits.JobMemoryLimit = MaximumMemory.Value;
                notifyLimits.LimitFlags |= Interop.Kernel32.JobObjectExtendedLimits.JobMemory;
            }

            if (CpuRate.HasValue)
            {
                notifyLimits.RateControlToleranceInterval = (int)CpuRate.Value.Interval;
                notifyLimits.RateControlTolerance = (int)CpuRate.Value.Tolerance;
                notifyLimits.LimitFlags |= Interop.Kernel32.JobObjectNotificationLimits.CpuRateControl;
            }
        }

        /// <summary>
        /// Raises a <see cref="JobObject.IoLimitExceeded"/> event when the total number of I/O bytes read for
        /// all processes associated with the job exceeds this value.
        /// </summary>
        public ulong? IoBytesRead { get; set; }

        /// <summary>
        /// Raises a <see cref="JobObject.IoLimitExceeded"/> event when the total number of I/O bytes written for
        /// all processes associated with the job exceeds this value.
        /// </summary>
        public ulong? IoBytesWritten { get; set; }

        /// <summary>
        /// Raises a <see cref="JobObject.TimeLimitExceeded"/> event when the total user-mode time for all processes
        /// in the job exceeds this value.
        /// </summary>
        public TimeSpan? TimeLimit { get; set; }

        /// <summary>
        /// Raises a <see cref="JobObject.MemoryLimitExceeded"/> event when the total committed virtual memory for
        /// all processes associated with the job exceeds this value.
        /// </summary>
        public ulong? MaximumMemory { get; set; }


        /// <summary>
        /// Raises a <see cref="JobObject.CpuRateLimitExceeded"/> event when the CPU time for all processes associated
        /// with the job exceeds this value.
        /// </summary>
        public RateControl? CpuRate { get; set; }
    }
}
