using System;
using System.Collections.Generic;
using System.Text;

namespace Fs.Processes.JobObjects
{
    /// <summary>
    /// EventArgs for events occuring when a process exits.
    /// </summary>
    public class ProcessExitedEventArgs: ProcessIdEventArgs
    {
        internal ProcessExitedEventArgs ( int processId, bool isAbnormal )
            : base(processId)
        {
            AbnormalExit = isAbnormal;
        }

        /// <summary>
        /// Gets a value indicating whether the process exited normally or abnormally.
        /// </summary>
        public bool AbnormalExit { get; }
    }
}
