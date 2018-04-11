using System;
using System.Collections.Generic;
using System.Text;

namespace Fs.Processes.JobObjects
{
    /// <summary>
    /// A set of values that control how a <see cref="JobObject"/> operates.
    /// </summary>
    [Flags]
    public enum JobOptions
    {
        /// <summary>
        /// A place holder used when no options are desired.
        /// </summary>
        None = 0,

        /// <summary>
        /// Allow processes to breakaway from the job, if requested.
        /// </summary>
        AllowBreakAway = 1,

        /// <summary>
        /// New processes are not automatically associated with the job.
        /// </summary>
        AllowSilentBreakAway = 2,

        /// <summary>
        /// Prevent Windows Error Reporting dialogs if a process terminates due to an unhandled exception.
        /// </summary>
        PreventWindowsErrorReporting = 4,

        /// <summary>
        /// Terminate all active processes associated with the <see cref="JobObject"/> when the last handle 
        /// to the <see cref="JobObject"/> is closed.
        /// </summary>
        TerminateProcessesWhenJobClosed = 8,

        /// <summary>
        /// Terminate all active processes when the <see cref="JobLimits.TimeLimit"/> is exceeded. If this 
        /// option is not set, a notification is generated and the <see cref="JobLimits.TimeLimit"/> is cleared.
        /// </summary>
        TerminateAtTimeLimit = 16,
    }
}
