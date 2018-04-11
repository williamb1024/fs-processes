using System;
using System.Collections.Generic;
using System.Text;

namespace Fs.Processes.JobObjects
{
    /// <summary>
    /// Options for rate control notifications.
    /// </summary>
    public struct RateControl
    {
        private readonly RateControlInterval _interval;
        private readonly RateControlTolerance _tolerance;

        /// <summary>
        /// Constructs a <see cref="RateControl"/> instance with the specified settings.
        /// </summary>
        /// <param name="interval">The <see cref="RateControlInterval"/> assigned to the <see cref="RateControl.Interval"/> property.</param>
        /// <param name="tolerance">The <see cref="RateControlTolerance"/> assigned to the <see cref="RateControl.Tolerance"/> property.</param>
        public RateControl ( RateControlInterval interval, RateControlTolerance tolerance )
        {
            _interval = interval;
            _tolerance = tolerance;
        }

        /// <summary>
        /// The measurement interval for rate control.
        /// </summary>
        public RateControlInterval Interval
        {
            get
            {
                return (_interval != 0) ? _interval : RateControlInterval.Short;
            }
        }

        /// <summary>
        /// The tolerance level for rate control.
        /// </summary>
        public RateControlTolerance Tolerance
        {
            get
            {
                return (_tolerance != 0) ? _tolerance : RateControlTolerance.High;
            }
        }
    }
}
