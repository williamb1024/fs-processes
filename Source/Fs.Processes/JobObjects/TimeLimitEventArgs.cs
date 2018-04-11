using System;
using System.Collections.Generic;
using System.Text;

namespace Fs.Processes.JobObjects
{
    /// <summary>
    /// EventArgs for events that occur when a time limit is exceeded.
    /// </summary>
    public class TimeLimitEventArgs : EventArgs
    {
        internal TimeLimitEventArgs ( int? processId )
        {
            ProcessId = processId;
            Time = null;
        }

        internal TimeLimitEventArgs ( TimeSpan timeSpan )
        {
            Time = timeSpan;
        }

        /// <summary>
        /// The ID of the process associated with the event, or <c>null</c> if the event is not
        /// associated with a specific process.
        /// </summary>
        public int? ProcessId { get; }

        /// <summary>
        /// The total amount of time for a processes in the job at the time the notification was sent, or <c>null</c>
        /// if time information is not available.
        /// </summary>
        public TimeSpan? Time { get; }
    }
}
