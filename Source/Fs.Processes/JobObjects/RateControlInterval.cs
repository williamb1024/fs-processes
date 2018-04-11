using System;
using System.Collections.Generic;
using System.Text;

namespace Fs.Processes.JobObjects
{
    /// <summary>
    /// The interval used in a <see cref="RateControl"/> structure.
    /// </summary>
    public enum RateControlInterval
    {
        /// <summary>
        /// A short interval of 10 seconds.
        /// </summary>
        Short = 1,

        /// <summary>
        /// A medium interval of 1 minute.
        /// </summary>
        Medium = 2,

        /// <summary>
        /// A long interval of 10 minutes.
        /// </summary>
        Long = 3
    }
}
