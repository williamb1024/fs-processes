using System;
using System.Collections.Generic;
using System.Text;

namespace Fs.Processes.JobObjects
{
    /// <summary>
    /// Settings that control the limits associated with a <see cref="JobObject"/>.
    /// </summary>
    public struct JobLimits
    {
        private TimeSpan? _timeLimit;
        private JobOptions _jobOptions;
        private bool _replaceTimeLimit;
        private bool _notifyTimeLimit;

        internal JobLimits ( in Interop.Kernel32.JOBOBJECT_EXTENDED_LIMIT_INFORMATION limitInfo,
                             in Interop.Kernel32.JOBOBJECT_END_OF_JOB_TIME_INFORMATION endInfo,
                             in Interop.Kernel32.JOBOBJECT_BASIC_UI_RESTRICTIONS uiInfo,
                             in Interop.Kernel32.JOBOBJECT_CPU_RATE_CONTROL_INFORMATION cpuInfo )
        {
            ActiveProcesses = null;
            if ((limitInfo.BasicLimitInformation.LimitFlags & Interop.Kernel32.JobObjectBasicLimits.ActiveProcess) != 0)
                ActiveProcesses = limitInfo.BasicLimitInformation.ActiveProcessLimit;

            Affinity = null;
            if ((limitInfo.BasicLimitInformation.LimitFlags & Interop.Kernel32.JobObjectBasicLimits.Affinity) != 0)
                Affinity = (ulong)limitInfo.BasicLimitInformation.Affinity;

            _replaceTimeLimit = false;
            _notifyTimeLimit = (endInfo.EndOfJobTimeAction == 1);

            _timeLimit = null;
            if ((limitInfo.BasicLimitInformation.LimitFlags & Interop.Kernel32.JobObjectBasicLimits.JobTime) != 0)
                _timeLimit = TimeSpan.FromTicks(limitInfo.BasicLimitInformation.PerJobUserTimeLimit.QuadPart);

            ProcessTimeLimit = null;
            if ((limitInfo.BasicLimitInformation.LimitFlags & Interop.Kernel32.JobObjectBasicLimits.ProcessTime) != 0)
                ProcessTimeLimit = TimeSpan.FromTicks(limitInfo.BasicLimitInformation.PerProcessUserTimeLimit.QuadPart);

            WorkingSet = null;
            if ((limitInfo.BasicLimitInformation.LimitFlags & Interop.Kernel32.JobObjectBasicLimits.WorkingSet) != 0)
                WorkingSet = ((ulong)limitInfo.BasicLimitInformation.MinimumWorkingSetSize, (ulong)limitInfo.BasicLimitInformation.MaximumWorkingSetSize);

            MaximumProcessMemory = null;
            if ((limitInfo.BasicLimitInformation.LimitFlags & Interop.Kernel32.JobObjectExtendedLimits.ProcessMemory) != 0)
                MaximumProcessMemory = (ulong)limitInfo.ProcessMemoryLimit;

            MaximumMemory = null;
            if ((limitInfo.BasicLimitInformation.LimitFlags & Interop.Kernel32.JobObjectExtendedLimits.JobMemory) != 0)
                MaximumMemory = (ulong)limitInfo.JobMemoryLimit;

            _jobOptions = CalculateJobOptions(limitInfo.BasicLimitInformation.LimitFlags);
            UiRestrictions = CalculateJobUiRestrictions(uiInfo.UIRestrictionsClass);
            CpuRate = CalculateCpuLimit(cpuInfo);
        }

        internal void GetLimits ( out Interop.Kernel32.JOBOBJECT_EXTENDED_LIMIT_INFORMATION basicLimits,
                                  out Interop.Kernel32.JOBOBJECT_END_OF_JOB_TIME_INFORMATION endInfo,
                                  out Interop.Kernel32.JOBOBJECT_BASIC_UI_RESTRICTIONS uiLimits,
                                  out Interop.Kernel32.JOBOBJECT_CPU_RATE_CONTROL_INFORMATION cpuLimits )
        {
            basicLimits = default;
            uiLimits = default;
            cpuLimits = default;
            endInfo = default;

            // basic and extended limits...
            if (!PreserveTimeLimit)
            {
                if (TimeLimit.HasValue)
                {
                    basicLimits.BasicLimitInformation.PerJobUserTimeLimit.QuadPart = TimeLimit.Value.Ticks;
                    basicLimits.BasicLimitInformation.LimitFlags |= Interop.Kernel32.JobObjectBasicLimits.JobTime;
                }
            }
            else
                basicLimits.BasicLimitInformation.LimitFlags |= Interop.Kernel32.JobObjectBasicLimits.PreserveJobTime;

            if (ProcessTimeLimit.HasValue)
            {
                basicLimits.BasicLimitInformation.PerProcessUserTimeLimit.QuadPart = ProcessTimeLimit.Value.Ticks;
                basicLimits.BasicLimitInformation.LimitFlags |= Interop.Kernel32.JobObjectBasicLimits.ProcessTime;
            }

            if (ActiveProcesses.HasValue)
            {
                basicLimits.BasicLimitInformation.ActiveProcessLimit = ActiveProcesses.Value;
                basicLimits.BasicLimitInformation.LimitFlags |= Interop.Kernel32.JobObjectBasicLimits.ActiveProcess;
            }

            if (Affinity.HasValue)
            {
                basicLimits.BasicLimitInformation.Affinity = (UIntPtr)Affinity.Value;
                basicLimits.BasicLimitInformation.LimitFlags |= Interop.Kernel32.JobObjectBasicLimits.Affinity;
            }

            if (WorkingSet.HasValue)
            {
                var (Minimum, Maximum) = WorkingSet.Value;
                basicLimits.BasicLimitInformation.MinimumWorkingSetSize = (UIntPtr)Minimum;
                basicLimits.BasicLimitInformation.MaximumWorkingSetSize = (UIntPtr)Maximum;
                basicLimits.BasicLimitInformation.LimitFlags |= Interop.Kernel32.JobObjectBasicLimits.WorkingSet;
            }

            if (MaximumProcessMemory.HasValue)
            {
                basicLimits.ProcessMemoryLimit = (UIntPtr)MaximumProcessMemory.Value;
                basicLimits.BasicLimitInformation.LimitFlags |= Interop.Kernel32.JobObjectExtendedLimits.ProcessMemory;
            }

            if (MaximumMemory.HasValue)
            {
                basicLimits.JobMemoryLimit = (UIntPtr)MaximumMemory.Value;
                basicLimits.BasicLimitInformation.LimitFlags |= Interop.Kernel32.JobObjectExtendedLimits.JobMemory;
            }

            JobOptions jobOptions = Options;
            if ((jobOptions & JobOptions.AllowBreakAway) != 0)
                basicLimits.BasicLimitInformation.LimitFlags |= Interop.Kernel32.JobObjectExtendedLimits.BreakawayOK;

            if ((jobOptions & JobOptions.AllowSilentBreakAway) != 0)
                basicLimits.BasicLimitInformation.LimitFlags |= Interop.Kernel32.JobObjectExtendedLimits.SilentBreakawayOK;

            if ((jobOptions & JobOptions.PreventWindowsErrorReporting) != 0)
                basicLimits.BasicLimitInformation.LimitFlags |= Interop.Kernel32.JobObjectExtendedLimits.DieOnUnhandledException;

            if ((jobOptions & JobOptions.TerminateProcessesWhenJobClosed) != 0)
                basicLimits.BasicLimitInformation.LimitFlags |= Interop.Kernel32.JobObjectExtendedLimits.KillOnJobClose;

            // end of time limit action...
            if ((jobOptions & JobOptions.TerminateAtTimeLimit) == 0)
                endInfo.EndOfJobTimeAction = 1;

            // cpu rate limits..
            if (CpuRate != null)
                CpuRate.GetLimits(out cpuLimits);

            // ui restrictions...
            JobUiRestrictions jobUiRestrictions = UiRestrictions;
            if ((jobUiRestrictions & JobUiRestrictions.Desktop) != 0)
                uiLimits.UIRestrictionsClass |= Interop.Kernel32.JobObjectUiRestrictions.Desktop;

            if ((jobUiRestrictions & JobUiRestrictions.DisplaySettings) != 0)
                uiLimits.UIRestrictionsClass |= Interop.Kernel32.JobObjectUiRestrictions.DisplaySettings;

            if ((jobUiRestrictions & JobUiRestrictions.ExitWindows) != 0)
                uiLimits.UIRestrictionsClass |= Interop.Kernel32.JobObjectUiRestrictions.ExitWindows;

            if ((jobUiRestrictions & JobUiRestrictions.GlobalAtoms) != 0)
                uiLimits.UIRestrictionsClass |= Interop.Kernel32.JobObjectUiRestrictions.GlobalAtoms;

            if ((jobUiRestrictions & JobUiRestrictions.Handles) != 0)
                uiLimits.UIRestrictionsClass |= Interop.Kernel32.JobObjectUiRestrictions.Handles;

            if ((jobUiRestrictions & JobUiRestrictions.ReadClipboard) != 0)
                uiLimits.UIRestrictionsClass |= Interop.Kernel32.JobObjectUiRestrictions.ReadClipboard;

            if ((jobUiRestrictions & JobUiRestrictions.SystemParameters) != 0)
                uiLimits.UIRestrictionsClass |= Interop.Kernel32.JobObjectUiRestrictions.SystemParameters;

            if ((jobUiRestrictions & JobUiRestrictions.WriteClipboard) != 0)
                uiLimits.UIRestrictionsClass |= Interop.Kernel32.JobObjectUiRestrictions.WriteClipboard;
        }

        internal static CpuLimit CalculateCpuLimit ( in Interop.Kernel32.JOBOBJECT_CPU_RATE_CONTROL_INFORMATION cpuInfo )
        {
            if ((cpuInfo.ControlFlags & Interop.Kernel32.JobObjectCpuControl.Enable) == 0)
                return null;

            switch (cpuInfo.ControlFlags & (Interop.Kernel32.JobObjectCpuControl.WeightBased | 
                                            Interop.Kernel32.JobObjectCpuControl.MinMaxRate))
            {
                case Interop.Kernel32.JobObjectCpuControl.None:
                    return new CpuRateLimit(cpuInfo);

                case Interop.Kernel32.JobObjectCpuControl.WeightBased:
                    return new CpuWeightedRateLimit(cpuInfo);

                case Interop.Kernel32.JobObjectCpuControl.MinMaxRate:
                    return new CpuMinMaxRateLimit(cpuInfo);

                default:
                    // the flags are invalid, we'll just pretend that it's not enabled..
                    return null;
            }
        }

        private static JobOptions CalculateJobOptions ( uint limitFlags )
        {
            JobOptions jobOptions = JobOptions.None;

            if ((limitFlags & Interop.Kernel32.JobObjectExtendedLimits.BreakawayOK) != 0)
                jobOptions |= JobOptions.AllowBreakAway;

            if ((limitFlags & Interop.Kernel32.JobObjectExtendedLimits.SilentBreakawayOK) != 0)
                jobOptions |= JobOptions.AllowSilentBreakAway;

            if ((limitFlags & Interop.Kernel32.JobObjectExtendedLimits.DieOnUnhandledException) != 0)
                jobOptions |= JobOptions.PreventWindowsErrorReporting;

            if ((limitFlags & Interop.Kernel32.JobObjectExtendedLimits.KillOnJobClose) != 0)
                jobOptions |= JobOptions.TerminateProcessesWhenJobClosed;

            return jobOptions;
        }

        private static JobUiRestrictions CalculateJobUiRestrictions ( uint uiRestrictions )
        {
            // NOTE: This could be implemented as a simple type case, since uiRestrictions
            // and JobUiRestrictions currently use the same bits for each flag. I'm assuming 
            // that this might change, so it's broken down into if/set statements..

            JobUiRestrictions jobUiRestrictions = JobUiRestrictions.None;

            if ((uiRestrictions & Interop.Kernel32.JobObjectUiRestrictions.Desktop) != 0)
                jobUiRestrictions |= JobUiRestrictions.Desktop;

            if ((uiRestrictions & Interop.Kernel32.JobObjectUiRestrictions.DisplaySettings) != 0)
                jobUiRestrictions |= JobUiRestrictions.DisplaySettings;

            if ((uiRestrictions & Interop.Kernel32.JobObjectUiRestrictions.ExitWindows) != 0)
                jobUiRestrictions |= JobUiRestrictions.ExitWindows;

            if ((uiRestrictions & Interop.Kernel32.JobObjectUiRestrictions.GlobalAtoms) != 0)
                jobUiRestrictions |= JobUiRestrictions.GlobalAtoms;

            if ((uiRestrictions & Interop.Kernel32.JobObjectUiRestrictions.Handles) != 0)
                jobUiRestrictions |= JobUiRestrictions.Handles;

            if ((uiRestrictions & Interop.Kernel32.JobObjectUiRestrictions.ReadClipboard) != 0)
                jobUiRestrictions |= JobUiRestrictions.ReadClipboard;

            if ((uiRestrictions & Interop.Kernel32.JobObjectUiRestrictions.SystemParameters) != 0)
                jobUiRestrictions |= JobUiRestrictions.SystemParameters;

            if ((uiRestrictions & Interop.Kernel32.JobObjectUiRestrictions.WriteClipboard) != 0)
                jobUiRestrictions |= JobUiRestrictions.WriteClipboard;

            return jobUiRestrictions;
        }

        /// <summary>
        /// A set of <see cref="JobOptions"/> that controls how the job operates.
        /// </summary>
        public JobOptions Options
        {
            get { return _jobOptions | (_notifyTimeLimit ? JobOptions.None : JobOptions.TerminateProcessesWhenJobClosed); }
            set
            {
                _jobOptions = value & ~(JobOptions.TerminateProcessesWhenJobClosed);
                _notifyTimeLimit = (value & JobOptions.TerminateProcessesWhenJobClosed) == 0;
            }
        }

        /// <summary>
        /// The per-job user mode execution time limit. The system adds the current time of the processes
        /// associated with the job to this limit. For example, if you set this to 1 minute and the job has
        /// a process that has accumulated 5 minutes of execution time, the enforced time limit is 6 minutes.
        /// </summary>
        /// <remarks>Assignments to this property set <see cref="PreserveTimeLimit"/> to <c>false</c>.</remarks>
        public TimeSpan? TimeLimit
        {
            get { return _timeLimit; }
            set { _timeLimit = value; _replaceTimeLimit = true; }
        }

        /// <summary>
        /// When <c>true</c>, any effective <see cref="TimeLimit"/> value is preserved. When <c>false</c>, the
        /// <see cref="TimeLimit"/> value replaces the in-effect limit.
        /// </summary>
        /// <remarks>Defaults to <c>true</c>. Assignments to <see cref="TimeLimit"/> set this property to <c>false</c>.</remarks>
        public bool PreserveTimeLimit
        {
            get { return !_replaceTimeLimit; }
            set { _replaceTimeLimit = !value; }
        }

        /// <summary>
        /// Per-process user mode execution time-limit.
        /// </summary>
        public TimeSpan? ProcessTimeLimit { get; set; }

        /// <summary>
        /// The minimum and maximum WorkingSet size for all processes in the job.
        /// </summary>
        public (ulong Minimum, ulong Maximum)? WorkingSet { get; set; }

        /// <summary>
        /// The maximum number of active processes associated with the job.
        /// </summary>
        public uint? ActiveProcesses { get; set; }

        /// <summary>
        /// The processor affinity for all processes associated with the job.
        /// </summary>
        public ulong? Affinity { get; set; }

        // PriorityClass & SchedulingClass ( // TODO: ??)

        /// <summary>
        /// The maximum amount of virtual memory committed to a process associated with the job. Attempts to commit
        /// memory beyond this point will fail.
        /// </summary>
        public ulong? MaximumProcessMemory { get; set; }

        /// <summary>
        /// The maximum amount of virtual memory committed to all processes associated with the job. Attempts to commit
        /// memory after this value is reached will fail.
        /// </summary>
        public ulong? MaximumMemory { get; set; }

        /// <summary>
        /// Limits to amount of CPU time processes associated with the job may use.
        /// </summary>
        public CpuLimit CpuRate { get; set; }

        /// <summary>
        /// Specifies UI restrictions to apply to processes associated with the job.
        /// </summary>
        public JobUiRestrictions UiRestrictions { get; set; }
    }
}
