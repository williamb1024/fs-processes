using System;
using System.Collections.Generic;
using System.Text;

namespace Fs.Processes.JobObjects
{
    /// <summary>
    /// EventArgs for events occurring due to memory limits.
    /// </summary>
    public class MemoryLimitEventArgs: EventArgs
    {
        internal MemoryLimitEventArgs ( int? processId, ulong? bytes )
        {
            ProcessId = processId;
            Bytes = bytes;
        }

        internal MemoryLimitEventArgs ( ulong bytes )
        {
            Bytes = bytes;
        }

        /// <summary>
        /// Gets the process Id that caused the event, or <c>null</c> if the event is not associated
        /// with a specific process.
        /// </summary>
        public int? ProcessId { get; }

        /// <summary>
        /// Gets the number of bytes committed memory for all processes in the job at the time the notification
        /// was sent, or <c>null</c> if the information is not available.
        /// </summary>
        public ulong? Bytes { get; }
    }
}
