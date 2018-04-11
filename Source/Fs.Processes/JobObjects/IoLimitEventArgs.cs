using System;
using System.Collections.Generic;
using System.Text;

namespace Fs.Processes.JobObjects
{
    /// <summary>
    /// EventArgs instance associated when Io limit events.
    /// </summary>
    public class IoLimitEventArgs: EventArgs
    {
        internal IoLimitEventArgs ( IoLimitType limitType, ulong bytes )
        {
            Type = limitType;
            Bytes = bytes;
        }

        /// <summary>
        /// Gets a value indicating the type of limit that has occurred.
        /// </summary>
        public IoLimitType Type { get; }
        
        /// <summary>
        /// Gets the total number of I/O bytes for all processes in the job at the time
        /// the notification was sent.
        /// </summary>
        public ulong Bytes { get; }
    }
}
