using System;
using System.Collections.Generic;
using System.Text;

namespace Fs.Processes.JobObjects
{
    /// <summary>
    /// EventArgs for events occurring when a rate limit is exceeded.
    /// </summary>
    public class RateLimitEventArgs: EventArgs
    {
        internal RateLimitEventArgs ( RateControlTolerance tolerance )
        {
            Tolerance = tolerance;
        }

        /// <summary>
        /// The tolerance level that has been exceeded.
        /// </summary>
        public RateControlTolerance Tolerance { get; }
    }
}
