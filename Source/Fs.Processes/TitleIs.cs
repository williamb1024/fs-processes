using System;
using System.Collections.Generic;
using System.Text;

namespace Fs.Processes
{
    /// <summary>
    /// Specifies how <see cref="CreateProcessInfo.Title"/> is interpreted.
    /// </summary>
    public enum TitleIs
    {
        /// <summary>
        /// The title is treated the console window title.
        /// </summary>
        Undefined,

        /// <summary>
        /// The title is an AppuserModelID.
        /// </summary>
        AppID,

        /// <summary>
        /// The title contains the path of the shortcut file (.lnk) that the user used to
        /// invoke the process.
        /// </summary>
        LinkName
    }
}
