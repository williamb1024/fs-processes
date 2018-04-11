using System;
using System.Collections.Generic;
using System.Text;

namespace Fs.Processes
{
    /// <summary>
    /// A value indicating how a new window should appear when a new process is created.
    /// </summary>
    public enum WindowShow
    {
        /// <summary>
        /// The window appears hidden.
        /// </summary>
        Hidden,

        /// <summary>
        /// The window appears on screen, in a default location.
        /// </summary>
        Normal,

        /// <summary>
        /// The window appears minimized.
        /// </summary>
        Minimized,

        /// <summary>
        /// The window appears maximized.
        /// </summary>
        Maximized,
    }
}
