using System;
using System.Collections.Generic;
using System.Text;

namespace Fs.Processes
{
    /// <summary>
    /// A set of options that controls how a new process is created.
    /// </summary>
    [Flags]
    public enum ProcessOptions
    {
        /// <summary>
        /// Default option
        /// </summary>
        None = 0,

        /// <summary>
        /// The child processes of a process associated with a job are not associated with the job.
        /// </summary>
        BreakawayFromJob = 0x01000000,

        /// <summary>
        /// The new process does not inherit the error mode of the calling process. Instead, the new process
        /// gets the default error mode.
        /// </summary>
        DefaultErrorMode = 0x04000000,

        /// <summary>
        /// The new process has a new console, instead of inheriting its parent's console. This flag cannot
        /// be used with <see cref="Detached"/>.
        /// </summary>
        NewConsole = 0x00000010,

        /// <summary>
        /// Thew new process is the root process of a new process group. This flag is ignored if specified
        /// with <see cref="NewConsole"/>.
        /// </summary>
        NewProcessGroup = 0x00000200,

        /// <summary>
        /// The process is a console application that is being run without a console window. The console handle
        /// for the new process is not set. This flag is ignored if used with either <see cref="NewConsole"/> or
        /// <see cref="Detached"/>.
        /// </summary>
        NoWindow = 0x08000000,

        /// <summary>
        /// Execute a child process that bypasses the process restrictions that would normally be applied
        /// automatically to the process.
        /// </summary>
        PreserveCodeAuthzLevel = 0x02000000,

        /// <summary>
        /// The primary thread of the new process is created in a suspended state and does not run until
        /// the thread is resumed.
        /// </summary>
        Suspended = 0x00000004,

        /// <summary>
        /// The calling thread starts and debugs the new process. Child processes of the new process
        /// are not automatically attached.
        /// </summary>
        DebugOnlyThisProcess = 0x00000002,

        /// <summary>
        /// The calling thread starts and debugs the new process and all child processes created by the new process.
        /// If combined with <see cref="DebugOnlyThisProcess"/> the caller debugs only the new process, not any
        /// child processes.
        /// </summary>
        Debug = 0x00000001,

        /// <summary>
        /// For console processes, the new process does not inherits it parent's console.
        /// </summary>
        Detached = 0x00000008,

        /// <summary>
        /// The process inherits it parent's affinity.
        /// </summary>
        InheritParentAffinity = 0x00010000
    }

    /// <summary>
    /// A set of extensions for <see cref="ProcessOptions"/>.
    /// </summary>
    public static class ProcessOptionsExtensions
    {
        private static ProcessOptions NewConsoleDetatched =
            ProcessOptions.NewConsole | ProcessOptions.Detached;

        private static ProcessOptions AllOptions =
            ProcessOptions.BreakawayFromJob | ProcessOptions.Debug | ProcessOptions.DebugOnlyThisProcess | ProcessOptions.DefaultErrorMode |
            ProcessOptions.Detached | ProcessOptions.InheritParentAffinity | ProcessOptions.NewConsole | ProcessOptions.NewProcessGroup |
            ProcessOptions.NoWindow | ProcessOptions.PreserveCodeAuthzLevel | ProcessOptions.Suspended;

        /// <summary>
        /// Validates a set of <see cref="ProcessOptions"/>.
        /// </summary>
        /// <param name="processOptions">The options to validate.</param>
        /// <returns><c>true</c> if the options are valid; otherise, <c>false</c>.</returns>
        public static bool IsValid ( this ProcessOptions processOptions )
        {
            // no unassigned flags are set...
            if ((processOptions & ~AllOptions) != 0)
                return false;

            // cannot combine NewConsole and Detatched..
            if ((processOptions & NewConsoleDetatched) == NewConsoleDetatched)
                return false;

            return true;
        }
    }
}
