using System;
using System.Collections.Generic;
using System.Text;

namespace Fs.Processes.JobObjects
{
    /// <summary>
    /// A set of values indicating the type of Io limit.
    /// </summary>
    public enum IoLimitType
    {
        /// <summary>
        /// I/O read limit.
        /// </summary>
        Read,

        /// <summary>
        /// I/O write limit.
        /// </summary>
        Write
    }
}
