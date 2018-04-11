using System;
using System.Collections.Generic;
using System.Text;

namespace Fs.Processes.JobObjects
{
    /// <summary>
    /// The tolerance level for a <see cref="RateControl"/> structure.
    /// </summary>
    public enum RateControlTolerance
    {
        /// <summary>
        /// Rate can be exceeded for 20% of the <see cref="RateControlInterval"/>.
        /// </summary>
        Low = 1,

        /// <summary>
        /// Rate can be exceeded for 40% of the <see cref="RateControlInterval"/>.
        /// </summary>
        Medium = 2,

        /// <summary>
        /// Rate can be exceeded for 60% of the <see cref="RateControlInterval"/>.
        /// </summary>
        High = 3
    }
}
