using System;
using System.Collections.Generic;
using System.Text;

namespace Fs.Processes.JobObjects
{
    /// <summary>
    /// A set of Ui restrictions applied to a <see cref="JobObject"/>.
    /// </summary>
    [Flags]
    public enum JobUiRestrictions
    {
        /// <summary>
        /// No restrictions
        /// </summary>
        None = 0,

        /// <summary>
        /// Prevent processes associated with the job from using User handles owned by
        /// processes not associated with the job. <see cref="JobObject.GrantUserHandleAccess(System.Runtime.InteropServices.SafeHandle, bool)"/> can
        /// be used to grant access to specific User handles.
        /// </summary>
        Handles = 1,

        /// <summary>
        /// Prevents processes associated with the job from reading data from the clipboard.
        /// </summary>
        ReadClipboard = 2,

        /// <summary>
        /// Prevents processes associated with the job from writing data to the clipboard.
        /// </summary>
        WriteClipboard = 4,

        /// <summary>
        /// Prevents processes associated with the job from changing system parameters by using
        /// the SystemParametersInfo function.
        /// </summary>
        SystemParameters = 8,

        /// <summary>
        /// Prevents processes associated with the job from calling the ChangeDisplaySettings
        /// functions.
        /// </summary>
        DisplaySettings = 16,

        /// <summary>
        /// Prevents processes associated with the job from accessing global atoms. When this flag is used,
        /// each job has its own atom table.
        /// </summary>
        GlobalAtoms = 32,

        /// <summary>
        /// Prevents processes associated with this job from creating desktops and switching desktops using the
        /// CreateDesktop or SwitchDesktop functions.
        /// </summary>
        Desktop = 64,

        /// <summary>
        /// Prevents processes associated with the job from calling the ExitWindows or ExitWindowsEx
        /// functions.
        /// </summary>
        ExitWindows = 128
    }
}
