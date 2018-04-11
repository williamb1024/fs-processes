using System;
using System.Collections.Generic;
using System.Text;

namespace Fs.Processes.JobObjects
{
    /// <summary>
    /// EventArgs for events exposing a process's unique identifier.
    /// </summary>
    public class ProcessIdEventArgs
    {
        internal ProcessIdEventArgs ( int processId )
        {
            ID = processId;
        }

        /// <summary>
        /// Gets the unique identifier of the process associated with the event.
        /// </summary>
        public int ID { get; }
    }
}
