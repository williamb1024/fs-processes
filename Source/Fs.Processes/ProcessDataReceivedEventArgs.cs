using System;
using System.Collections.Generic;
using System.Text;

namespace Fs.Processes
{
    /// <summary>
    /// Event data for redirected output from a process.
    /// </summary>
    public class ProcessDataReceivedEventArgs: EventArgs
    {
        internal ProcessDataReceivedEventArgs ( string data )
        {
            Data = data;
        }

        /// <summary>
        /// The data read from the process's output.
        /// </summary>
        public string Data { get; }
    }
}
