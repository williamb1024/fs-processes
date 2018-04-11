using System;
using System.Collections.Generic;
using System.Text;

namespace Fs.Processes
{
    /// <summary>
    /// Represents the foreground and background colors used in a Windows Console.
    /// </summary>
    public struct ConsoleColors
    {
        internal int GetFillAttribute ()
        {
            // FOREGROUND is low nibble, BACKGROUND is high nibble
            return (int)Foreground | ((int)Background << 4);
        }

        /// <summary>
        /// Gets or sets the foreground color.
        /// </summary>
        public ConsoleColor Foreground { get; set; }

        /// <summary>
        /// Gets or sets the background color.
        /// </summary>
        public ConsoleColor Background { get; set; }
    }
}
